using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventServe.EventStore
{
    public interface IEventStoreConnectionProvider
    {
        Task<IEventStoreConnection> GetConnection();
        Task<UserCredentials> GetCredentials();
    }

    public class EventStoreConnectionProvider : IEventStoreConnectionProvider
    {
        private readonly UserCredentials _credentials;
        private readonly string _host;
        private readonly int _port;
        private readonly bool _disableTls;
        private SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
        private IEventStoreConnection _conn;
        private bool _connected;

        public EventStoreConnectionProvider(IOptions<EventStoreConnectionOptions> options)
        {
            _credentials = new UserCredentials(options.Value.Username, options.Value.Password);
            _host = options.Value.Host;
            _port = options.Value.Port;
            _disableTls = options.Value.DisableTls;
        }

        public async Task<IEventStoreConnection> GetConnection()
        {
            if (_conn != null)
                return _conn;

            await _connectionSemaphore.WaitAsync();

            if(_conn != null)
            {
                _connectionSemaphore.Release();
                return _conn;
            }


            var settings = ConnectionSettings.Create()
                .SetDefaultUserCredentials(_credentials)
                .KeepReconnecting()
                .KeepRetrying()
                .EnableVerboseLogging()
                .UseDebugLogger();

            if (_disableTls)
                settings.DisableTls();

            _conn = EventStoreConnection.Create(settings, new Uri($"tcp://{_host}:{_port}"));
            _conn.Connected += _conn_Connected;
            await _conn.ConnectAsync();

            while(!_connected)
            {
                await Task.Delay(2000);
            }

            _connectionSemaphore.Release();
            return _conn;
        }

        private void _conn_Connected(object sender, ClientConnectionEventArgs e)
        {
            _connected = true;
        }

        public Task<UserCredentials> GetCredentials()
        {
            return Task.FromResult(_credentials);
        }
    }
}

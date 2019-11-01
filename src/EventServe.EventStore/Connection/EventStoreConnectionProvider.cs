using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace EventServe.EventStore
{
    public interface IEventStoreConnectionProvider
    {
        IEventStoreConnection GetConnection();
        Task<UserCredentials> GetCredentials();
    }

    public class EventStoreConnectionProvider : IEventStoreConnectionProvider
    {
        private readonly UserCredentials _credentials;
        private readonly string _host;
        private readonly int _port;

        public EventStoreConnectionProvider(IOptions<EventStoreConnectionOptions> options)
        {
            _credentials = new UserCredentials(options.Value.Username, options.Value.Password);
            _host = options.Value.Host;
            _port = options.Value.Port;
        }

        public IEventStoreConnection GetConnection()
        {
            var settings = ConnectionSettings.Create();
            settings.SetDefaultUserCredentials(_credentials);

            var conn = EventStoreConnection.Create(settings, new Uri($"tcp://{_host}:{_port}"));
            return conn;
        }

        public Task<UserCredentials> GetCredentials()
        {
            return Task.FromResult(_credentials);
        }
    }
}

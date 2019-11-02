using Docker.DotNet;
using Docker.DotNet.Models;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{
    public class EmbeddedEventStoreFixture : IAsyncLifetime
    {
        const string EventStoreImage = "eventstore/eventstore";

        private readonly string _containerName;
        private readonly DockerClient _dockerClient;

        private bool _connected;
        private IEventStoreConnection _conn;
        public EventStoreConnectionOptions EventStoreConnectionOptions { get; private set; }

        public EmbeddedEventStoreFixture()
        {
            var random = new Random();
            EventStoreConnectionOptions = new EventStoreConnectionOptions
            {
                Host = "localhost",
                Port = random.Next(1114, 2111),
                Username = "admin",
                Password = "changeit"
            };

            _containerName = "es" + Guid.NewGuid().ToString("N");

            var address = Environment.OSVersion.Platform == PlatformID.Unix
                ? new Uri("unix:///var/run/docker.sock")
                : new Uri("npipe://./pipe/docker_engine");
            var config = new DockerClientConfiguration(address);
            _dockerClient = config.CreateClient();
        }



        public async Task InitializeAsync()
        {
            
            var images = await _dockerClient.Images.ListImagesAsync(new ImagesListParameters { MatchName = EventStoreImage });
            if (images.Count == 0)
            {
                // No image found. Pulling latest ..
                Console.WriteLine("[docker] no image found - pulling latest");
                await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = EventStoreImage, Tag = "latest" }, null, IgnoreProgress.Forever);
            }
            Console.WriteLine("[docker] creating container " + _containerName);
            //Create container ...
            await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = EventStoreImage,
                    Name = _containerName,
                    Tty = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                "2113/tcp",
                                new List<PortBinding> {
                                    new PortBinding
                                    {
                                        HostPort = "2113"
                                    }
                                }
                            },
                            {
                                "1113/tcp",
                                new List<PortBinding> {
                                    new PortBinding
                                    {
                                        HostPort = EventStoreConnectionOptions.Port.ToString()
                                    }
                                }
                            }
                        }
                    }
                });
            // Starting the container ...
            Console.WriteLine("[docker] starting container " + _containerName);
            await _dockerClient.Containers.StartContainerAsync(_containerName, new ContainerStartParameters { });
            var endpoint = new Uri($"tcp://127.0.0.1:{EventStoreConnectionOptions.Port}");
            var settings = ConnectionSettings
                .Create()
                .KeepReconnecting()
                .KeepRetrying()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));
            var connectionName = $"M={Environment.MachineName},P={Process.GetCurrentProcess().Id},T={DateTimeOffset.UtcNow.Ticks}";
            _conn = EventStoreConnection.Create(settings, endpoint, connectionName);

            _conn.Connected += Connection_Connected;

            Console.WriteLine("[docker] connecting to eventstore");
            await _conn.ConnectAsync();

            while (!_connected)
                await Task.Delay(250);

        }

        private void Connection_Connected(object sender, ClientConnectionEventArgs e)
        {
            _connected = true;
        }

        public async Task DisposeAsync()
        {
            if (_dockerClient != null)
            {
                _conn?.Dispose();
                Console.WriteLine("[docker] stopping container " + _containerName);
                await _dockerClient.Containers.StopContainerAsync(_containerName, new ContainerStopParameters { });
                Console.WriteLine("[docker] removing container " + _containerName);
                await _dockerClient.Containers.RemoveContainerAsync(_containerName, new ContainerRemoveParameters { Force = true });
                _dockerClient.Dispose();
            }
        }


        private class IgnoreProgress : IProgress<JSONMessage>
        {
            public static readonly IProgress<JSONMessage> Forever = new IgnoreProgress();

            public void Report(JSONMessage value) { }
        }
    }
}

using Docker.DotNet;
using Docker.DotNet.Models;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
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
        private int _suffix;
        private int _prefix;
        private bool _connected;

        public EmbeddedEventStoreFixture()
        {
            EventStoreContainer = "es" + Guid.NewGuid().ToString("N");
        }

        private string EventStoreContainer { get; set; }

        //public StreamName NextStreamName()
        //{
        //    return new StreamName($"stream-{Interlocked.Increment(ref _suffix)}");
        //}

        //public string NextStreamNamePrefix()
        //{
        //    return $"scenario-{Interlocked.Increment(ref _prefix):D}-";
        //}

        public IEventStoreConnection Connection { get; private set; }

        const string EventStoreImage = "eventstore/eventstore";

        public async Task InitializeAsync()
        {
            var address = Environment.OSVersion.Platform == PlatformID.Unix
                ? new Uri("unix:///var/run/docker.sock")
                : new Uri("npipe://./pipe/docker_engine");
            var config = new DockerClientConfiguration(address);
            this.Client = config.CreateClient();
            var images = await this.Client.Images.ListImagesAsync(new ImagesListParameters { MatchName = EventStoreImage });
            if (images.Count == 0)
            {
                // No image found. Pulling latest ..
                Console.WriteLine("[docker] no image found - pulling latest");
                await this.Client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = EventStoreImage, Tag = "latest" }, null, IgnoreProgress.Forever);
            }
            Console.WriteLine("[docker] creating container " + EventStoreContainer);
            //Create container ...
            await this.Client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = EventStoreImage,
                    Name = EventStoreContainer,
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
                                        HostPort = "1113"
                                    }
                                }
                            }
                        }
                    }
                });
            // Starting the container ...
            Console.WriteLine("[docker] starting container " + EventStoreContainer);
            await this.Client.Containers.StartContainerAsync(EventStoreContainer, new ContainerStartParameters { });
            var endpoint = new Uri("tcp://127.0.0.1:1113");
            var settings = ConnectionSettings
                .Create()
                .KeepReconnecting()
                .KeepRetrying()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));
            var connectionName = $"M={Environment.MachineName},P={Process.GetCurrentProcess().Id},T={DateTimeOffset.UtcNow.Ticks}";
            this.Connection = EventStoreConnection.Create(settings, endpoint, connectionName);

            this.Connection.Connected += Connection_Connected;

            Console.WriteLine("[docker] connecting to eventstore");
            await this.Connection.ConnectAsync();


            while (!_connected)
                await Task.Delay(250);


        }

        private void Connection_Connected(object sender, ClientConnectionEventArgs e)
        {
            _connected = true;
        }

        public async Task DisposeAsync()
        {
            if (this.Client != null)
            {
                this.Connection?.Dispose();
                Console.WriteLine("[docker] stopping container " + EventStoreContainer);
                await this.Client.Containers.StopContainerAsync(EventStoreContainer, new ContainerStopParameters { });
                Console.WriteLine("[docker] removing container " + EventStoreContainer);
                await this.Client.Containers.RemoveContainerAsync(EventStoreContainer, new ContainerRemoveParameters { Force = true });
                this.Client.Dispose();
            }
        }

        private DockerClient Client { get; set; }

        private class IgnoreProgress : IProgress<JSONMessage>
        {
            public static readonly IProgress<JSONMessage> Forever = new IgnoreProgress();

            public void Report(JSONMessage value) { }
        }
    }
}

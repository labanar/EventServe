using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Options;
using Xunit;

namespace EventServe.EventStore.IntegrationTests {

    public class EventStoreContainerReference
    {
        public string ContainerName { get; set; }
        public string VolumeName { get; set; }
        public int Port { get; set; }
    }

    public class SandboxedEventStoreConnection : IAsyncDisposable
    {
        private readonly EventStoreContainerReference _containerReference;
        private readonly EmbeddedEventStoreFixture _fixture;
        private readonly IEventStoreConnection _connection;
        public IEventStoreConnectionProvider ConnectionProvider { get; private set; }

        public SandboxedEventStoreConnection(EmbeddedEventStoreFixture fixture, EventStoreContainerReference containerReference)
        {
            _containerReference = containerReference;
            _fixture = fixture;

            var endpoint = new Uri($"tcp://127.0.0.1:{containerReference.Port}");
            var settings = ConnectionSettings.Create()
                .KeepReconnecting()
                .KeepRetrying()
                .DisableTls()
                .SetDefaultUserCredentials(new UserCredentials("admin", "changeit"));
            var connectionName = $"M={Environment.MachineName},P={Process.GetCurrentProcess().Id},T={DateTimeOffset.UtcNow.Ticks}";
            _connection = EventStoreConnection.Create(settings, endpoint, connectionName);

            ConnectionProvider = new EventStoreConnectionProvider(Options.Create(new EventStoreConnectionOptions
            {
                Host = "127.0.0.1",
                Port = _containerReference.Port,
                Username = "admin",
                Password = "changeit"
            }));
        }

        public async ValueTask DisposeAsync()
        {
            await _fixture.DestroyEventStoreContainer(_containerReference);
        }
    }

    public class EmbeddedEventStoreFixture : IAsyncLifetime {

        private readonly DockerClient _dockerClient;
        const string EVENTSTORE_SEARCH_IMAGE_NAME = "eventstore/eventstore";
        const string EVENTSTORE_SEARCH_IMAGE_TAG = "20.10.4-buster-slim";

        private List<string> _containersToCleanUp = new List<string>();
        private List<string> _volumesToCleanUp = new List<string>();


        public EmbeddedEventStoreFixture() {
            var address = Environment.OSVersion.Platform == PlatformID.Unix ?
                new Uri("unix:///var/run/docker.sock") :
                new Uri("npipe://./pipe/docker_engine");
            var config = new DockerClientConfiguration(address);
            _dockerClient = config.CreateClient();
        }

        public async Task InitializeAsync() {}
        public async Task DisposeAsync() { }

        public async Task<SandboxedEventStoreConnection> CreateEventStoreSandbox()
        {
            var containerReference = await CreateEventStoreContainer();
            _containersToCleanUp.Add(containerReference.ContainerName);
            _volumesToCleanUp.Add(containerReference.VolumeName);
            return new SandboxedEventStoreConnection(this, containerReference);
        }

        private async Task<EventStoreContainerReference> CreateEventStoreContainer()
        {
            var id = Guid.NewGuid();
            var containerReference = new EventStoreContainerReference
            {
                ContainerName = $"eventserve-tests-{id}",
                VolumeName = $"eventserve-tests-{id}-data",
                Port = GetRandomUnusedPort(),
            };

            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = EVENTSTORE_SEARCH_IMAGE_NAME,
                    Tag = EVENTSTORE_SEARCH_IMAGE_TAG,
                },
                null,
                new Progress<JSONMessage>());


            var volumeCreateResponse = await _dockerClient.Volumes.CreateAsync(new VolumesCreateParameters
            {
                Name = containerReference.VolumeName,
                Driver = "local"
            });

            var containerResp = await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = $"{EVENTSTORE_SEARCH_IMAGE_NAME}:{EVENTSTORE_SEARCH_IMAGE_TAG}",
                    Name = containerReference.ContainerName,
                    Tty = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>> 
                        {
                            {
                                "1113/tcp",
                                new List<PortBinding> {
                                    new PortBinding {
                                        HostPort = containerReference.Port.ToString()
                                    }
                                }
                            }
                        }
                    },
                    Env = new List<string>
                    {
                        "EVENTSTORE_INSECURE=true",
                        "EVENTSTORE_INT_TCP_HEARTBEAT_INTERVAL=60000",
                        "EVENTSTORE_ENABLE_EXTERNAL_TCP=true",
                        "EVENTSTORE_RUN_PROJECTIONS=All",
                        "EVENTSTORE_START_STANDARD_PROJECTIONS=true",
                        "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP=true",
                        "EVENTSTORE_CLUSTER_SIZE=1"
                    }
                });

            await _dockerClient.Containers.StartContainerAsync(containerReference.ContainerName, new ContainerStartParameters { });
            return containerReference;
        }

        public async Task DestroyEventStoreContainer(EventStoreContainerReference containerReference)
        {
            Debug.WriteLine("[Docker] Stopping container " + containerReference.ContainerName);
            await TryStopContainer(containerReference.ContainerName);
            Debug.WriteLine("[Docker] Removing container " + containerReference.ContainerName);
            await TryRemoveContainer(containerReference.ContainerName);
            Debug.WriteLine("[Docker] Removing volume " + containerReference.VolumeName);
            await TryRemoveVolume(containerReference.VolumeName);
        }

        private async Task<bool> TryStopContainer(string containerName)
        {
            try
            {
                await _dockerClient.Containers.StopContainerAsync(containerName, new ContainerStopParameters { });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryRemoveContainer(string containerName)
        {
            try
            {
                await _dockerClient.Containers.RemoveContainerAsync(containerName, new ContainerRemoveParameters { Force = true });
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TryRemoveVolume(string volumeName)
        {
            try
            {
                await _dockerClient.Volumes.RemoveAsync(volumeName, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0) { ExclusiveAddressUse = true};
            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}
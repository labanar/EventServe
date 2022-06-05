using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using EventServe.SqlStreamStore;
using EventServe.SqlStreamStore.MsSql;
using Microsoft.EntityFrameworkCore;
using SqlStreamStore;
using Xunit;

namespace EventServe.EventStore.IntegrationTests
{

    public class MsSqlContainerReference
    {
        public string ContainerName { get; set; }
        public string VolumeName { get; set; }
        public string SaPassword { get; set; }
        public int Port { get; set; }
    }

    public class MsSqlSandbox : IAsyncDisposable
    {
        public string ConnectionString { get; private set; }

        private readonly MsSqlContainerReference _containerReference;
        private readonly SqlStreamStoreFixture _fixture;

        public MsSqlSandbox(SqlStreamStoreFixture fixture, MsSqlContainerReference containerReference)
        {
            _containerReference = containerReference;
            _fixture = fixture;
            ConnectionString = $"Server=localhost,{containerReference.Port};Database=sss-mssql;User Id=sa;Password={containerReference.SaPassword};MultipleActiveResultSets=True;";
        }

        public async ValueTask DisposeAsync()
        {
            await _fixture.DestroyMsSqlContainer(_containerReference);
        }
    }

    public class SqlStreamStoreFixture : IAsyncLifetime
    {
        private readonly DockerClient _dockerClient;
        const string SQL_SERVER_IMAGE_NAME = "mcr.microsoft.com/mssql/server";
        const string SQL_SERVER_IMAGE_TAG = "2022-latest";

        private List<string> _containersToCleanUp = new List<string>();
        private List<string> _volumesToCleanUp = new List<string>();


        public SqlStreamStoreFixture()
        {
            var address = Environment.OSVersion.Platform == PlatformID.Unix ?
                new Uri("unix:///var/run/docker.sock") :
                new Uri("npipe://./pipe/docker_engine");
            var config = new DockerClientConfiguration(address);
            _dockerClient = config.CreateClient();
        }

        public async Task InitializeAsync() { }
        public async Task DisposeAsync() { }

        public async Task<MsSqlSandbox> CreateMsSqlSandbox()
        {
            var containerReference = await CreateMsSqlContainer();
            _containersToCleanUp.Add(containerReference.ContainerName);
            _volumesToCleanUp.Add(containerReference.VolumeName);
            return new MsSqlSandbox(this, containerReference);
        }

        private async Task<MsSqlContainerReference> CreateMsSqlContainer()
        {
            var id = Guid.NewGuid();
            var containerReference = new MsSqlContainerReference
            {
                ContainerName = $"eventserve-tests-{id}",
                VolumeName = $"eventserve-tests-{id}-data",
                Port = GetRandomUnusedPort(),
                SaPassword = "yourStrong1(!)Password"
            };

            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = SQL_SERVER_IMAGE_NAME,
                    Tag = SQL_SERVER_IMAGE_TAG,
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
                    Image = $"{SQL_SERVER_IMAGE_NAME}:{SQL_SERVER_IMAGE_TAG}",
                    Name = containerReference.ContainerName,
                    Tty = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            "1433/tcp",
                            new List<PortBinding> {
                                new PortBinding {
                                    HostPort = containerReference.Port.ToString()
                                }
                            }
                        }
                    },
                    },
                    Env = new List<string>
                    {
                        $"SA_PASSWORD={containerReference.SaPassword}",
                        "ACCEPT_EULA=Y"
                    }
                });

            await _dockerClient.Containers.StartContainerAsync(containerReference.ContainerName, new ContainerStartParameters { });

            var connectionString = $"Server=localhost,{containerReference.Port};Database=sss-mssql;User Id=sa;Password={containerReference.SaPassword};MultipleActiveResultSets=True;";
            var options = new DbContextOptionsBuilder<SqlStreamStoreContext>()
            .UseSqlServer(
                connectionString,
                options =>
                {
                    options.MigrationsAssembly(typeof(MsSqlStreamStoreOptions).Assembly.FullName);
                    options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })
            .Options;

            var context = new SqlStreamStoreContext(options);
            await context.Database.MigrateAsync();

            //Migrate the SSS context
            using var store = new MsSqlStreamStoreV3(new MsSqlStreamStoreV3Settings(connectionString)
            {
                Schema = "TestSchema"
            });
            await store.CreateSchemaIfNotExists();
            await store.CheckSchema();

            return containerReference;
        }

        public async Task DestroyMsSqlContainer(MsSqlContainerReference containerReference)
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
            var listener = new TcpListener(IPAddress.Loopback, 0) { ExclusiveAddressUse = true };
            listener.Start();

            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            return port;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SqlStreamStore;
using Xunit;

namespace EventServe.SqlStreamStore.MsSql.IntegrationTests
{
    public class EmbeddedMsSqlStreamStoreFixture : IAsyncLifetime
    {
        const string MsSqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

        private readonly string _containerName;
        private readonly int _port;
        private readonly string _saPass;
        private readonly DockerClient _dockerClient;

        public int HostPort => _port;
        public string AdminPassword => _saPass;
        public string ConnectionString => $"Server=localhost,{_port};Database=sss-mssql;User Id=sa;Password={_saPass};MultipleActiveResultSets=True;";

        public EmbeddedMsSqlStreamStoreFixture()
        {
            var random = new Random();
            _containerName = "mssql" + Guid.NewGuid().ToString("N");
            _port = random.Next(1435, 1599);
            _saPass = "StR0nGP4sSW0rD";

            var address = Environment.OSVersion.Platform == PlatformID.Unix ?
                new Uri("unix:///var/run/docker.sock") :
                new Uri("npipe://./pipe/docker_engine");
            var config = new DockerClientConfiguration(address);
            _dockerClient = config.CreateClient();
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("[Docker] Creating container " + _containerName);
            //Create container ...
            await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Image = MsSqlServerImage,
                    Name = _containerName,
                    Tty = true,
                    HostConfig = new HostConfig
                    {
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                "1433/tcp",
                                new List<PortBinding> {
                                    new PortBinding {
                                        HostPort = _port.ToString()
                                    }
                                }
                            }
                        },
                    },   
                    Env = new List<string>
                    {
                        $"SA_PASSWORD={_saPass}",
                        "ACCEPT_EULA=Y"
                    }
                });
            // Starting the container ...
            Console.WriteLine("[Docker] Starting container " + _containerName);

            await _dockerClient.Containers.StartContainerAsync(
                _containerName, 
                new ContainerStartParameters { });

           
            var options = new DbContextOptionsBuilder<SqlStreamStoreContext>()
                .UseSqlServer(ConnectionString, options => 
                {
                    options.MigrationsAssembly(typeof(MsSqlStreamStoreOptions).Assembly.FullName);
                    options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                })
                .Options;

            var context = new SqlStreamStoreContext(options);
            context.Database.Migrate();

            //Migrate the SSS context
            using var store = new MsSqlStreamStoreV3(new MsSqlStreamStoreV3Settings(ConnectionString)
            {
                Schema = "TestSchema"
            });
            await store.CreateSchemaIfNotExists();
            await store.CheckSchema();
        }

        public async Task DisposeAsync()
        {
            if (_dockerClient != null)
            {
                Console.WriteLine("[Docker] Stopping container " + _containerName);
                await _dockerClient.Containers.StopContainerAsync(_containerName, new ContainerStopParameters { });
                Console.WriteLine("[Docker] Removing container " + _containerName);
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
namespace QarnotSDK.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Threading;
    using Docker.DotNet;
    using Docker.DotNet.BasicAuth;
    using Docker.DotNet.Models;

    public class StorageClusterContainer : IDisposable
    {
        private string StorageClusterContainerID;
        private const string ImageName = "ceph-s3-box";
        private const string DefaultRelease = "tentacle";
        private const string ContainerPort = "7480/tcp";
        private readonly string FullImageName;
        private readonly DockerClient DockerClient;
        private const string DockerSocketPath = "unix:///var/run/docker.sock";

        public StorageClusterContainer()
        {
            var release = Environment.GetEnvironmentVariable("INTEGRATION_TEST_CEPH_RELEASE") ?? DefaultRelease;
            FullImageName = $"{ImageName}:{release}";

            var dockerUser = Environment.GetEnvironmentVariable("INTEGRATION_TEST_DOCKER_USERNAME");
            var dockerPwd = Environment.GetEnvironmentVariable("INTEGRATION_TEST_DOCKER_PASSWORD");
            if (!string.IsNullOrWhiteSpace(dockerUser))
            {
                using var credentials = new BasicAuthCredentials(dockerUser, dockerPwd);
                using var dockerConfig = new DockerClientConfiguration(new Uri(DockerSocketPath), credentials);
                DockerClient = dockerConfig.CreateClient();
            }
            else
            {
                Console.WriteLine("WARNING: Missing Docker Credentials (use INTEGRATION_TEST_DOCKER_USERNAME and INTEGRATION_TEST_DOCKER_PASSWORD to set them)");
                using var dockerConfig = new DockerClientConfiguration(new Uri(DockerSocketPath));
                DockerClient = dockerConfig.CreateClient();
            }
        }

        private async Task<string> GenerateStorageClusterContainerAsync(CancellationToken ct)
        {
            var images = await DockerClient.Images.ListImagesAsync(
                new ImagesListParameters()
                {
                    All = true,
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                        { ["reference"] = new Dictionary<string, bool> { [FullImageName] = true } }
                },
                ct);

            if (images.Count == 0)
            {
                var msg = $"No image {FullImageName} found on host, please run container/build.sh before launching tests";
                Console.Error.WriteLine(msg);
                Environment.FailFast(msg);
            }

            var accessKey = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_ADMIN_ACCESS_KEY") ?? "access";
            var secretKey = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_ADMIN_SECRET_KEY") ?? "secret";

            var storageContainer = await DockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters()
                {
                    Name = $"sdk-csharp-test-ceph-cluster",
                    Image = FullImageName,
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        { ContainerPort, default }
                    },
                    Env = new List<string>
                    {
                        $"ACCESS_KEY={accessKey}",
                        $"SECRET_KEY={secretKey}",
                    },
                    HostConfig = new HostConfig
                    {
                        NetworkMode = "bridge",
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                ContainerPort,
                                new List<PortBinding>
                                {
                                    new PortBinding
                                    {
                                        HostIP = "127.0.0.1",
                                        HostPort = ContainerPort,
                                    },
                                }
                            }
                        },
                    },
                },
                ct);
            return storageContainer.ID;
        }

        public async Task StartAsync(CancellationToken ct = default)
        {
            var existingContainer = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_IP");
            if (existingContainer != default)
            {
                var cephContainers = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters()
                {
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["name"] = new Dictionary<string, bool>
                        {
                            [existingContainer] = true
                        }
                    },
                    All = true
                },
                ct);
                var container = cephContainers.FirstOrDefault();
                StorageClusterContainerID = container?.ID;
            }

            if (StorageClusterContainerID == default)
            {
                StorageClusterContainerID = await GenerateStorageClusterContainerAsync(ct);
                await DockerClient
                    .Containers
                    .StartContainerAsync(
                        StorageClusterContainerID,
                        new ContainerStartParameters(),
                        ct);
            }

            // Poll the RGW S3 endpoint until it responds. Any HTTP response (including
            // 403 AccessDenied for unsigned requests) means RadosGW is up and ready.
            using var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) };
            while (true)
            {
                await Task.Delay(4000, ct);
                try
                {
                    await httpClient.GetAsync("http://127.0.0.1:7480/", ct);
                    break;
                }
                catch (Exception)
                {
                    // RGW not ready yet, keep polling
                }
            }
        }

        public async Task<string> GetIPAddress(CancellationToken ct = default)
        {
            var inspectContainer = await DockerClient.Containers.InspectContainerAsync(StorageClusterContainerID, ct);
            return inspectContainer.NetworkSettings.IPAddress;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StorageClusterContainer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(StorageClusterContainerID))
                    {
                        DockerClient.Containers.RemoveContainerAsync(
                            StorageClusterContainerID,
                            new ContainerRemoveParameters()
                            {
                                Force = true,
                            }).GetAwaiter().GetResult();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("could not clean container" + e.Message);
                }

                DockerClient.Dispose();
            }
        }
    }
}

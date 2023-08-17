namespace QarnotSDK.IntegrationTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using Docker.DotNet;
    using Docker.DotNet.BasicAuth;
    using Docker.DotNet.Models;

    public class StorageClusterContainer : IDisposable
    {
        private string StorageClusterContainerID;
        //private const string CephNanoLocalImageName = "ceph/daemon";
        private const string CephNanoLocalImageName = "ceph-nano";
        private const string CephNanoLocalImageTag = "latest";
        private const string CephNanoFullImageName = $"{CephNanoLocalImageName}:{CephNanoLocalImageTag}";
        private const string StorageClusterContainerPort = "8000/tcp";
        private const string StorageClusterHostPort = "8000/tcp";
        private readonly DockerClient DockerClient;
        private const string DockerSocketPath = "unix:///var/run/docker.sock";

        public StorageClusterContainer()
        {

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
                        { ["reference"] = new Dictionary<string, bool> { [CephNanoFullImageName] = true } }
                },
                ct);

            if (images.Count == 0)
            {
                var msg = $"No image {CephNanoLocalImageName} found on host, please build it before launching test";
                Console.Error.WriteLine(msg);
                Environment.FailFast(msg);
            }

            var storageContainer = await DockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters()
                {
                    Name = "sdk-csharp-test-ceph-cluster",
                    Image = CephNanoFullImageName,
                    ExposedPorts = new Dictionary<string, EmptyStruct>
                    {
                        { StorageClusterContainerPort, default }
                    },
                    Env = new List<string>
                    {
                        "CEPH_DEMO_UID=rgw",
                        "CEPH_DEMO_ACCESS_KEY=${STORAGE_ADMIN_ACCESS:-access}",
                        "CEPH_DEMO_SECRET_KEY=${STORAGE_ADMIN_SECRET:-secret}"
                    },
                    HostConfig = new HostConfig
                    {
                        NetworkMode = "bridge",
                        PortBindings = new Dictionary<string, IList<PortBinding>>
                        {
                            {
                                StorageClusterContainerPort,
                                new List<PortBinding>
                                {
                                    new PortBinding
                                    {
                                        HostIP = "127.0.0.1",
                                        HostPort = StorageClusterHostPort,
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

            string log = string.Empty;
            var messageToWait = "Running on http://0.0.0.0"; // By default it is http://0.0.0.0:5000 but the default from docker.redmont image is http://0.0.0.0:5001
            while(!log.Contains(messageToWait, StringComparison.CurrentCultureIgnoreCase))
            {
                await Task.Delay(4000, ct);
                using var multiplexedStream = await DockerClient
                    .Containers
                    .GetContainerLogsAsync(
                        StorageClusterContainerID,
                        true,
                        new ContainerLogsParameters()
                        {
                            ShowStdout = true, ShowStderr = true, Tail = "50",
                        },
                        ct);

                using var stdin = new MemoryStream();
                using var stdout = new MemoryStream();
                using var stderr = new MemoryStream();
                await multiplexedStream.CopyOutputToAsync(stdin, stdout, stderr, ct);
                stdout.Seek(0, SeekOrigin.Begin);
                using var stdoutReader = new StreamReader(stdout);
                log = await stdoutReader.ReadToEndAsync();
            }

            var createAdminUserExec = await DockerClient
                .Exec
                .ExecCreateContainerAsync(
                    StorageClusterContainerID,
                    new ContainerExecCreateParameters()
                    {
                        Cmd = new List<string>()
                        {
                            "/usr/bin/radosgw-admin",
                            "user",
                            "create",
                            "--tenant=rgw",
                            "--uid=rgw",
                            "--display-name=rgw",
                            "--admin",
                            "--key-type=s3",
                            "--secret-key=secret",
                            "--access-key=access",
                        }
                    },
                    ct);
            using var _ = await DockerClient
                .Exec
                .StartWithConfigContainerExecAsync(
                    createAdminUserExec.ID,
                    new ContainerExecStartParameters(),
                    ct);

            await Task.Delay(4000, ct);

            var giveUserAdminPermissionsExec = await DockerClient
                .Exec
                .ExecCreateContainerAsync(
                    StorageClusterContainerID,
                    new ContainerExecCreateParameters()
                    {
                        Cmd = new List<string>()
                        {
                            "/usr/bin/radosgw-admin",
                            "caps",
                            "add",
                            "--uid=rgw",
                            "--tenant=rgw",
                            "--caps='users=*;buckets=*;usage=*'"
                        }
                    },
                    ct);
            using var __ = await DockerClient
                .Exec
                .StartWithConfigContainerExecAsync(
                    giveUserAdminPermissionsExec.ID,
                    new ContainerExecStartParameters(),
                    ct);

            await Task.Delay(2000, ct);
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

namespace QarnotSDK.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [SetUpFixture]
    public class StorageClusterSetUp
    {
        private StorageClusterContainer StorageClusterContainer;

        [OneTimeSetUp]
        public async Task SetUpStorageCluster()
        {
            if (!Gitlabci.IsInGitlabCI() && IntegrationTestContext.IsUsingRealStorage()) // All code using Docker.Dotnet cannot be run in gitlab-ci because docker daemon is not available to make requests
            {
                StorageClusterContainer = new StorageClusterContainer();
                await StorageClusterContainer.StartAsync();
                var cephContainerIP = await StorageClusterContainer.GetIPAddress();
                Environment.SetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_IP", cephContainerIP); // local container usually does not share same network as the tests so use IP address instead of container name
            }
        }

        [OneTimeTearDown]
        public void DisposeContainers()
        {
            StorageClusterContainer?.Dispose();
        }
    }
}

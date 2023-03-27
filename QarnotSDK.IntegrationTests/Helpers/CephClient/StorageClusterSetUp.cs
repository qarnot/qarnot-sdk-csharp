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
            Environment.SetEnvironmentVariable("QARNOT_SDK_CSHARP_TEST_USE_REAL_REMOTE_STORAGE", "true");
            var useRealStorageCluster = bool.Parse(Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TEST_USE_REAL_REMOTE_STORAGE") ?? "false");

            if (useRealStorageCluster)
            {
                StorageClusterContainer = new StorageClusterContainer();
                await StorageClusterContainer.StartAsync();
            }
        }

        [OneTimeTearDown]
        public void DisposeContainers()
        {
            StorageClusterContainer?.Dispose();
        }
    }
}

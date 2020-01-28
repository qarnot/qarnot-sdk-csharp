namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Reflection;
    using NUnit.Framework;
    using Amazon.Runtime;
    using Amazon.S3;
    using QarnotSDK;
    using QarnotSDK.Internal;

    [TestFixture]
    public class ConnectionExtensionTests
    {
        [Test]
        public async Task EnsureGetConfiguredS3ClientAsyncRetrieveTheCorrectClient()
        {
            var token = "132456789";
            var storageAccess = "987465133";
            var storageUrl = "https://thestorage.qarnot.com";
            var computeUrl = "https://thecompute.qarnot.com";
            var forcePathStyle = true;
            var connection = new Connection(
                uri: computeUrl,
                storageUri: storageUrl,
                token: token,
                forceStoragePathStyle: forcePathStyle
            );
            connection.StorageAccessKey = storageAccess;

            var s3Client = await connection.GetConfiguredS3ClientAsync();
            Assert.AreEqual(new Uri(s3Client.Config.ServiceURL), new Uri(storageUrl));

            // Use reflection to check that the s3client is well configured
            var field= typeof (AmazonS3Client).GetProperty(
                "Credentials",
                BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance);

            var credentials = (AWSCredentials)field.GetValue(s3Client);
            var immutableCredentials = credentials.GetCredentials();

            Assert.AreEqual(storageAccess, immutableCredentials.AccessKey);
            Assert.AreEqual(token, immutableCredentials.SecretKey);
        }
    }
}
namespace QarnotSDK.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.Runtime.Internal;
    using Amazon.S3;
    using Amazon.S3.Model;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    [Category("Bucket")]
    public class QBucketIT
    {
        private AmazonS3Client _storageClient { get; set; }
        private Connection _connection { get; set; }
        private string InitBucketName { get; set; }
        private int BucketsInitCount { get; set; }
        private List<(string name, string content)> InitFiles { get; set; }

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [OneTimeSetUp]
        [Timeout(5000)]
        public async Task CommonSetup()
        {
            IntegrationTestContext.IgnoreIfNotRealStorage();

            Amazon.AWSConfigs.LoggingConfig.LogTo = Amazon.LoggingOptions.Console;
            var cephIP = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_IP") ?? "http://127.0.0.1";
            if (!cephIP.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
            {
                cephIP = string.Format("http://{0}", cephIP); // RadosGW client cannot parse IP or container name as URI so need to add the http prefix to the endpoint
            }
            var adminAccessKey = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_ADMIN_ACCESS_KEY") ?? "access";
            var adminSecretKey = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_ADMIN_SECRET_KEY") ?? "secret";
            var cephClientOptions = new CephClientOptions($"{cephIP}:8000", adminAccessKey, adminSecretKey);
            TestContext.Progress.WriteLine("Ceph Config: endpoint = {0}, accessKey = {1}, secretKey = {2}", cephClientOptions.RadosGWEndpoint, cephClientOptions.RadosGWAdminAccessKey, cephClientOptions.RadosGWAdminSecretKey);
            var context = new IntegrationTestContext(cephClientOptions);
            await context.ResetStorageAsync();

            // Create user
            var accessKey = "testAccess";
            var secretKey = "testSecret";
            await context.CreateStorageAccount("UserUuid", "tenant", "test-user", accessKey, secretKey);

            // HttpHandler = new InterceptingFakeHttpHandler(); // used when debugging to intercept requests to api and storage
            _storageClient = context.GetUserStorageClient(accessKey, secretKey, HttpHandler);

            _connection = new Connection(
                uri: "https://127.0.0.1/",
                storageUri: cephClientOptions.RadosGWEndpoint,
                token: secretKey,
                httpClientHandler: HttpHandler,
                forceStoragePathStyle: true
                )
                {
                    StorageAccessKey = accessKey,
                    StorageSecretKey = secretKey,
                    StorageUploadPartSize = 0,
                    //S3HttpClientFactory = new FakeS3HttpClientFactory(HttpHandler),
                };
        }

        [SetUp]
        public async Task SpecificSetup()
        {
            InitFiles = new ();
            var random = new Random();
            var suffix = random.Next();
            InitBucketName = $"existingBucket{suffix}";
            await _storageClient.PutBucketAsync(InitBucketName);
            var userBuckets = await _storageClient.ListBucketsAsync();
            BucketsInitCount = userBuckets.Buckets.Count;

            var directory = $"bucketDirectory{suffix}";
            Directory.CreateDirectory(directory);
            var filePath = $"{directory}/initFile";
            var contentString = "someContent";
            using (var stream = File.Create(filePath))
            {
                var content = new UTF8Encoding(true).GetBytes(contentString);
                stream.Write(content);
            }
            var __ = await _storageClient.PutObjectAsync(new(){BucketName=InitBucketName, Key = filePath, FilePath = filePath});
            InitFiles.Add((filePath, contentString));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(""))) {
                var secondSubFile = $"{directory}/otherFile";
                var _ = await _storageClient.PutObjectAsync(new(){BucketName=InitBucketName, Key = secondSubFile, InputStream = stream});
                InitFiles.Add((secondSubFile, ""));
            }
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(""))) {
                var emptyRootFile = "emptyFile";
                var _ = await _storageClient.PutObjectAsync(new(){BucketName=InitBucketName, Key = emptyRootFile, InputStream = stream});
                InitFiles.Add((emptyRootFile, ""));
            }
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(""))) {
                var emptyRootFolder = "emptyfolder/";
                var _ = await _storageClient.PutObjectAsync(new(){BucketName=InitBucketName, Key = emptyRootFolder, InputStream = stream});
                InitFiles.Add((emptyRootFolder, ""));
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            if (InitFiles != default && InitFiles.Count > 0)
            {
                var directory = Path.GetDirectoryName(InitFiles[0].name);
                if (File.Exists(InitFiles[0].name))
                {
                    File.Delete(InitFiles[0].name);
                }
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory);
                }
            }
        }

        [Test, NonParallelizable, Order(1)]
        [Timeout(5000)]
        public async Task BucketShouldBeCreatedAtInitializationIfCreateIsTrue()
        {
            var bucketName = "testbucket";
            var userBuckets = await _storageClient.ListBucketsAsync();
            var bucketInitCount = userBuckets.Buckets.Count;
            Assert.IsEmpty(userBuckets.Buckets.Where(b => b.BucketName == bucketName), "bucket should not already exist");
            var _ = new QBucket(_connection, bucketName, true);
            userBuckets = await _storageClient.ListBucketsAsync();
            TestContext.Progress.WriteLine("list of buckets: {0}", string.Join(",", userBuckets.Buckets.Select(b => b.BucketName)));
            Assert.IsNotEmpty(userBuckets.Buckets, "bucket should be created in user storage");
            Assert.AreEqual(bucketInitCount + 1, userBuckets.Buckets.Count, "bucket should be created in user storage");
            CollectionAssert.Contains(userBuckets.Buckets.Select(b => b.BucketName), bucketName, "bucket should be created in user storage");
        }

        [Test, NonParallelizable, Order(1)]
        [Timeout(5000)]
        public async Task BucktCreationShouldNotThrowIfItAlreadyExists()
        {
            var bucketName = InitBucketName;
            var userBuckets = await _storageClient.ListBucketsAsync();
            var bucketInitCount = userBuckets.Buckets.Count;
            Assert.DoesNotThrow(() => {
                var bucket = new QBucket(_connection, bucketName, true);
                Assert.IsNotNull(bucket);
            });
        }

        [Test, NonParallelizable, Order(2)]
        [Timeout(5000)]
        public async Task BucketShouldNotBeCreatedAtInitializationIfCreateIsFalse()
        {
            var bucketName = "inexistent-bucket";
            var userBuckets = await _storageClient.ListBucketsAsync();
            Assert.IsNull(userBuckets.Buckets?.FirstOrDefault(b => b.BucketName == bucketName), "bucket should not exist");
            var bucket = new QBucket(_connection, bucketName, false); // create bucket without creating it in storage
            userBuckets = await _storageClient.ListBucketsAsync();
            Assert.AreEqual(BucketsInitCount, userBuckets.Buckets.Count, "bucket should still not exist in storage after initialization");
        }

        [Test, NonParallelizable, Order(3)]
        [Timeout(5000)]
        public async Task UploadStreamShouldUploadStreamInStorageBucketFile()
        {
            var messageToUpload = "message to upload in bucket";
            var fileName = "directory/remoteFile";
            using var streamToUpload = new MemoryStream(Encoding.UTF8.GetBytes(messageToUpload));
            var bucket = new QBucket(_connection, InitBucketName, false);
            var initObjectsCountFromStorageBucketResponse = await _storageClient.ListObjectsV2Async(new ListObjectsV2Request(){BucketName = InitBucketName});
            await bucket.UploadStreamAsync(streamToUpload, fileName);
            var newObjectsCountFromStorageBucketResponse = await _storageClient.ListObjectsV2Async(new ListObjectsV2Request(){BucketName = InitBucketName});
            Assert.AreEqual(initObjectsCountFromStorageBucketResponse.S3Objects.Count + 1, newObjectsCountFromStorageBucketResponse.S3Objects.Count, "The new object should be uploaded from stream");
            var bucketResponseFromStorage = await _storageClient.GetObjectAsync(InitBucketName, fileName);
            Assert.IsNotNull(bucketResponseFromStorage, "The new object should be uploaded from stream");
            using (var reader = new StreamReader(bucketResponseFromStorage.ResponseStream))
            {
                Assert.AreEqual(messageToUpload, await reader.ReadToEndAsync(), "The content of the downloaded object should be the same as the original");
            }
        }

        [Test, NonParallelizable, Order(4)]
        [Timeout(5000)]
        public async Task DownloadStreamShouldRetrieveFileFromStorageBucket()
        {
            var bucket = new QBucket(_connection, InitBucketName, false);
            var objectsFromStorageBucketResponse = await _storageClient.ListObjectsV2Async(new ListObjectsV2Request(){BucketName = InitBucketName});
            Assert.AreNotEqual(0, objectsFromStorageBucketResponse.S3Objects.Count, "The remote object should be created from test setup");
            var downloadedStream = await bucket.DownloadStreamAsync(InitFiles[0].name);
            Assert.IsNotNull(downloadedStream, "The remote object should be downloaded from bucket");
            using (var reader = new StreamReader(downloadedStream))
            {
                Assert.AreEqual(InitFiles[0].content, await reader.ReadToEndAsync(), "The content of the downloaded object should be the same as the original");
            }
        }

        [Test, NonParallelizable, Order(5)]
        [Timeout(5000)]
        public async Task ListEntriesShouldListFilesAndFoldersInStorageBucket()
        {
            var bucket = new QBucket(_connection, InitBucketName, false);
            // List from root
            var listEntries = await bucket.ListEntriesAsync();
            var listActualFilesAndFolders = (await _storageClient.ListObjectsV2Async(new ListObjectsV2Request(){BucketName = InitBucketName})).S3Objects;
            var expectedRootFilesAndFolders = listActualFilesAndFolders
                .Select(f => f.Key.Split('/')[0])
                .Distinct()
                .ToList();
            TestContext.Progress.WriteLine("list all files/folders: {0}, expected: {1}, got: {2}", string.Join(",", listActualFilesAndFolders.Select(f => f.Key)), string.Join(",", expectedRootFilesAndFolders), string.Join(",", listEntries.Select(b => b.Name)));
            Assert.AreEqual(expectedRootFilesAndFolders.Count, listEntries.Count, "Then bucket entries should list all root files and folders");
            Assert.AreEqual(3, listEntries.Count, "Then bucket entries should list two folders and one file fom test setup");
            CollectionAssert.Contains(listEntries.Select(s => s.Name), string.Format("{0}/", InitFiles[0].name.Split('/')[0]), "List entries should contain the init file directory");
            CollectionAssert.DoesNotContain(listEntries.Select(s => s.Name), InitFiles[0].name.Split('/')[1], "List entries should not contain the file inside the subdirectory");

            // List from subdirectory
            var directory = Path.GetDirectoryName(InitFiles[0].name);
            var listFolderEntries = await bucket.ListEntriesAsync(directory);
            Assert.AreEqual(2, listFolderEntries.Count, "Then bucket entries should list two folders and one file fom test setup");
            var listFolderActualFilesAndFoldersResponse = await _storageClient.ListObjectsV2Async(new ListObjectsV2Request(){BucketName = InitBucketName});
            var listFolderActualFilesAndFolders = listFolderActualFilesAndFoldersResponse.S3Objects;
            var expectedFolderFilesAndFolders = listActualFilesAndFolders
                .Where(f => f.Key.StartsWith(directory, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
            TestContext.Progress.WriteLine("list all files/folders: {0}, expected: {1}, got: {2}", string.Join(",", listFolderActualFilesAndFolders.Select(f => f.Key)), string.Join(",", expectedFolderFilesAndFolders), string.Join(",", listFolderEntries.Select(b => b.Name)));
            Assert.AreEqual(expectedFolderFilesAndFolders.Count, listFolderEntries.Count, "Then bucket entries should list all root files and folders");
            CollectionAssert.DoesNotContain(listFolderEntries.Select(s => s.Name), string.Format("{0}/", directory), "List entries should not contain the directory name");
            CollectionAssert.DoesNotContain(listFolderEntries.Select(s => s.Name),InitFiles[2].name, "List entries should not contain root files");
            CollectionAssert.Contains(listFolderEntries.Select(s => s.Name), InitFiles[0].name, "List entries should contain the files inside the subdirectory");
            CollectionAssert.Contains(listFolderEntries.Select(s => s.Name), InitFiles[1].name, "List entries should contain the files inside the subdirectory");
        }

        [Test, NonParallelizable, Order(6)]
        [Timeout(5000)]
        public async Task ListFilesShouldListFilesInStorageBucket()
        {
            var bucket = new QBucket(_connection, InitBucketName, false);
            var listFiles = await bucket.ListFilesAsync();
            Assert.GreaterOrEqual(InitFiles.Count, listFiles.Count, "The bucket contains all files added at setup");
            CollectionAssert.Contains(listFiles.Select(f => f.Name), InitFiles[0].name, "The listed file should contain its whole path");
            CollectionAssert.DoesNotContain(listFiles.Select(f => f.Name), InitFiles[0].name.Split('/')[0], "The folder should not be listed alone");
        }

        [Test, NonParallelizable, Order(7)]
        [Timeout(5000)]
        public async Task DeleteEntryShouldDeleteFileFromStorageBucket()
        {
            var tmpEntry = "tmp";
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(""))) {
                var _ = await _storageClient.PutObjectAsync(new(){BucketName=InitBucketName, Key = tmpEntry, InputStream = stream});
            }
            var getEntryResponse = await _storageClient.GetObjectAsync(InitBucketName, tmpEntry);
            Assert.AreEqual(HttpStatusCode.OK, getEntryResponse.HttpStatusCode);
            var bucket = new QBucket(_connection, InitBucketName, false);
            await bucket.DeleteEntryAsync(tmpEntry);
            var ex = Assert.ThrowsAsync<AmazonS3Exception>(async () => await _storageClient.GetObjectAsync(InitBucketName, tmpEntry));
            Assert.AreEqual("NoSuchKey", ex.ErrorCode);
            Assert.IsAssignableFrom<HttpErrorResponseException>(ex.InnerException);
            Assert.AreEqual(HttpStatusCode.NotFound, (ex.InnerException as HttpErrorResponseException).Response.StatusCode);
        }

        [Test, NonParallelizable, Order(8)]
        [Timeout(5000)]
        public async Task BucketShouldBeDeletedFromStorage()
        {
            var bucket = new QBucket(_connection, InitBucketName, false);
            await bucket.DeleteAsync();
            var userBuckets = await _storageClient.ListBucketsAsync();
            Assert.IsEmpty(userBuckets.Buckets.Where(b => b.BucketName == InitBucketName), "bucket should be deleted from storage");
        }
    }
}
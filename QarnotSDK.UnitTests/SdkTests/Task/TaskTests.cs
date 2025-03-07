namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    using QarnotSDK;


    [TestFixture]
    public class TaskTests
    {
        private const string StorageUrl = "http://storage";
        private const string ApiUrl = "http://api";
        private const string Token = "token";

        private readonly string TmpDir = Path.GetTempPath() + Path.DirectorySeparatorChar + "QarnotTest" + Guid.NewGuid().ToString();

        private Connection Connect { get; set; }

        private Connection ReadonlyConnect { get; set; }

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler()
            {
                ResponseBody = TaskTestsData.TaskResponseFullBody,
            };
            Connect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler)
            {
                StorageAccessKey = "fake@mail.com",
            };
            ReadonlyConnect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler)
            {
                IsReadOnly = true,
            };
        }

        [TearDown]
        public void TearDown()
        {
            HttpHandler.Dispose();
        }

        public void TestBodyRequestAssert(string methodCall, string partialPath, string body)
        {
            ParsedRequest value = HttpHandler.ParsedRequests.Find(req =>
                req.Method.Contains(methodCall, StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ApiUrl}/{partialPath}", StringComparison.InvariantCultureIgnoreCase));

            Assert.AreEqual(body, value.Content);
        }

        public void TestRequestAssert(string methodCall, string partialPath)
        {
            bool value = HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains(methodCall, StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ApiUrl}/{partialPath}", StringComparison.InvariantCultureIgnoreCase));
            if (value == false)
            {
                Console.WriteLine("Looking for: " + methodCall + "_#_" + ApiUrl + "/" + partialPath);
                foreach (var r in HttpHandler.ParsedRequests)
                {
                    Console.WriteLine(r.Method + "_*_" + r.Uri);
                }
            }

            Assert.True(value);
        }

        [Test]
        public async Task CheckVpnConnectionsTestValues()
        {
            HttpHandler.ResponseBody = TaskTestsData.TaskResponseFullBody;
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            await task.UpdateStatusAsync();
            Assert.AreEqual(task.Status.RunningInstancesInfo.PerRunningInstanceInfo[0].VpnConnections[0].VpnName, "my-vpn");
            Assert.AreEqual(task.Status.RunningInstancesInfo.PerRunningInstanceInfo[0].VpnConnections[0].NodeIPAddressCidr, "172.20.0.14/16");
        }

        [Test]
        public async Task CheckCoreCountTestValues()
        {
            HttpHandler.ResponseBody = TaskTestsData.TaskResponseFullBody;
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            await task.UpdateStatusAsync();
            Assert.AreEqual(task.Status.RunningInstancesInfo.PerRunningInstanceInfo[0].CoreCount, 8);
            Assert.AreEqual(task.Status.RunningInstancesInfo.PerRunningInstanceInfo[1].CoreCount, 8);
            Assert.AreEqual(task.Status.RunningInstancesInfo.PerRunningInstanceInfo[2].CoreCount, 0);
        }

        // TODO: create unitTests for SnapshotAsync
        // TODO: create unitTests for CopyStdoutToAsync
        // TODO: create unitTests for CopyStderrToAsync
        // TODO: create unitTests for CopyFreshStdoutToAsync
        // TODO: create unitTests for CopyFreshStderrToAsync
        // TODO: create unitTests for StdoutAsync
        // TODO: create unitTests for StderrAsync
        // TODO: create unitTests for FreshStdoutAsync
        // TODO: create unitTests for FreshStderrAsync
        public async Task CallRunAsyncAndVerifyTheUuidIsUpload()
        {
            QTask task = new QTask(Connect, "name", "profile", 10);
            string uuid = TaskTestsData.TaskResponseUuid;

            Assert.IsTrue(task.Uuid.ToString() != uuid);
            await task.RunAsync(-1, TmpDir);
            Assert.IsTrue(task.Uuid.ToString() == uuid);
            Assert.IsTrue(task.Completed);
        }

        [Test]
        public async Task PostSubmitAsyncVerifyTheUuidIsUpload()
        {
            QTask task = new QTask(Connect, "name", "profile", 10);
            string uuid = TaskTestsData.TaskResponseUuid;
            Assert.IsTrue(task.Uuid.ToString() != uuid);
            await task.SubmitAsync();
            Assert.IsTrue(task.Uuid.ToString() == uuid);
        }

        [Test]
        public async Task PostSubmitAsyncShouldGetOnCorrectEndpoint()
        {
            QTask task = new QTask(Connect, "name", "profile", 10);
            await task.SubmitAsync();
            TestRequestAssert("POST", "tasks");
            TestRequestAssert("GET", "tasks/" + TaskTestsData.TaskResponseUuid);
         }

        [Test]
        public async Task CheckPreSubmitAsyncConstantValues()
        {
            string name = "name";
            string profile = "profile";
            AdvancedRanges range = new AdvancedRanges("1-100,10-20");

            var task = new QTask(Connect, name, profile, range);
            task.SetConstant("key_1", "name_1");
            task.SetConstant("key_2", "name_2");
            await task.PreSubmitAsync(default);
            Assert.IsTrue(task.Constants["key_1"] == "name_1");
            Assert.IsTrue(task.Constants["key_2"] == "name_2");
        }

        [Test]
        public async Task CheckPreSubmitAsyncConstraintValues()
        {
            string name = "name";
            string profile = "profile";
            AdvancedRanges range = new AdvancedRanges("1-100,10-20");

            var task = new QTask(Connect, name, profile, range);
            task.SetConstraint("key_3", "name_3");
            task.SetConstraint("key_4", "name_4");
            await task.PreSubmitAsync(default);
            Assert.IsTrue(task.Constraints["key_3"] == "name_3");
            Assert.IsTrue(task.Constraints["key_4"] == "name_4");
        }

        [Test]
        public void PreSubmitAsyncFailIfInstanceAndRangeSetTogether()
        {
            string name = "name";
            string profile = "profile";
            uint instanceCount = 10;
            AdvancedRanges range = new AdvancedRanges("1-100,10-20");
            Exception ex = null;
            QTask task = null;
            task = new QTask(Connect, name, profile, range);

            task._taskApi.InstanceCount = instanceCount;
            ex = Assert.ThrowsAsync<Exception>(async () => await task.PreSubmitAsync(default));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void PreSubmitAsyncFailIfInstanceAndRangeUnsetTogether()
        {
            string name = "name";
            string profile = "profile";
            QJob job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);

            var task = new QTask(Connect, name, profile, 0);
            var ex = Assert.ThrowsAsync<Exception>(async () => await task.PreSubmitAsync(default));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task PreSubmitAsyncDontFailIfHaveDependenciesOrNoJob()
        {
            string name = "name";
            string profile = "profile";
            uint instanceCount = 10;
            QJob job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            QPool pool = new QPool(Connect, Guid.NewGuid());
            QTask task = null;

            job = Connect.CreateJob("job", pool, "jobshortname");
            task = new QTask(Connect, name, job, instanceCount);
            task.SetTaskDependencies(task);
            await task.PreSubmitAsync(default);
            task = new QTask(Connect, name, profile, instanceCount);
            await task.PreSubmitAsync(default);
        }

        [Test]
        public void PreSubmitAsyncFailIfDependenciesAndNoJob()
        {
            string name = "name";
            string profile = "profile";
            uint instanceCount = 10;
            QJob job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            Exception ex = null;
            QTask task = null;
            task = new QTask(Connect, name, profile, instanceCount);

            task.SetTaskDependencies(task);
            ex = Assert.ThrowsAsync<Exception>(async () => await task.PreSubmitAsync(default));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task PreSubmitAsyncFailIfProfileAndJobWithoutPool()
        {
            string name = "name";
            uint instanceCount = 10;
            AdvancedRanges range = new AdvancedRanges("1-100,10-20");
            QJob job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            QPool pool = new QPool(Connect, Guid.NewGuid());
            Exception ex = null;
            QTask task = null;

            job = Connect.CreateJob("job", pool, "jobshortname");
            task = new QTask(Connect, name, job, instanceCount);
            await task.PreSubmitAsync(default);

            job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            task = new QTask(Connect, name,  job, instanceCount);
            ex = Assert.ThrowsAsync<Exception>(async () => await task.PreSubmitAsync(default));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task PreSubmitAsyncFailIfHaveNoProfileAndNoJob()
        {
            string name = "name";
            string profile = "profile";
            uint instanceCount = 10;
            AdvancedRanges range = new AdvancedRanges("1-100,10-20");
            QJob job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            QPool pool = new QPool(Connect, Guid.NewGuid());
            Exception ex = null;
            QTask task = null;

            job = Connect.CreateJob("job", pool, "jobshortname");
            task = new QTask(Connect, name, job, instanceCount);
            await task.PreSubmitAsync(default);
            task = new QTask(Connect, name, profile, instanceCount);
            await task.PreSubmitAsync(default);

            task = new QTask(Connect, name);
            ex = Assert.ThrowsAsync<Exception>(async () => await task.PreSubmitAsync(default));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void PreSubmitAsyncReadonlyFailTest()
        {
            string name = "name";
            string profile = "profile";
            AdvancedRanges range = new AdvancedRanges("1-100,10-20");
            QJob job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            QPool pool = new QPool(Connect, Guid.NewGuid());
            Exception ex = null;
            QTask task = null;

            task = new QTask(ReadonlyConnect, name, profile, range);
            ex = Assert.ThrowsAsync<Exception>(async () => await task.PreSubmitAsync(default));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void AbortAsyncReadonlyFailTest()
        {
            QTask task = new QTask(ReadonlyConnect, Guid.NewGuid().ToString());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await task.AbortAsync());
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task AbortAsyncShouldPostOnCorrectEndpoint()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            await task.AbortAsync();
            TestRequestAssert("POST", $"tasks/{id}/abort");
        }

        [Test]
        public void SnapshotAsyncReadonlyFailTest()
        {
            QTask task = new QTask(ReadonlyConnect, Guid.NewGuid().ToString());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await task.SnapshotAsync());
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task SnapshotAsyncShouldPostOnCorrectEndpoint()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            await task.SnapshotAsync();
            TestRequestAssert("POST", $"tasks/{id}/snapshot");
        }

        [Test]
        public async Task SnapshotAsyncBodyValuesCheck()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            task.SnapshotBlacklist = "BlackList";
            task.SnapshotWhitelist = "WhiteList";
            task.SnapshotBucket = new QBucket(Connect, "bucket", false);
            task.SnapshotBucketPrefix = "Prefix";
            string body = "{\"Whitelist\":null,\"Blacklist\":null,\"Bucket\":null,\"BucketPrefix\":null}";

            await task.SnapshotAsync();
            TestBodyRequestAssert("POST", $"tasks/{id}/snapshot", body);
        }

        [Test]
        public void TriggerSnapshotAsyncReadonlyFailTest()
        {
            QTask task = new QTask(ReadonlyConnect, Guid.NewGuid().ToString());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await task.TriggerSnapshotAsync());
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task TriggerSnapshotAsyncShouldPostOnCorrectEndpoint()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            await task.TriggerSnapshotAsync();
            TestRequestAssert("POST", $"tasks/{id}/snapshot");
        }

        [Test]
        public async Task TriggerSnapshotAsyncBodyValuesCheck()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            task.SnapshotBlacklist = "BlackList";
            task.SnapshotWhitelist = "WhiteList";
            task.SnapshotBucket = new QBucket(Connect, "bucket", false);
            task.SnapshotBucketPrefix = "Prefix";
            string body = "{\"Whitelist\":null,\"Blacklist\":null,\"Bucket\":null,\"BucketPrefix\":null}";

            await task.TriggerSnapshotAsync();
            TestBodyRequestAssert("POST", $"tasks/{id}/snapshot", body);
        }

        [Test]
        public async Task TriggerSnapshotAsyncChangeBodyValuesCheck()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            task.SnapshotBlacklist = "BlackList";
            task.SnapshotWhitelist = "WhiteList";
            task.SnapshotBucket = new QBucket(Connect, "bucket", false);
            task.SnapshotBucketPrefix = "Prefix";
            string body = "{\"Whitelist\":\"White2List\",\"Blacklist\":\"Black2List\",\"Bucket\":\"buc2ket\",\"BucketPrefix\":\"Pre2fix\"}";

            await task.TriggerSnapshotAsync("White2List", "Black2List", new QBucket(Connect, "buc2ket", false), "Pre2fix");
            TestBodyRequestAssert("POST", $"tasks/{id}/snapshot", body);
        }

        [Test]
        public void SnapshotPeriodicAsyncReadonlyFailTest()
        {
            QTask task = new QTask(ReadonlyConnect, Guid.NewGuid().ToString());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await task.SnapshotPeriodicAsync(1));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task SnapshotPeriodicAsyncShouldPostOnCorrectEndpoint()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);

            await task.SnapshotPeriodicAsync(1);
            TestRequestAssert("POST", $"tasks/{id}/snapshot/periodic");
        }

        [Test]
        public async Task SnapshotPeriodicAsyncBodyValuesCheck()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            task.SnapshotBlacklist = "BlackList";
            task.SnapshotWhitelist = "WhiteList";
            task.SnapshotBucket = new QBucket(Connect, "bucket", false);
            task.SnapshotBucketPrefix = "Prefix";
            string body = "{\"Interval\":1,\"Whitelist\":null,\"Blacklist\":null,\"Bucket\":null,\"BucketPrefix\":null}";

            await task.SnapshotPeriodicAsync(1);
            TestBodyRequestAssert("POST", $"tasks/{id}/snapshot/periodic", body);
        }

        [Test]
        public void TriggerPeriodicSnapshotAsyncReadonlyFailTest()
        {
            QTask task = new QTask(ReadonlyConnect, Guid.NewGuid().ToString());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await task.TriggerPeriodicSnapshotAsync(1));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task TriggerPeriodicSnapshotAsyncShouldPostOnCorrectEndpoint()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);

            await task.TriggerPeriodicSnapshotAsync(1);
            TestRequestAssert("POST", $"tasks/{id}/snapshot/periodic");
        }

        [Test]
        public async Task TriggerPeriodicSnapshotAsyncBodyValuesCheck()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            task.SnapshotBlacklist = "BlackList";
            task.SnapshotWhitelist = "WhiteList";
            task.SnapshotBucket = new QBucket(Connect, "bucket", false);
            task.SnapshotBucketPrefix = "Prefix";
            string body = "{\"Interval\":1,\"Whitelist\":null,\"Blacklist\":null,\"Bucket\":null,\"BucketPrefix\":null}";

            await task.TriggerPeriodicSnapshotAsync(1);
            TestBodyRequestAssert("POST", $"tasks/{id}/snapshot/periodic", body);
        }

        [Test]
        public async Task TriggerPeriodicSnapshotAsyncChangeBodyValuesCheck()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            task.SnapshotBlacklist = "BlackList";
            task.SnapshotWhitelist = "WhiteList";
            task.SnapshotBucket = new QBucket(Connect, "bucket", false);
            task.SnapshotBucketPrefix = "Prefix";
            string body = "{\"Interval\":1,\"Whitelist\":\"White2List\",\"Blacklist\":\"Black2List\",\"Bucket\":\"buc2ket\",\"BucketPrefix\":\"Pre2fix\"}";

            await task.TriggerPeriodicSnapshotAsync(1, "White2List", "Black2List", new QBucket(Connect, "buc2ket", false), "Pre2fix");
            TestBodyRequestAssert("POST", $"tasks/{id}/snapshot", body);
        }

        [Test]
        public void DownloadResultAsyncIntergrationTestCheckTheCreationOfADirectoryInTmp()
        {
            var task = new QTask(Connect, Guid.NewGuid().ToString());
            string dirName = TmpDir;

            if (Directory.Exists(dirName))
            {
                Directory.Delete(dirName);
            }

            var ex = Assert.ThrowsAsync<System.NullReferenceException>(async () => await task.DownloadResultAsync(dirName));
            Assert.IsTrue(Directory.Exists(dirName));
            Directory.Delete(dirName);
        }

        [Test]
        public async Task WaitAsyncWait2CancelAfter1Sec()
        {
            using var httpHandler = new WaitHTTPHandler(2);
            Connection connect = new Connection(ApiUrl, StorageUrl, Token, httpHandler);
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(connect, uuid);

            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Assert.AreEqual(task.Name, uuid);
            var wait = task.WaitAsync(3, cancellationToken.Token);
            Thread.Sleep(1000);
            cancellationToken.Dispose();
            await wait;
            Assert.IsTrue(task.Completed);
            Assert.AreEqual(task.State, "Success");
        }

        [Test]
        public async Task WaitAsync2SecCheckTime()
        {
            using var httpHandler = new WaitHTTPHandler(2);
            Connection connect = new Connection(ApiUrl, StorageUrl, Token, httpHandler);
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(connect, uuid);

            DateTime now = DateTime.Now;
            DateTime min_time = now.AddSeconds(1);

            await task.WaitAsync(2);

            if (DateTime.Now < min_time)
            {
                throw new Exception($"wait job is to short... start: {now}, actual:{DateTime.Now} < {min_time}");
            }
        }

        [Test]
        public async Task WaitAsync2SecCheckReturnValue()
        {
            using var httpHandler = new WaitHTTPHandler(2);
            Connection connect = new Connection(ApiUrl, StorageUrl, Token, httpHandler);
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(connect, uuid);

            Assert.AreEqual(task.Name, uuid);
            await task.WaitAsync();
            Assert.AreEqual(task.Name, "vanilla_task_name");
            Assert.AreEqual(task.State, "Success");
        }

        [Test]
        public async Task WaitAsync1SecAndDontReturnBefore4SecondsItMustReturnFalse()
        {
            using var httpHandler = new WaitHTTPHandler(4);
            Connection connect = new Connection(ApiUrl, StorageUrl, Token, httpHandler);
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(connect, uuid);

            DateTime now = DateTime.Now;
            DateTime min_time = now.AddMilliseconds(1000.0);
            DateTime max_time = now.AddMilliseconds(3000.0);

            bool ret = await task.WaitAsync(1);
            Assert.IsFalse(ret);
            if (DateTime.Now < min_time)
            {
                throw new Exception($"wait job is to short... start: {now}, actual:{DateTime.Now} < {min_time}");
            }
            else if (DateTime.Now > max_time)
            {
                throw new Exception($"wait job is to long... start: {now}, actual:{DateTime.Now} > {max_time}");
            }
        }

        [Test]
        public async Task UpdateStatusAsyncWithNoParamShouldGetOnCorrectEndpoint()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            await task.UpdateStatusAsync();
            TestRequestAssert("GET", "tasks");
        }

        [Test]
        public async Task UpdateStatusAsyncWithNoParamCheckReturnBody()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            Assert.AreEqual(task.Name, uuid);
            Assert.IsNull(task.Profile);
            await task.UpdateStatusAsync();
            Assert.AreEqual(task.Name, TaskTestsData.TaskResponseName);
            Assert.AreEqual(task.Profile, TaskTestsData.TaskResponseProfile);
        }

        [Test]
        public async Task UpdateStatusAsyncCheckNameAndProfile()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            CancellationToken cancellationToken = default;
            bool updateBucket = true;
            Assert.AreEqual(task.Name, uuid);
            Assert.IsNull(task.Profile);
            await task.UpdateStatusAsync(cancellationToken, updateBucket);
            Assert.AreEqual(task.Name, TaskTestsData.TaskResponseName);
            Assert.AreEqual(task.Profile, "docker-batch");
            TestRequestAssert("GET", "tasks");
        }

        [Test]
        public void SetConstraintCheckThatNullKeyThrowException()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            task.SetConstraint("key1", "value1");
            Exception ex = Assert.Throws<ArgumentNullException>(() => task.SetConstraint(null, "value1"));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void SetConstraintCheckThatNullValueDeleteValue()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            task.SetConstraint("key1", "value1");
            Assert.AreEqual("value1", task.Constraints["key1"]);
            task.SetConstraint("key1", null);
            Assert.That(task.Constraints, !Contains.Key("key1"));
        }

        [Test]
        public void SetConstraintModifyValueCheck()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            task.SetConstraint("key1", "value1");
            task.SetConstraint("key1", "value3");
            Assert.AreEqual("value3", task.Constraints["key1"]);
        }

        [Test]
        public void SetConstraintAddValueCheck()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            Assert.True(task.Constraints.Count == 0);
            task.SetConstraint("key1", "value1");
            task.SetConstraint("key2", "value2");
            Assert.AreEqual("value1", task.Constraints["key1"]);
            Assert.AreEqual("value2", task.Constraints["key2"]);
        }

        [Test]
        public void SetConstantCheckThatNullKeyThrowException()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            Assert.True(task.Constants.Count == 0);
            task.SetConstant("key1", "value1");
            Exception ex = Assert.Throws<ArgumentNullException>(() => task.SetConstant(null, "value1"));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void SetConstantCheckThatNullValueDeleteValue()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            Assert.True(task.Constants.Count == 0);
            task.SetConstant("key1", "value1");
            task.SetConstant("key1", null);
            Assert.That(task.Constants, !Contains.Key("key1"));
        }

        [Test]
        public void SetConstantModifyValueValueCheck()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            Assert.True(task.Constants.Count == 0);
            task.SetConstant("key1", "value1");
            task.SetConstant("key1", "value3");
            Assert.AreEqual("value3", task.Constants["key1"]);
        }

        [Test]
        public void SetConstantAddValueCheck()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            Assert.True(task.Constants.Count == 0);
            task.SetConstant("key1", "value1");
            task.SetConstant("key2", "value2");
            Assert.AreEqual("value1", task.Constants["key1"]);
            Assert.AreEqual("value2", task.Constants["key2"]);
        }

        [Test]
        public void SetTagsCheckTheValuesAdd()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            string[] tags = new string[] { "tag1", "tag2" };
            task.SetTags(tags);
            CollectionAssert.AreEqual(tags, task.Tags);
        }

        [Test]
        public void SnapshotBucketCheckTheValuesAdd()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            var moqBucket = new Mock<QBucket>();
            moqBucket.Setup(foo => foo.Shortname).Returns("bucket-name");

            QBucket bucket = moqBucket.Object;
            task.SnapshotBucket = bucket;
            Assert.AreEqual("bucket-name", task.SnapshotBucket.Shortname);
            Assert.AreEqual("bucket-name", task._taskApi.SnapshotBucket);
        }

        [Test]
        public void ResultsBucketPrefixCheckTheValuesAdd()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            string bucketPrefix = "bucket-prefix";
            task.ResultsBucketPrefix = bucketPrefix;
            Assert.AreEqual(bucketPrefix, task.ResultsBucketPrefix);
        }

        [Test]
        public void SnapshotBucketPrefixCheckTheValuesAdd()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            string bucketPrefix = "bucket-prefix";
            task.SnapshotBucketPrefix = bucketPrefix;
            Assert.AreEqual(bucketPrefix, task.SnapshotBucketPrefix);
        }

        [Test]
        public void SetTaskDependenciesWithGuidListCheckThatDependsOnIsTheGoodUuid()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            Guid[] ids = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
            task.SetTaskDependencies(ids);
            CollectionAssert.AreEqual(task.DependsOn, ids);
        }

        [Test]
        public void SetTaskDependenciesWithTaskListCheckThatDependsOnIsTheGoodUuid()
        {
            QTask task1 = new QTask(Connect, Guid.NewGuid().ToString());
            QTask task2 = new QTask(Connect, Guid.NewGuid().ToString());
            QTask task3 = new QTask(Connect, Guid.NewGuid().ToString());
            QTask[] tasks = new QTask[] { task1, task2 };
            task3.SetTaskDependencies(tasks);
            Assert.True(task1.Uuid == task3.DependsOn[0]);
            Assert.True(task2.Uuid == task3.DependsOn[1]);
        }

        [Test]
        public async Task CreateAsyncCreateANewTaskWithATaskApiCheckUuid()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            QTask task2 = await QTask.CreateAsync(Connect, task._taskApi);
            Assert.AreEqual(task2.Uuid, task.Uuid);
        }

        [Test]
        public async Task InitializeAsyncCreateANewTaskWithATaskApiCheckUuid()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            QTask task2 = await task.InitializeAsync(Connect, task._taskApi);
            Assert.AreEqual(task2.Uuid, task.Uuid);
        }

        [Test]
        public void InitializeAsyncApiNullFailThrowException()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            NullReferenceException ex = Assert.ThrowsAsync<NullReferenceException>(async () => await task.InitializeAsync(Connect, null));
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task InitializeAsyncConnectNullDoNotFail()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            QTask task2 = await task.InitializeAsync(null, task._taskApi);
            Assert.AreEqual(task2.Uuid, task.Uuid);
        }

        [Test]
        public async Task GetMaxRetriesPerInstanceIsExposedInTask()
        {
            TaskApi taskApi = new TaskApi();
            taskApi.MaxRetriesPerInstance = 10;
            QTask task = await QTask.CreateAsync(null, taskApi);
            Assert.AreEqual(task.MaxRetriesPerInstance, 10);
        }

        [Test]
        public async Task SetMaxRetriesPerInstanceIsExposedInTask()
        {
            TaskApi taskApi = new TaskApi();
            QTask task = await QTask.CreateAsync(null, taskApi);
            task.MaxRetriesPerInstance = 10;
            Assert.AreEqual(task.MaxRetriesPerInstance, 10);
        }

        [Test]
        public void TestAllTheTaskConstructorsEntries()
        {
            QJob job;
            QTask task;
            QPool pool;
            Guid uuid = Guid.NewGuid();
            string name;
            string profile;
            string shortname;
            uint instanceCount;
            AdvancedRanges range;
            job = Connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            name = "PretyName";
            shortname = "shortname";
            profile = "profile";
            range = new AdvancedRanges("1-100,10-20");
            instanceCount = 42;
            pool = new QPool(Connect, uuid);
            task = new QTask(Connect, uuid);
            Assert.True(task.Uuid == uuid);

            task = new QTask(Connect, name, job, range, shortname, profile);
            Assert.True(task.InstanceCount == range.Count);
            Assert.True(task._taskApi.JobUuid == job.Uuid.ToString());
            Assert.True(task.Name == name);
            Assert.True(task.Profile == profile);
            Assert.True(task.Shortname == shortname);

            task = new QTask(Connect, name, job, instanceCount, shortname, profile);
            Assert.True(task.InstanceCount == instanceCount);
            Assert.True(task._taskApi.JobUuid == job.Uuid.ToString());
            Assert.True(task.Name == name);
            Assert.True(task.Profile == profile);
            Assert.True(task.Shortname == shortname);

            task = new QTask(Connect, name, pool, range, shortname);
            Assert.True(task.InstanceCount == range.Count);
            Assert.True(task._taskApi.PoolUuid == pool.Uuid.ToString());
            Assert.True(task.Name == name);
            Assert.True(task.Shortname == shortname);

            task = new QTask(Connect, name, pool, instanceCount, shortname);
            Assert.True(task._taskApi.PoolUuid == pool.Uuid.ToString());
            Assert.True(task.Name == name);
            Assert.True(task.Shortname == shortname);
            Assert.True(task.InstanceCount == instanceCount);

            task = new QTask(Connect, name, profile, range, shortname);
            Assert.True(task.InstanceCount == range.Count);
            Assert.True(task.Name == name);
            Assert.True(task.Profile == profile);
            Assert.True(task.Shortname == shortname);

            task = new QTask(Connect, name, profile, instanceCount, shortname);
            Assert.True(task.InstanceCount == instanceCount);
            Assert.True(task.Name == name);
            Assert.True(task.Profile == profile);
            Assert.True(task.Shortname == shortname);

            task = new QTask(Connect, name, profile, shortname);
            Assert.True(task.Name == name);
            Assert.True(task.Profile == profile);

            Assert.True(task.Shortname == shortname);
        }

        [Test]
        [Category("PoolResourcesSync")]
        public async Task TestTaskBuildFromJson_FillsUpWaitForPoolSynchronization_WithNull_WhenAbsentFromResponse()
        {
            HttpHandler.ResponseBody = TaskTestsData.TaskResponseWithAdvancedBucketsFullBody;
            var task = await Connect.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task.WaitForPoolResourcesSynchronization, Is.Null);
        }

        [Test]
        [Category("PoolResourcesSync")]
        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public async Task TestTaskBuildFromJson_FillsUpWaitForPoolSynchronization_WithValue_WhenPresentInResponse(bool? wait)
        {
            HttpHandler.ResponseBody = TaskTestsData.TaskResponse_WithWaitForPoolResourcesSynchronization(wait);
            var task = await Connect.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task.WaitForPoolResourcesSynchronization, Is.EqualTo(wait));
        }


        [Test]
        [Category("PoolResourcesSync")]
        public async Task TestTaskWithoutWaitForPoolSynchronization_LeavesTaskApiFieldNull()
        {
            QPool pool = Connect.CreatePool("pool-name", "pool-profile", 1, taskDefaultWaitForPoolResourcesSynchronization: true);
            QTask task = Connect.CreateTask("task-name", pool, 1);
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.WaitForPoolResourcesSynchronization, Is.Null);
        }


        [Test]
        [Category("PoolResourcesSync")]
        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public async Task TestTaskWithWaitForPoolSynchronization_FillsTaskApiField(bool? wait)
        {
            QPool pool = Connect.CreatePool("pool-name", "pool-profile", 1, taskDefaultWaitForPoolResourcesSynchronization: true);
            QTask task = Connect.CreateTask("task-name", pool, 1, waitForPoolResourcesSynchronization: wait);
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.WaitForPoolResourcesSynchronization, Is.EqualTo(wait));
        }

        [Test]
        public async Task CheckDefaultTTLTestValues()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            Assert.IsNull(task.DefaultResourcesCacheTTLSec);
            await task.UpdateStatusAsync();
            Assert.IsNotNull(task.DefaultResourcesCacheTTLSec);
            Assert.AreEqual(7776000, task.DefaultResourcesCacheTTLSec);
        }

        [Test]
        public async Task CheckPrivilegesTestValues()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            Assert.IsNotNull(task.Privileges);
            Assert.IsNull(task.Privileges.ExportApiAndStorageCredentialsInEnvironment);
            await task.UpdateStatusAsync();
            Assert.False(task.Privileges.ExportApiAndStorageCredentialsInEnvironment);
        }

        [Test]
        public async Task CheckRetrySettingsTestValues()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, uuid);
            Assert.IsNotNull(task.RetrySettings);
            Assert.IsNull(task.RetrySettings.MaxTotalRetries);
            Assert.IsNull(task.RetrySettings.MaxPerInstanceRetries);
            await task.UpdateStatusAsync();
            Assert.AreEqual(12, task.RetrySettings.MaxTotalRetries);
            Assert.AreEqual(12, task.RetrySettings.MaxPerInstanceRetries);
        }

        [Test]
        public async Task CheckTaskPoolNameValues()
        {
            string poolUuid = PoolTestsData.PoolResponseUuid;
            var pool = new QPool(Connect, Guid.NewGuid().ToString());
            HttpHandler.ResponseBody = PoolTestsData.PoolResponseBody;
            await pool.UpdateStatusAsync();
            Assert.AreEqual(poolUuid, pool.Uuid.ToString());
            Assert.False(string.IsNullOrWhiteSpace(pool.Name));
            Assert.False(string.IsNullOrWhiteSpace(pool.Shortname));

            HttpHandler.ResponseBody = TaskTestsData.TaskInPoolResponseBody;
            QTask task = new QTask(Connect, Guid.NewGuid().ToString(), pool);
            await task.UpdateStatusAsync();
            Assert.AreEqual(poolUuid, task.PoolUuid.ToString());

            HttpHandler.ResponseBody = PoolTestsData.PoolResponseBody;
            var taskPool = task.Pool;
            Assert.AreEqual(poolUuid, taskPool.Uuid.ToString());
            Assert.AreEqual(pool.Name, taskPool.Name);
            Assert.AreEqual(pool.Shortname, taskPool.Shortname);
        }

        [Test]
        public async Task CheckTaskSchedulingTypeDeserializationFromJson()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new (Connect, uuid);
            Assert.IsNull(task.SchedulingType);
            Assert.IsNull(task.TargetedReservedMachineKey);
            await task.UpdateStatusAsync();
            Assert.IsNotNull(task.SchedulingType);
            Assert.AreEqual(SchedulingType.Reserved, task.SchedulingType);
            Assert.AreEqual("some-reserved-machine", task.TargetedReservedMachineKey);
        }

        [TestCase(SchedulingType.Flex)]
        [TestCase(SchedulingType.OnDemand)]
        [TestCase(SchedulingType.Reserved)]
        public async Task CheckTaskSchedulingTypeSerialization(SchedulingType schedulingType)
        {
            QTask task = new (Connect, "test-task-with-scheduling", "profile", 1, schedulingType: schedulingType);
            Assert.IsNotNull(task.SchedulingType);
            Assert.AreEqual(schedulingType, task.SchedulingType);

            task.TargetedReservedMachineKey = "test-machine";

            if (schedulingType != SchedulingType.Reserved)
            {
                var ex = Assert.ThrowsAsync<Exception>(async () => await task.SubmitAsync());
                Assert.AreEqual("Cannot target a reserved machine without using a 'Reserved' scheduling type.", ex.Message);
                task.TargetedReservedMachineKey = default;
            }
            await task.SubmitAsync();

            var taskCreateRequest = HttpHandler.ParsedRequests.FirstOrDefault(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ApiUrl}/task", StringComparison.InvariantCultureIgnoreCase));

            Assert.That(taskCreateRequest, Is.Not.Null);

            var taskCreateString = taskCreateRequest.Content;
            dynamic taskCreateJson = JObject.Parse(taskCreateString);

            Console.WriteLine(taskCreateString);

            Assert.IsNotNull(taskCreateJson.SchedulingType);
            Assert.AreEqual(schedulingType.ToString(), taskCreateJson.SchedulingType.ToString());
            if (schedulingType == SchedulingType.Reserved)
            {
                Assert.IsNotNull(taskCreateJson.TargetedReservedMachineKey);
                Assert.AreEqual("test-machine", taskCreateJson.TargetedReservedMachineKey.ToString());
            }
        }

        [Test]
        public async Task CheckTaskForcedNetworkRulesDeserializationFromJson()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new (Connect, uuid);
            Assert.IsNull(task.ForcedNetworkRules);
            await task.UpdateStatusAsync();
            Assert.IsNotNull(task.ForcedNetworkRules);
            Assert.AreEqual(2, task.ForcedNetworkRules.Count);
            var firstRule = task.ForcedNetworkRules[0];
            Assert.AreEqual(true, firstRule.Inbound);
            Assert.AreEqual("tcp", firstRule.Proto);
            Assert.AreEqual("bound-to-be-alive", firstRule.To);
            Assert.AreEqual("1234", firstRule.Port);
            Assert.AreEqual("1000", firstRule.Priority);
            Assert.AreEqual("Inbound test", firstRule.Description);
            var secondRule = task.ForcedNetworkRules[1];
            Assert.AreEqual(false, secondRule.Inbound);
            Assert.AreEqual("tcp", secondRule.Proto);
            Assert.AreEqual("bound-to-the-devil", secondRule.PublicHost);
            Assert.AreEqual("666", secondRule.PublicPort);
            Assert.AreEqual("1000", secondRule.Priority);
            Assert.AreEqual("Outbound test", secondRule.Description);
        }

        [Test]
        public async Task CheckTaskForcedNetworkRulesSerialization()
        {
            QTask task = new (Connect, "test-task-with-forced-network-rules", "profile", 1);
            Assert.IsNull(task.ForcedNetworkRules);

            var networkRules = new List<ForcedNetworkRule>()
            {
                new ForcedNetworkRule(true, "tcp", "bound-to-be-alive", "1234", priority: "1000", description: "Inbound test"),
                new ForcedNetworkRule(false, "tcp", publicHost: "bound-to-the-devil", publicPort: "666", priority: "1000", description: "Outbound test"),
            };
            task.ForcedNetworkRules = networkRules;
            await task.SubmitAsync();

            var taskCreateRequest = HttpHandler.ParsedRequests.FirstOrDefault(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ApiUrl}/task", StringComparison.InvariantCultureIgnoreCase));

            Assert.That(taskCreateRequest, Is.Not.Null);

            var taskCreateString = taskCreateRequest.Content;
            var taskCreateJson = JsonConvert.DeserializeObject<dynamic>(taskCreateString);

            Console.WriteLine(taskCreateString);

            var forcedNetworkRules = taskCreateJson.ForcedNetworkRules;
            Assert.IsNotNull(forcedNetworkRules);
            Assert.AreEqual(2, forcedNetworkRules.Count);
            var firstExpectedRule = networkRules[0];
            var firstRule = forcedNetworkRules[0];
            Console.WriteLine("firstRule -> {0}", firstRule);
            Console.WriteLine("firstRule.GetType() -> {0}", firstRule.GetType());
            Assert.AreEqual(firstExpectedRule.Inbound, (bool)firstRule["Inbound"]);
            Assert.AreEqual(firstExpectedRule.Proto, (string)firstRule["Proto"]);
            Assert.AreEqual(firstExpectedRule.To, (string)firstRule["To"]);
            Assert.AreEqual(firstExpectedRule.Port, (string)firstRule["Port"]);
            Assert.AreEqual(firstExpectedRule.Priority, (string)firstRule["Priority"]);
            Assert.AreEqual(firstExpectedRule.Description, (string)firstRule["Description"]);
            var secondExpectedRule = networkRules[1];
            var secondRule = forcedNetworkRules[1];
            Assert.AreEqual(secondExpectedRule.Inbound, (bool)secondRule["Inbound"]);
            Assert.AreEqual(secondExpectedRule.Proto, (string)secondRule["Proto"]);
            Assert.AreEqual(secondExpectedRule.PublicHost, (string)secondRule["PublicHost"]);
            Assert.AreEqual(secondExpectedRule.PublicPort, (string)secondRule["PublicPort"]);
            Assert.AreEqual(secondExpectedRule.Priority, (string)secondRule["Priority"]);
            Assert.AreEqual(secondExpectedRule.Description, (string)secondRule["Description"]);
        }

        [Test]
        public async Task CheckTaskForcedConstantsDeserializationFromJson()
        {
            string uuid = Guid.NewGuid().ToString();
            QTask task = new (Connect, uuid);
            Assert.IsNull(task.ForcedConstants);
            await task.UpdateStatusAsync();
            Assert.IsNotNull(task.ForcedConstants);
            Assert.AreEqual(2, task.ForcedConstants.Count);
            var firstConstant = task.ForcedConstants[0];
            Assert.AreEqual("the-name-1", firstConstant.ConstantName);
            Assert.AreEqual("the-value-1", firstConstant.ForcedValue);
            Assert.AreEqual(true, firstConstant.ForceExportInEnvironment);
            Assert.AreEqual(null, firstConstant.Access);

            var secondConstant = task.ForcedConstants[1];
            Assert.AreEqual("the-name-2", secondConstant.ConstantName);
            Assert.AreEqual("the-value-2", secondConstant.ForcedValue);
            Assert.AreEqual(null, secondConstant.ForceExportInEnvironment);
            Assert.AreEqual(ForcedConstant.ForcedConstantAccess.ReadOnly, secondConstant.Access);
        }

        [Test]
        public async Task CheckTaskForcedConstantsSerialization()
        {
            QTask task = new (Connect, "test-task-with-forced-network-rules", "profile", 1);
            Assert.IsNull(task.ForcedConstants);

            var forcedConstants = new List<ForcedConstant>()
            {
                new ForcedConstant("the-first-constant-1", "the-forced-value-1"),
                new ForcedConstant("the-second-constant-2", "the-forced-value-2", true),
                new ForcedConstant("the-third-constant-3", "the-forced-value-3", null, ForcedConstant.ForcedConstantAccess.ReadWrite),
            };
            task.ForcedConstants = forcedConstants;
            await task.SubmitAsync();

            var taskCreateRequest = HttpHandler.ParsedRequests.FirstOrDefault(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ApiUrl}/task", StringComparison.InvariantCultureIgnoreCase));

            Assert.That(taskCreateRequest, Is.Not.Null);

            var taskCreateString = taskCreateRequest.Content;

            var taskCreateJson = JsonConvert.DeserializeObject<dynamic>(taskCreateString);

            var retrievedForcedConstants = taskCreateJson.ForcedConstants;
            Assert.IsNotNull(retrievedForcedConstants);
            Assert.AreEqual(3, retrievedForcedConstants.Count);
            var rule = retrievedForcedConstants[0];
            Assert.AreEqual("the-first-constant-1" , (string) rule["ConstantName"]);
            Assert.AreEqual("the-forced-value-1" , (string) rule["ForcedValue"]);
            Assert.AreEqual(null, (bool?) rule["ForceExportInEnvironment"]);
            Assert.AreEqual(null, (ForcedConstant.ForcedConstantAccess?) rule["Access"]);

            rule = retrievedForcedConstants[1];
            Assert.AreEqual("the-second-constant-2" , (string) rule["ConstantName"]);
            Assert.AreEqual("the-forced-value-2" , (string) rule["ForcedValue"]);
            Assert.AreEqual(true, (bool?) rule["ForceExportInEnvironment"]);
            Assert.AreEqual(null, (ForcedConstant.ForcedConstantAccess?) rule["Access"]);

            rule = retrievedForcedConstants[2];
            Assert.AreEqual("the-third-constant-3" , (string) rule["ConstantName"]);
            Assert.AreEqual("the-forced-value-3" , (string) rule["ForcedValue"]);
            Assert.AreEqual(null, (bool?) rule["ForceExportInEnvironment"]);
            Assert.AreEqual(ForcedConstant.ForcedConstantAccess.ReadWrite, (ForcedConstant.ForcedConstantAccess?) rule["Access"]);
        }
    }


    [TestFixture]
    public class TaskTestsAdvancedResources
    {
        private const string StorageUrl = "http://storage";
        private const string ApiUrl = "http://api";
        private const string Token = "token";

        private Connection Connect { get; set; }
        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        private Connection ConnectLegacyBucket { get; set; }
        private InterceptingFakeHttpHandler HttpHandlerLegacyBucket { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler()
            {
                ResponseBody = TaskTestsData.TaskResponseWithAdvancedBucketsFullBody,
            };
            Connect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler);

            HttpHandlerLegacyBucket = new InterceptingFakeHttpHandler()
            {
                ResponseBody = TaskTestsData.TaskResponseWithLegacyBucketsFullBody,
            };
            ConnectLegacyBucket = new Connection(ApiUrl, StorageUrl, Token, HttpHandlerLegacyBucket);
        }


        [Test]
        public async Task TestTaskPreSubmitAsync_WithPrefixFiltering_FillsUpApiTask()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Resources = new List<QAbstractStorage> { bucket.WithFiltering(new BucketFilteringPrefix(prefix)) };
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, task._taskApi.AdvancedResourceBuckets.Count);

            var apiResource = task._taskApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.Filtering.PrefixFiltering);
            Assert.AreEqual(prefix, apiResource.Filtering.PrefixFiltering.Prefix);
        }


        [Test]
        public async Task TestTaskPreSubmitAsync_WithResourcesTransformation_FillsUpApiTask()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Resources = new List<QAbstractStorage> { bucket.WithResourcesTransformation(new ResourcesTransformationStripPrefix(prefix)) };
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, task._taskApi.AdvancedResourceBuckets.Count);

            var apiResource = task._taskApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.ResourcesTransformation.StripPrefix);
            Assert.AreEqual(prefix, apiResource.ResourcesTransformation.StripPrefix.Prefix);
        }



        // Check that chaining both With* calls is correct
        [Test]
        public async Task TestTaskPreSubmitAsync_WithPrefixFiltering_AndResourcesTransformation_AndCacheTTL_FillsUpApiTask()
        {
            string prefix = "filter/prefix";
            int ttl = 1000;
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Resources = new List<QAbstractStorage> {
                bucket.WithFiltering(new BucketFilteringPrefix(prefix))
                      .WithResourcesTransformation(new ResourcesTransformationStripPrefix(prefix))
                      .WithCacheTTL(ttl),
            };
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, task._taskApi.AdvancedResourceBuckets.Count);

            var apiResource = task._taskApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNotNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.Filtering.PrefixFiltering);
            Assert.AreEqual(prefix, apiResource.Filtering.PrefixFiltering.Prefix);

            Assert.IsNotNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.ResourcesTransformation.StripPrefix);
            Assert.AreEqual(prefix, apiResource.ResourcesTransformation.StripPrefix.Prefix);

            Assert.AreEqual(ttl, apiResource.CacheTTLSec);
        }

        public async Task TestTaskPreSubmitAsync_With_ResultsCacheTTL_FillsUpApiTask()
        {
            int ttl = 1000;
            QBucket bucket = new QBucket(Connect, "out-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Results = bucket.WithCacheTTL(ttl);

            await task.PreSubmitAsync(default);

            Assert.AreEqual("out-bucket", task._taskApi.ResultBucket);
            Assert.AreEqual(ttl, task._taskApi.ResultsCacheTTLSec);
        }

        // NOTE: this test is there so that the SDK remains BC with older versions of rest-computing.
        // When all rest-computing have migrated, this test and the behavior it tests can be
        // removed.
        [Test]
        public async Task TestTaskPreSubmitAsync_WithoutFilterOrTransformation_UsesLegacyResources()
        {
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Resources = new List<QAbstractStorage> { bucket };
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.AdvancedResourceBuckets, Is.Null.Or.Empty);

            Assert.That(task._taskApi.ResourceBuckets.Count, Is.EqualTo(1));
            Assert.That(task._taskApi.ResourceBuckets[0], Is.EqualTo("the-bucket"));
        }


        [Test]
        public async Task TestTaskBuildFromJson_FillsUpFilterAndTransformationAndCacheTTL_InApiProxyObject()
        {
            var task = await Connect.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task._taskApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.That(task._taskApi.AdvancedResourceBuckets.Count, Is.EqualTo(2));

            var firstBucket = task._taskApi.AdvancedResourceBuckets[0];
            var secondBucket = task._taskApi.AdvancedResourceBuckets[1];

            Assert.That(firstBucket.BucketName, Is.EqualTo("someBucket"));
            Assert.That(firstBucket.Filtering, Is.Not.Null);
            Assert.That(firstBucket.Filtering.PrefixFiltering, Is.Not.Null);
            Assert.That(firstBucket.Filtering.PrefixFiltering.Prefix, Is.EqualTo("some/prefix/"));
            Assert.That(firstBucket.ResourcesTransformation, Is.Not.Null);
            Assert.That(firstBucket.ResourcesTransformation.StripPrefix, Is.Not.Null);
            Assert.That(firstBucket.ResourcesTransformation.StripPrefix.Prefix, Is.EqualTo("transformed-prefix/"));
            Assert.AreEqual(1000, firstBucket.CacheTTLSec);

            Assert.That(secondBucket.BucketName, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
            Assert.That(secondBucket.CacheTTLSec, Is.Null);
        }


        [Test]
        public async Task TestTaskBuildFromJson_FillsUpFilterAndTransformationAndCacheTTL_InSDKObject()
        {
            var task = await Connect.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task.Resources.Count, Is.EqualTo(2));

            var firstBucket = task.Resources[0] as QBucket;
            var secondBucket = task.Resources[1] as QBucket;

            Assert.That(firstBucket.Shortname, Is.EqualTo("someBucket"));
            Assert.That(firstBucket.Filtering, Is.Not.Null);
            Assert.That(firstBucket.Filtering, Is.AssignableFrom(typeof(BucketFilteringPrefix)));
            Assert.That((firstBucket.Filtering as BucketFilteringPrefix).Prefix, Is.EqualTo("some/prefix/"));
            Assert.That(firstBucket.ResourcesTransformation, Is.Not.Null);
            Assert.That(firstBucket.ResourcesTransformation, Is.AssignableFrom(typeof(ResourcesTransformationStripPrefix)));
            Assert.That((firstBucket.ResourcesTransformation as ResourcesTransformationStripPrefix).Prefix, Is.EqualTo("transformed-prefix/"));
            Assert.AreEqual(1000, firstBucket.CacheTTLSec);

            Assert.That(secondBucket.Shortname, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
            Assert.That(secondBucket.CacheTTLSec, Is.Null);
        }


        [Test]
        public async Task TestTaskBuildFromJson_FillsUpFilterAndTransformationAndCacheTTL_InSDKObject_WithLegacyResources()
        {
            var task = await ConnectLegacyBucket.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task.Resources.Count, Is.EqualTo(2));

            var firstBucket = task.Resources[0] as QBucket;
            var secondBucket = task.Resources[1] as QBucket;

            Assert.That(firstBucket.Shortname, Is.EqualTo("someBucket"));
            Assert.That(firstBucket.Filtering, Is.Null);
            Assert.That(firstBucket.ResourcesTransformation, Is.Null);
            Assert.That(firstBucket.CacheTTLSec, Is.Null);

            Assert.That(secondBucket.Shortname, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
            Assert.That(secondBucket.CacheTTLSec, Is.Null);
        }
    }
}

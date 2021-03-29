namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;
    using Moq;


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
            Connect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler);
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
        public async Task AbortAsyncShouldGetOnCorrectEndpoint()
        {
            var id = Guid.NewGuid();
            QTask task = new QTask(Connect, id);
            await task.AbortAsync();
            TestRequestAssert("POST", $"tasks/{id}/abort");
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
            var moqBucket = new Mock<QBucket>();// new QBucket(Connect, "bucket1");
            moqBucket.Setup(foo => foo.Shortname).Returns("bucket-name");

            QBucket bucket = moqBucket.Object;
            task.SnapshotBucket = bucket;
            CollectionAssert.AreEqual("bucket-name", task.SnapshotBucket.Shortname);
        }

        [Test]
        public void SnapshotBucketPrefixCheckTheValuesAdd()
        {
            QTask task = new QTask(Connect, Guid.NewGuid().ToString());
            string bucket_prefix = "bucket-prefix";
            task.SnapshotBucketPrefix = bucket_prefix;
            CollectionAssert.AreEqual(bucket_prefix, task.SnapshotBucketPrefix);
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
            Assert.AreEqual(1, task._taskApi.AdvancedResourceBuckets.Count());

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
            Assert.AreEqual(1, task._taskApi.AdvancedResourceBuckets.Count());

            var apiResource = task._taskApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.ResourcesTransformation.StripPrefix);
            Assert.AreEqual(prefix, apiResource.ResourcesTransformation.StripPrefix.Prefix);
        }



        // Check that chaining both With* calls is correct
        [Test]
        public async Task TestTaskPreSubmitAsync_WithPrefixFiltering_AndResourcesTransformation_FillsUpApiTask()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Resources = new List<QAbstractStorage> {
                bucket.WithFiltering(new BucketFilteringPrefix(prefix))
                      .WithResourcesTransformation(new ResourcesTransformationStripPrefix(prefix))
            };
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, task._taskApi.AdvancedResourceBuckets.Count());

            var apiResource = task._taskApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNotNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.Filtering.PrefixFiltering);
            Assert.AreEqual(prefix, apiResource.Filtering.PrefixFiltering.Prefix);

            Assert.IsNotNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.ResourcesTransformation.StripPrefix);
            Assert.AreEqual(prefix, apiResource.ResourcesTransformation.StripPrefix.Prefix);
        }



        // NOTE: this test is there so that the SDK remains BC with older versions of rest-computing.
        // When all rest-computing have migrated, this test and the behavior it tests can be
        // removed.
        [Test]
        public async Task TestTaskPreSubmitAsync_WithoutFilterOrTransformation_UsesLegacyResources()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QTask task = Connect.CreateTask("task-name", "task-profile", 1);
            task.Resources = new List<QAbstractStorage> { bucket };
            await task.PreSubmitAsync(default);

            Assert.That(task._taskApi.AdvancedResourceBuckets, Is.Null.Or.Empty);

            Assert.That(task._taskApi.ResourceBuckets.Count, Is.EqualTo(1));
            Assert.That(task._taskApi.ResourceBuckets[0], Is.EqualTo("the-bucket"));
        }


        [Test]
        public async Task TestTaskBuildFromJson_FillsUpFilterAndTransformation_InApiProxyObject()
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

            Assert.That(secondBucket.BucketName, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
        }


        [Test]
        public async Task TestTaskBuildFromJson_FillsUpFilterAndTransformation_InSDKObject()
        {
            var task = await Connect.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task.Resources.Count(), Is.EqualTo(2));

            var firstBucket = task.Resources[0] as QBucket;
            var secondBucket = task.Resources[1] as QBucket;

            Assert.That(firstBucket.Shortname, Is.EqualTo("someBucket"));
            Assert.That(firstBucket.Filtering, Is.Not.Null);
            Assert.That(firstBucket.Filtering, Is.AssignableFrom(typeof(BucketFilteringPrefix)));
            Assert.That((firstBucket.Filtering as BucketFilteringPrefix).Prefix, Is.EqualTo("some/prefix/"));
            Assert.That(firstBucket.ResourcesTransformation, Is.Not.Null);
            Assert.That(firstBucket.ResourcesTransformation, Is.AssignableFrom(typeof(ResourcesTransformationStripPrefix)));
            Assert.That((firstBucket.ResourcesTransformation as ResourcesTransformationStripPrefix).Prefix, Is.EqualTo("transformed-prefix/"));

            Assert.That(secondBucket.Shortname, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
        }


        [Test]
        public async Task TestTaskBuildFromJson_FillsUpFilterAndTransformation_InSDKObject_WithLegacyResources()
        {
            var task = await ConnectLegacyBucket.RetrieveTaskByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(task.Resources.Count(), Is.EqualTo(2));

            var firstBucket = task.Resources[0] as QBucket;
            var secondBucket = task.Resources[1] as QBucket;

            Assert.That(firstBucket.Shortname, Is.EqualTo("someBucket"));
            Assert.That(firstBucket.Filtering, Is.Null);
            Assert.That(firstBucket.ResourcesTransformation, Is.Null);

            Assert.That(secondBucket.Shortname, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
        }

    }
}

namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class PoolTests
    {
        private const string StorageUrl = "http://storage";

        private const string ApiUrl = "http://api";

        private const string Token = "token";

        private Connection Connect { get; set; }

        private Connection ReadonlyConnect { get; set; }

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler()
            {
                ResponseBody = PoolTestsData.PoolResponseFullBody,
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
                Console.WriteLine("Looking for: " + methodCall + "_#_" + ApiUrl + partialPath);
                foreach (var r in HttpHandler.ParsedRequests)
                {
                    Console.WriteLine(r.Method + "_*_" + r.Uri);
                }
            }

            Assert.True(value);
        }

        [Test]
        public async Task GetNodeStatusCheckReturnBodyState()
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            await pool.InitializeAsync(Connect, poolApi);
            await pool.StartAsync("profile", 5);
            var ret = pool.GetNodeStatus(1);
            Assert.AreEqual(ret.State, "Execution");
        }

        [Test]
        public async Task GetNodeStatusShouldGetOnCorrectEndpoint()
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            await pool.InitializeAsync(Connect, poolApi);
            await pool.StartAsync("profile", 5);
            pool.GetNodeStatus(1);

            TestRequestAssert("POST", "pools");
            TestRequestAssert("GET", "pools/" + PoolTestsData.PoolResponseUuid);
        }

        [Test]
        public async Task GetPublicHostForApplicationPortCheckReturnHostValue()
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            await pool.InitializeAsync(Connect, poolApi);
            await pool.StartAsync("profile", 5);
            string ret = pool.GetPublicHostForApplicationPort(42);
            Assert.AreEqual(ret, "host23:23");
        }

        [Test]
        public async Task GetPublicHostForApplicationPortShouldGetOnCorrectEndpoint()
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            await pool.InitializeAsync(Connect, poolApi);
            await pool.StartAsync("profile", 5);
            pool.GetPublicHostForApplicationPort(42);

            TestRequestAssert("POST", "pools");
            TestRequestAssert("GET", "pools/" + PoolTestsData.PoolResponseUuid);
        }

        [Test]
        public async Task DeleteAsyncCheckNoFailWhenCall()
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            await pool.InitializeAsync(Connect, poolApi);
            Assert.AreNotEqual(pool.Uuid.ToString(), PoolTestsData.PoolConstTagUuidHandler);
            await pool.DeleteAsync();
        }

        [Test]
        public async Task DeleteAsyncShouldGetOnCorrectEndpoint()
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = new Guid(PoolTestsData.PoolResponseUuid) };
            await pool.InitializeAsync(Connect, poolApi);
            await pool.DeleteAsync();
            TestRequestAssert("DELETE", "pools/" + PoolTestsData.PoolResponseUuid);
        }

        [Test]
        public async Task UpdateStatusAsyncCheckReturnBody()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolConstTagHandler;
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            await pool.InitializeAsync(Connect, poolApi);
            Assert.AreNotEqual(pool.Uuid.ToString(), PoolTestsData.PoolConstTagUuidHandler);
            await pool.UpdateStatusAsync();
            Assert.AreEqual(pool.Uuid.ToString(), PoolTestsData.PoolConstTagUuidHandler);
            var dictConstraints = new[]
            {
                new KeyValHelper("constraintskey1", "constraintsvalue1"),
                new KeyValHelper("constraintskey2", "constraintsvalue2"),
            };
            var dictConstants = new[]
            {
                new KeyValHelper("constantskey1", "constantsvalue1"),
                new KeyValHelper("constantskey2", "constantsvalue2"),
            };
            var dictTag = new[] { "tag1", "tag2" };
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(pool.Constraints[dictConstraints[i].Key], dictConstraints[i].Value);
                Assert.AreEqual(pool.Constants[dictConstants[i].Key], dictConstants[i].Value);
                Assert.AreEqual(pool._poolApi.Tags[i], dictTag[i]);
            }
        }

        [Test]
        public async Task UpdateStatusAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolConstTagHandler;
            var pool = new QPool();
            var uuid = new Guid(PoolTestsData.PoolResponseUuid);
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            await pool.InitializeAsync(Connect, poolApi);
            await pool.UpdateStatusAsync();
            TestRequestAssert("GET", "pools/" + PoolTestsData.PoolResponseUuid);
        }

        [Test]
        public async Task StartAsyncCheckReturnBody()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolConstTagHandler;
            var pool = new QPool();
            var uuid = Guid.NewGuid();
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            await pool.InitializeAsync(Connect, poolApi);
            Assert.AreNotEqual(pool.Uuid.ToString(), PoolTestsData.PoolConstTagUuidHandler);
            await pool.StartAsync(profile: "test", 3);
            Assert.AreEqual(pool.Uuid.ToString(), PoolTestsData.PoolConstTagUuidHandler);
            Assert.AreEqual(pool.Profile, "AddConstTagHandler-Profile");
            Assert.AreEqual(pool.NodeCount, 3);

            var dictConstraints = new[]
            {
                new KeyValHelper("constraintskey1", "constraintsvalue1"),
                new KeyValHelper("constraintskey2", "constraintsvalue2"),
            };
            var dictConstants = new[]
            {
                new KeyValHelper("constantskey1", "constantsvalue1"),
                new KeyValHelper("constantskey2", "constantsvalue2"),
            };
            var dictTag = new[] { "tag1", "tag2" };
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(pool.Constraints[dictConstraints[i].Key], dictConstraints[i].Value);
                Assert.AreEqual(pool.Constants[dictConstants[i].Key], dictConstants[i].Value);
                Assert.AreEqual(pool._poolApi.Tags[i], dictTag[i]);
            }
        }

        [Test]
        public async Task StartAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolConstTagHandler;
            var pool = new QPool();
            var uuid = new Guid(PoolTestsData.PoolResponseUuid);
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            await pool.InitializeAsync(Connect, poolApi);
            await pool.StartAsync(profile: "test", 3);
            TestRequestAssert("POST", "pools");
        }

        [Test]
        public async Task CommitAsyncCheckReturnBody()
        {
            var pool = new QPool();
            var uuid = Guid.NewGuid();
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            var dict = new[]
            {
                new KeyValHelper("key1", "plop3"),
                new KeyValHelper("key2", "plop4"),
            };
            await pool.InitializeAsync(Connect, poolApi);
            pool.SetConstraint("key1", "plop3");
            pool.SetConstraint("key2", "plop4");
            await pool.CommitAsync();
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(pool._poolApi.Constraints[i].Key, dict[i].Key);
                Assert.AreEqual(pool._poolApi.Constraints[i].Value, dict[i].Value);
            }
        }

        [Test]
        public async Task CommitAsyncShouldGetOnCorrectEndpoint()
        {
            var pool = new QPool();
            var uuid = Guid.NewGuid();
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));

            await pool.InitializeAsync(Connect, poolApi);
            pool.SetConstraint("key1", "plop3");
            pool.SetConstraint("key2", "plop3");
            await pool.CommitAsync();

            TestRequestAssert("PUT", "pools");
        }

        [Test]
        public void SetPreparationTaskVerifyTheStoreValue()
        {
            var commanLine = "echo hello world";
            var pool = new QPool(Connect, Guid.NewGuid());
            var prepartionTask = new PoolPreparationTask(commanLine);
            pool.SetPreparationTask(prepartionTask);
            Assert.AreEqual(pool.PreparationCommandLine, commanLine);
        }

        [Test]
        public void PoolPreparationCommandLineVerifyTheStoreValue()
        {
            var commanLine = "echo hello world";
            var pool = new QPool(Connect, Guid.NewGuid());
            pool.PreparationCommandLine = commanLine;
            Assert.AreEqual(pool.PreparationCommandLine, commanLine);
        }

        [Test]
        public void CheckPoolPreparationCommandLineIsSendInThePoolRequest()
        {
            var commanLine = "echo hello world";
            var pool = new QPool(Connect, Guid.NewGuid());
            pool.PreparationCommandLine = commanLine;
            pool.StartAsync("profile", 5);
            Assert.IsTrue(HttpHandler.ParsedRequests.Any(request => request.Content.Contains("\"CommandLine\":\"" + commanLine + "\"")));
        }

        [Test]
        public void SetConstraintDeleteOneElement()
        {
            var pool = new QPool(Connect, Guid.NewGuid());
            var dict = new[]
            {
                new KeyValHelper("key1", "value1"),
                new KeyValHelper("key3", "value3"),
            };
            pool.SetConstraint("key1", "value1");
            pool.SetConstraint("key2", "value2");
            pool.SetConstraint("key3", "value3");
            pool.SetConstraint("key2", null);
            for (int i = 0; i < 2; i++)
            {
                Assert.AreEqual(pool.Constraints[dict[i].Key], dict[i].Value);
            }
        }

        [Test]
        public void SetConstraintVerifyTheAddOfNewValues()
        {
            var pool = new QPool(Connect, Guid.NewGuid());
            var dict = new[]
            {
                new KeyValHelper("key1", "value1"),
                new KeyValHelper("key2", "value2"),
                new KeyValHelper("key3", "value3"),
            };
            pool.SetConstraint("key1", "value1");
            pool.SetConstraint("key2", "value2");
            pool.SetConstraint("key3", "value3");
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(pool.Constraints[dict[i].Key], dict[i].Value);
            }
        }

        [Test]
        public void SetConstantVerifyTheDeleteOfAValue()
        {
            var pool = new QPool(Connect, Guid.NewGuid());
            var dict = new[]
            {
                new KeyValHelper("key1", "value1"),
                new KeyValHelper("key3", "value3"),
            };
            pool.SetConstant("key1", "value1");
            pool.SetConstant("key2", "value2");
            pool.SetConstant("key2", null);
            pool.SetConstant("key3", "value3");
            foreach (var d in dict)
            {
                Assert.AreEqual(pool.Constants[d.Key], d.Value);
            }
        }

        [Test]
        public void SetConstantVerifyTheAddOfAValue()
        {
            var pool = new QPool(Connect, Guid.NewGuid());
            var dict = new[]
            {
                new KeyValHelper("key1", "value1"),
                new KeyValHelper("key2", "value2"),
                new KeyValHelper("key3", "value3"),
            };
            pool.SetConstant("key1", "value1");
            pool.SetConstant("key2", "value2");
            pool.SetConstant("key3", "value3");
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(pool.Constants[dict[i].Key], dict[i].Value);
            }
        }

        [Test]
        public async Task CheckCoreCountTestValues()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolResponseFullBody;
            string uuid = Guid.NewGuid().ToString();
            QPool pool = new QPool(Connect, uuid);
            await pool.UpdateStatusAsync();
            Assert.AreEqual(pool.Status.RunningInstancesInfo.PerRunningInstanceInfo[0].CoreCount, 8);
            Assert.AreEqual(pool.Status.RunningInstancesInfo.PerRunningInstanceInfo[1].CoreCount, 8);
            Assert.AreEqual(pool.Status.RunningInstancesInfo.PerRunningInstanceInfo[2].CoreCount, 0);
        }

        [Test]
        public void SetTagsFailWhenNullArgument()
        {
            var pool = new QPool(Connect, Guid.NewGuid());
            var ex = Assert.Throws<ArgumentNullException>(() => pool.SetTags(null));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void SetTagsVerifyTheAddOfNewValues()
        {
            var pool = new QPool(Connect, Guid.NewGuid());
            var tags = new[] { "tag1", "tag2", "tag3" };
            pool.SetTags(tags);
            CollectionAssert.AreEquivalent(pool.Tags, tags);
        }

        [Test]
        public async Task CreateAsyncWhenCreatingANewPoolCheckIfUuidAreDifferent()
        {
            var uuid = Guid.NewGuid();
            var poolApi = new PoolApi() { Uuid = uuid };
            var pool1 = new QPool(Connect, poolApi);
            var pool2 = await QPool.CreateAsync(Connect, poolApi);
            Assert.AreEqual(pool1.Uuid, pool2.Uuid);
            Assert.AreNotEqual(pool1, pool2);
        }

        [Test]
        public async Task InitializeAsyncVerifyIfUuidIsIdentic()
        {
            var uuid = Guid.NewGuid();
            var poolApi = new PoolApi() { Uuid = uuid };
            var pool = new QPool();
            await pool.InitializeAsync(Connect, poolApi);
            Assert.AreEqual(uuid, pool.Uuid);
        }

        [Test]
        public void CheckAllPoolConstructorsGetTheGoodArguments()
        {
            var pool0 = new QPool();
            var pool1 = new QPool(Connect, Guid.NewGuid());
            var poolApi = new PoolApi() { Uuid = pool1.Uuid };
            var pool2 = new QPool(Connect, poolApi);
            Assert.AreEqual(pool1.Uuid, pool2.Uuid);
            var pool3 = new QPool(Connect, name: "name1", profile: "profile", initialNodeCount: 5, shortname: "shortname");
            Assert.AreEqual(pool3.Name, "name1");
            var pool4 = new QPool(Connect, name: "name2");
            Assert.AreEqual(pool4.Name, "name2");
        }

        [Test]
        public async Task UpdateResourcesAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolConstTagHandler;
            var pool = new QPool();
            var uuid = new Guid(PoolTestsData.PoolResponseUuid);
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            await pool.InitializeAsync(Connect, poolApi);

            await pool.UpdateResourcesAsync();
            TestRequestAssert("PATCH", "pools/" + PoolTestsData.PoolResponseUuid);
        }

        [Test]
        public async Task UpdateResourcesShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolConstTagHandler;
            var pool = new QPool();
            var uuid = new Guid(PoolTestsData.PoolResponseUuid);
            var poolApi = new PoolApi() { Uuid = uuid };
            poolApi.Constraints.Add(new KeyValHelper("key1", "value1"));
            poolApi.Constraints.Add(new KeyValHelper("key2", "value2"));
            await pool.InitializeAsync(Connect, poolApi);

            pool.UpdateResources();
            TestRequestAssert("PATCH", "pools/" + PoolTestsData.PoolResponseUuid);
        }

        [Test]
        public async Task GetNodeStatusListReturnTheGoodList()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolResponseFullBody;
            var tolerance = 0.0001;
            string uuid = Guid.NewGuid().ToString();
            QPool pool = new QPool(Connect, uuid);
            await pool.UpdateStatusAsync();
            var pollNodeStatusList = pool.GetNodeStatusList();

            Assert.AreEqual(pollNodeStatusList[0].State, "Execution");
            Assert.AreEqual(pollNodeStatusList[0].Progress, 0);
            Assert.That(pollNodeStatusList[0].ExecutionTimeGHz, Is.EqualTo(341.31247).Within(tolerance));

            Assert.AreEqual(pollNodeStatusList[1].State, "Execution");
            Assert.AreEqual(pollNodeStatusList[1].Progress, 0);
            Assert.That(pollNodeStatusList[1].ExecutionTimeGHz, Is.EqualTo(340.12228).Within(tolerance));

            Assert.AreEqual(pollNodeStatusList[2].State, "Execution");
            Assert.AreEqual(pollNodeStatusList[2].Progress, 0);
            Assert.That(pollNodeStatusList[2].ExecutionTimeGHz, Is.EqualTo(0).Within(tolerance));
        }

        [Test]
        [Category("PoolResourcesSync")]
        public async Task TestPoolBuildFromJson_FillsUpTaskDefaultWaitForPoolSynchronization_WithNull_WhenAbsentFromResponse()
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolResponseWithAdvancedBucketsFullBody;
            var pool = await Connect.RetrievePoolByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(pool.TaskDefaultWaitForPoolResourcesSynchronization, Is.Null);
        }

        [Test]
        [Category("PoolResourcesSync")]
        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public async Task TestPoolBuildFromJson_FillsUpTaskDefaultWaitForPoolSynchronization_WithValue_WhenPresentInResponse(bool? wait)
        {
            HttpHandler.ResponseBody = PoolTestsData.PoolResponse_WithTaskDefaultWaitForPoolResourcesSynchronization(wait);
            var pool = await Connect.RetrievePoolByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(pool.TaskDefaultWaitForPoolResourcesSynchronization, Is.EqualTo(wait));
        }


        [Test]
        [Category("PoolResourcesSync")]
        public void TestPoolWithoutTaskDefaultWaitForPoolSynchronization_LeavesTaskApiFieldNull()
        {
            QPool pool = Connect.CreatePool("pool-name", "pool-profile", 1);
            pool.PreSubmit(default);

            Assert.That(pool._poolApi.TaskDefaultWaitForPoolResourcesSynchronization, Is.Null);
        }


        [Test]
        [Category("PoolResourcesSync")]
        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public void TestPoolWithTaskDefaultWaitForPoolSynchronization_FillsTaskApiField(bool? wait)
        {
            QPool pool = Connect.CreatePool("pool-name", "pool-profile", 1, taskDefaultWaitForPoolResourcesSynchronization: wait);
            pool.PreSubmit(default);

            Assert.That(pool._poolApi.TaskDefaultWaitForPoolResourcesSynchronization, Is.EqualTo(wait));
        }
    }

    [TestFixture]
    public class PoolTestsAdvancedResources
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
                ResponseBody = PoolTestsData.PoolResponseWithAdvancedBucketsFullBody,
            };
            Connect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler);

            HttpHandlerLegacyBucket = new InterceptingFakeHttpHandler()
            {
                ResponseBody = PoolTestsData.PoolResponseWithLegacyBucketsFullBody,
            };
            ConnectLegacyBucket = new Connection(ApiUrl, StorageUrl, Token, HttpHandlerLegacyBucket);
        }


        [Test]
        public void TestPoolPreSubmit_WithPrefixFiltering_FillsUpApiPool()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QPool pool = Connect.CreatePool("task-name", "task-profile", 1);
            pool.Resources = new List<QAbstractStorage> { bucket.WithFiltering(new BucketFilteringPrefix(prefix)) };
            pool.PreSubmit(default);

            Assert.That(pool._poolApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, pool._poolApi.AdvancedResourceBuckets.Count);

            var apiResource = pool._poolApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.Filtering.PrefixFiltering);
            Assert.AreEqual(prefix, apiResource.Filtering.PrefixFiltering.Prefix);
        }


        [Test]
        public void TestPoolPreSubmit_WithResourcesTransformation_FillsUpApiPool()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QPool pool = Connect.CreatePool("task-name", "task-profile", 1);
            pool.Resources = new List<QAbstractStorage> { bucket.WithResourcesTransformation(new ResourcesTransformationStripPrefix(prefix)) };
            pool.PreSubmit(default);

            Assert.That(pool._poolApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, pool._poolApi.AdvancedResourceBuckets.Count);

            var apiResource = pool._poolApi.AdvancedResourceBuckets[0];

            Assert.AreEqual("the-bucket", apiResource.BucketName);
            Assert.IsNull(apiResource.Filtering);
            Assert.IsNotNull(apiResource.ResourcesTransformation);
            Assert.IsNotNull(apiResource.ResourcesTransformation.StripPrefix);
            Assert.AreEqual(prefix, apiResource.ResourcesTransformation.StripPrefix.Prefix);
        }


        // Check that chaining both With* calls is correct
        [Test]
        public void TestTaskPreSubmitAsync_WithPrefixFiltering_AndResourcesTransformation_FillsUpApiTask()
        {
            string prefix = "filter/prefix";
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QPool pool = Connect.CreatePool("pool-name", "pool-profile", 1);
            pool.Resources = new List<QAbstractStorage> {
                bucket.WithFiltering(new BucketFilteringPrefix(prefix))
                      .WithResourcesTransformation(new ResourcesTransformationStripPrefix(prefix)),
            };
            pool.PreSubmit(default);

            Assert.That(pool._poolApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.AreEqual(1, pool._poolApi.AdvancedResourceBuckets.Count);

            var apiResource = pool._poolApi.AdvancedResourceBuckets[0];

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
        public void TestPoolPreSubmit_WithoutFilterOrTransformation_UsesLegacyResources()
        {
            QBucket bucket = new QBucket(Connect, "the-bucket", create: false);
            QPool pool = Connect.CreatePool("task-name", "task-profile", 1);
            pool.Resources = new List<QAbstractStorage> { bucket };
            pool.PreSubmit(default);

            Assert.That(pool._poolApi.AdvancedResourceBuckets, Is.Null.Or.Empty);

            Assert.That(pool._poolApi.ResourceBuckets.Count, Is.EqualTo(1));
            Assert.That(pool._poolApi.ResourceBuckets[0], Is.EqualTo("the-bucket"));
        }


        [Test]
        public async Task TestBuildFromJson_FillsUpFilterAndTransformation_InApiProxyObject()
        {
            var pool = await Connect.RetrievePoolByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(pool._poolApi.ResourceBuckets, Is.Null.Or.Empty);
            Assert.That(pool._poolApi.AdvancedResourceBuckets.Count, Is.EqualTo(2));

            var firstBucket = pool._poolApi.AdvancedResourceBuckets[0];
            var secondBucket = pool._poolApi.AdvancedResourceBuckets[1];

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
        public async Task TestBuildFromJson_FillsUpFilterAndTransformation_InSDKObject()
        {
            var pool = await Connect.RetrievePoolByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(pool.Resources.Count, Is.EqualTo(2));

            var firstBucket = pool.Resources[0] as QBucket;
            var secondBucket = pool.Resources[1] as QBucket;

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
        public async Task TestPoolBuildFromJson_FillsUpFilterAndTransformation_InSDKObject_WithLegacyResources()
        {
            var pool = await ConnectLegacyBucket.RetrievePoolByUuidAsync("11111111-1111-1111-1111-111111111111");

            Assert.That(pool.Resources.Count, Is.EqualTo(2));

            var firstBucket = pool.Resources[0] as QBucket;
            var secondBucket = pool.Resources[1] as QBucket;

            Assert.That(firstBucket.Shortname, Is.EqualTo("someBucket"));
            Assert.That(firstBucket.Filtering, Is.Null);
            Assert.That(firstBucket.ResourcesTransformation, Is.Null);

            Assert.That(secondBucket.Shortname, Is.EqualTo("someOtherBucket"));
            Assert.That(secondBucket.Filtering, Is.Null);
            Assert.That(secondBucket.ResourcesTransformation, Is.Null);
        }
    }
}

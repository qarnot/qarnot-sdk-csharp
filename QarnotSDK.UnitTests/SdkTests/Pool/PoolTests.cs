namespace QarnotSDK.UnitTests
{
    using System;
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
    }
}

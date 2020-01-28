namespace QarnotSDK.UnitTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class JobTests
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
                ResponseBody = JobTestsData.JobResponseBody,
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
            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains(methodCall, StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ApiUrl}/{partialPath}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public void DeleteAsyncNoReadRightsThrowException()
        {
            var job = new QJob(ReadonlyConnect, Guid.NewGuid());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await job.DeleteAsync());
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task DeleteAsyncForceShouldGetOnCorrectEndpoint()
        {
            string uuid = JobTestsData.JobResponseUuid;
            string method = "DELETE";
            string uri = "jobs/" + uuid + "?force=true";

            QJob job = new QJob(Connect, new Guid(uuid));
            await job.DeleteAsync(force: true);

            TestRequestAssert(method, uri);
        }

        [Test]
        public async Task DeleteAsyncShouldGetOnCorrectEndpoint()
        {
            string uuid = JobTestsData.JobResponseUuid;
            string method = "DELETE";
            string uri = "jobs/" + uuid;

            QJob job = new QJob(Connect, new Guid(uuid));
            await job.DeleteAsync();

            TestRequestAssert(method, uri);
        }

        [Test]
        public async Task TerminateAsyncCheckRequestAndResponseUuid()
        {
            string uuid = JobTestsData.JobResponseUuid;
            string method = "POST";
            string uri = "jobs/" + uuid + "/terminate";

            var job = new QJob(Connect, new Guid(uuid));
            await job.TerminateAsync();
            Assert.AreEqual(uuid, job.Uuid.ToString());
            TestRequestAssert(method, uri);
        }

        [Test]
        public void TerminateAsyncNoReadRightsThrowException()
        {
            var job = new QJob(ReadonlyConnect, Guid.NewGuid());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await job.TerminateAsync());
            Assert.IsNotNull(ex);
        }

        [Test]
        public async Task UpdateStatusAsyncCheckRequestAndResponseUuid()
        {
            string method = "GET";
            string uri = "jobs/" + JobTestsData.JobSwitchUuid;

            var job = new QJob(Connect, new Guid(JobTestsData.JobSwitchUuid));
            await job.UpdateStatusAsync();
            Assert.AreEqual(JobTestsData.JobResponseUuid, job.Uuid.ToString());
            TestRequestAssert(method, uri);
        }

        [Test]
        public async Task SubmitAsyncCheckRequestsAndResponseUuid()
        {
            var job = new QJob(Connect, Guid.NewGuid());

            await job.SubmitAsync();
            Assert.AreEqual(JobTestsData.JobResponseUuid, job.Uuid.ToString());

            string method = "POST";
            string uri = "jobs";
            TestRequestAssert(method, uri);
            method = "GET";
            uri = "jobs/" + JobTestsData.JobResponseUuid;
            TestRequestAssert(method, uri);
        }

        [Test]
        public void SubmitAsyncNoReadRightsThrowException()
        {
            var job = new QJob(ReadonlyConnect, Guid.NewGuid());
            Exception ex = Assert.ThrowsAsync<Exception>(async () => await job.SubmitAsync());
            Assert.IsNotNull(ex);
        }

        [Test]
        public void VerifyTimeSpanSetAndGet()
        {
            var job = new QJob(Connect, Guid.NewGuid());
            var time = new TimeSpan(2, 14, 18);
            Assert.AreNotEqual(job.MaximumWallTime, time);
            Assert.AreEqual(job.MaximumWallTime, default(TimeSpan));
            job.MaximumWallTime = time;
            Assert.AreEqual(job.MaximumWallTime, time);
        }

        [Test]
        public void CheckJobConstructorsArgumentsAndBodies()
        {
            var uuid = Guid.NewGuid();
            var jobApi = new JobApi() { Uuid = uuid };
            var pool = new QPool(Connect, uuid);
            var job1 = new QJob(Connect, uuid);
            var job2 = new QJob(Connect, jobApi);
            Assert.AreEqual(job1.Uuid, job2.Uuid);

            var job3 = new QJob(Connect, name: "name1", pool: pool, shortname: "shortname", UseTaskDependencies: true);
            Assert.AreEqual(job3.Name, "name1");
            Assert.AreEqual(job3.Shortname, "shortname");
            Assert.AreEqual(job3.PoolUuid, uuid);
            Assert.AreEqual(job3.UseDependencies, true);

            var job4 = new QJob(Connect, name: "name2");
            Assert.AreEqual(job4.Name, "name2");
            Assert.AreEqual(job4.Shortname, null);
            Assert.AreEqual(job4.PoolUuid, default(Guid));
            Assert.AreEqual(job4.UseDependencies, false);
        }
    }
}

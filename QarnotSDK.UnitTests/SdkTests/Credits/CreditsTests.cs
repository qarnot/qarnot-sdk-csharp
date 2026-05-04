namespace QarnotSDK.UnitTests.SdkTests.Budget
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK.UnitTests.SdkTests.Credits;

    [TestFixture]
    [Category("Credits")]
    public class CreditsTests
    {
        private const string StorageUrl = "http://storage";
        private const string ApiUrl = "http://api";
        private const string Token = "token";

        private Connection Connect { get; set; }

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler()
            {
                ResponseBody = CreditsTestData.CreditsResponseBody,
            };
            Connect = new Connection(ApiUrl, StorageUrl, Token, HttpHandler)
            {
                StorageAccessKey = "fake@mail.com",
            };
        }

        [TearDown]
        public void TearDown()
        {
            HttpHandler.Dispose();
        }

        [Test]
        public async Task TestGetTaskCreditsCallsCorrectEndpoint()
        {
            var taskUuid = Guid.NewGuid();
            var credits = await Connect.CreditsClient.GetTaskCreditsAsync(taskUuid);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/tasks/{taskUuid}/credits"));
        }

        [Test]
        public async Task TestGetPoolCreditsCallsCorrectEndpoint()
        {
            var poolUuid = Guid.NewGuid();
            var credits = await Connect.CreditsClient.GetPoolCreditsAsync(poolUuid);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/pools/{poolUuid}/credits"));
        }

        [Test]
        public async Task TestGetAccountCreditsCallsCorrectEndpoint()
        {
            var credits = await Connect.CreditsClient.GetAccountCreditsAsync();

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/accounts/credits"));
        }

        [Test]
        public async Task TestCreditsDeserialization()
        {
            var credits = await Connect.CreditsClient.GetAccountCreditsAsync();

            Assert.IsNotNull(credits);
            Assert.That(credits.CreditsInEuros, Is.EqualTo(CreditsTestData.CreditsInEuro));
            Assert.That(credits.CreditsInCents, Is.EqualTo(CreditsTestData.CreditsInEuro * 100));
        }

        [Test]
        public async Task TestTaskGetConsumedCredits()
        {
            string name = Guid.NewGuid().ToString();
            QTask task = new QTask(Connect, name);
            HttpHandler.ResponseBody = TaskTestsData.TaskResponseFullBody;
            await task.UpdateStatusAsync();

            HttpHandler.ResponseBody = CreditsTestData.CreditsResponseBody;
            var credits = await task.GetConsumedCreditsAsync();

            var request = HttpHandler.ParsedRequests.FirstOrDefault(r => r.Uri.Contains("credits"));
            Assert.That(request.Uri, Does.Contain($"credits/v1/tasks/{TaskTestsData.TaskResponseUuid}/credits"));

            Assert.That(credits.CreditsInEuros, Is.EqualTo(CreditsTestData.CreditsInEuro));
        }

        [Test]
        public async Task TestPoolGetConsumedCredits()
        {
            string name = Guid.NewGuid().ToString();
            QPool pool = new QPool(Connect, name);
            HttpHandler.ResponseBody = PoolTestsData.PoolResponseFullBody;
            await pool.UpdateStatusAsync();

            HttpHandler.ResponseBody = CreditsTestData.CreditsResponseBody;

            var credits = await pool.GetConsumedCreditsAsync();

            var request = HttpHandler.ParsedRequests.FirstOrDefault(r => r.Uri.Contains("credits"));
            Assert.That(request.Uri, Does.Contain($"credits/v1/pools/{PoolTestsData.PoolResponseUuid}/credits"));

            Assert.That(credits.CreditsInEuros, Is.EqualTo(CreditsTestData.CreditsInEuro));
        }

        [Test]
        public async Task TestAccountGetCredits()
        {
            var credits = await Connect.GetCreditsAsync();

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/accounts/credits"));

            Assert.That(credits.CreditsInEuros, Is.EqualTo(CreditsTestData.CreditsInEuro));
        }
    }
}

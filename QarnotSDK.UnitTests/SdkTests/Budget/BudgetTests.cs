namespace QarnotSDK.UnitTests.SdkTests.Budget
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK.Sdk;

    [TestFixture]
    [Category("Budget")]
    public class BudgetTests
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
                ResponseBody = BudgetTestData.BudgetResponseBody,
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
        public async Task TestGetActiveBudgetsCallsCorrectEndpoint()
        {
            var projectUuid = Guid.NewGuid();
            var budgets = await Connect.CreditsClient.GetBudgetsAsync(projectUuid, activeOnly: true);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/projects/{projectUuid}/budgets"));
            Assert.That(request.Uri, Does.Contain("activeOnly=true"));
        }

        [Test]
        public async Task TestGetAllBudgetsCallsCorrectEndpoint()
        {
            var projectUuid = Guid.NewGuid();
            var budgets = await Connect.CreditsClient.GetBudgetsAsync(projectUuid, activeOnly: false);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/projects/{projectUuid}/budgets"));
            Assert.That(request.Uri, Does.Contain("?activeOnly=false"));
        }

        [Test]
        public async Task TestBudgetDeserialization()
        {
            var projectUuid = Guid.NewGuid();
            var budgets = await Connect.CreditsClient.GetBudgetsAsync(projectUuid, activeOnly: true);

            Assert.IsNotNull(budgets);
            Assert.That(budgets, Has.Count.EqualTo(1), "Should deserialize one budget from the response");

            var budget = budgets.First();
            Assert.That(budget.Uuid, Is.EqualTo(new Guid(BudgetTestData.BudgetUuid)));
            Assert.That(budget.Alias, Is.EqualTo(BudgetTestData.BudgetAlias));
            Assert.That(budget.ConsumedAmountInCents, Is.EqualTo(BudgetTestData.ConsumedAmountInCents));
            Assert.That(budget.TotalAmountInCents, Is.EqualTo(BudgetTestData.TotalAmountInCents));
            Assert.That(budget.RemainingAmountInCents, Is.EqualTo(BudgetTestData.RemainingAmountInCents));
            Assert.That(budget.StartDateUtc.ToUniversalTime(), Is.EqualTo(DateTime.Parse(BudgetTestData.StartDateUtc).ToUniversalTime()));
            Assert.That(budget.ExpirationDateUtc?.ToUniversalTime(), Is.EqualTo(DateTime.Parse(BudgetTestData.ExpirationDateUtc).ToUniversalTime()));
            Assert.That(budget.BudgetOverrunPolicy, Is.EqualTo(BudgetOverrunPolicy.AlertOnly));
            Assert.That(budget.IsArchived, Is.EqualTo(BudgetTestData.IsArchived));
        }

        [Test]
        public async Task TestEmptyBudgetsResponse()
        {
            HttpHandler.ResponseBody = BudgetTestData.EmptyBudgetsResponseBody;

            var projectUuid = Guid.NewGuid();
            var budgets = await Connect.CreditsClient.GetBudgetsAsync(projectUuid, activeOnly: true);

            Assert.That(budgets, Is.Not.Null);
            Assert.That(budgets, Has.Count.EqualTo(0), "Should return empty list when no budgets are available");
        }

        [Test]
        public async Task TestProjectRetrieveActiveBudgets()
        {
            var projectApi = new ProjectApi() { Uuid = Guid.NewGuid(), Name = "test-project", Slug = "test" };
            var project = new QProject(projectApi);

            var budgets = await project.RetrieveActiveBudgetsAsync(Connect);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/projects/{projectApi.Uuid}/budgets"));
            Assert.That(request.Uri, Does.Contain("activeOnly=true"));

            Assert.That(budgets, Has.Count.EqualTo(1));
        }

        [Test]
        public async Task TestProjectRetrieveAllBudgets()
        {
            var projectApi = new ProjectApi() { Uuid = Guid.NewGuid(), Name = "test-project", Slug = "test" };
            var project = new QProject(projectApi);

            var budgets = await project.RetrieveAllBudgetsAsync(Connect);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            Assert.That(request.Uri, Does.Contain($"credits/v1/projects/{projectApi.Uuid}/budgets"));
            Assert.That(request.Uri, Does.Contain("?activeOnly=false"));

            Assert.That(budgets, Has.Count.EqualTo(1));
        }

        [Test]
        public void TestProjectRetrieveActiveBudgetsThrowsWithoutConnection()
        {
            var projectApi = new ProjectApi() { Uuid = Guid.NewGuid(), Name = "test-project", Slug = "test" };
            var project = new QProject(projectApi);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await project.RetrieveActiveBudgetsAsync(null));
        }

        [Test]
        public void TestProjectRetrieveAllBudgetsThrowsWithoutConnection()
        {
            var projectApi = new ProjectApi() { Uuid = Guid.NewGuid(), Name = "test-project", Slug = "test" };
            var project = new QProject(projectApi);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await project.RetrieveAllBudgetsAsync(null));
        }
    }
}

namespace QarnotSDK.UnitTests.SdkTests.Carbon
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    [Category("CarbonFacts")]
    public class CarbonFactsTest
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
                ResponseBody = CarbonFactsTestData.CarbonFactsResponseBody,
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

        [TestCase(null)]
        [TestCase("test-datacenter")]
        public async Task TestGetCarbonFactsOfTask(string datacenterInParameter)
        {
            QTask task = new QTask(Connect, "testTaskCarbonFacts");
            var taskApi = new TaskApi() { Uuid = Guid.NewGuid() };
            await task.InitializeAsync(Connect, taskApi);
            var carbonFacts = await task.GetCarbonFactsAsync(datacenterInParameter);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            StringAssert.Contains($"tasks/{taskApi.Uuid}/carbon-facts", request.Uri);
            if (datacenterInParameter != null)
            {
                StringAssert.Contains($"?comparisonDatacenter={datacenterInParameter}", request.Uri);
            }
            else
            {
                StringAssert.DoesNotContain($"?comparisonDatacenter", request.Uri);
            }

            Assert.IsNotNull(carbonFacts);
            Assert.AreEqual(CarbonFactsTestData.ExpectedConsumedEnergy, carbonFacts.TotalConsumedEnergyWh);
            Assert.AreEqual(CarbonFactsTestData.ExpectedEnergyIT, carbonFacts.TotalEnergyITWh);
            Assert.AreEqual(CarbonFactsTestData.ExpectedReuseEnergy, carbonFacts.TotalReuseEnergyWh);
            Assert.AreEqual(CarbonFactsTestData.ExpectedCarbonFootprint, carbonFacts.CarbonFootprint_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedEquivalentDCName, carbonFacts.EquivalentDataCenterName);
            Assert.AreEqual(CarbonFactsTestData.ExpectedEquivalentDCCarbonFootprint, carbonFacts.EquivalentDCCarbonFootprint_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintCompute, carbonFacts.SavedCarbonFootprintCompute_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintHeat, carbonFacts.SavedCarbonFootprintHeat_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintComputeHeat, carbonFacts.SavedCarbonFootprintComputeHeat_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintPercent, carbonFacts.SavedCarbonFootprintPercentage);
            Assert.AreEqual(CarbonFactsTestData.ExpectedPUE, carbonFacts.PUE);
            Assert.AreEqual(CarbonFactsTestData.ExpectedERE, carbonFacts.ERE);
            Assert.AreEqual(CarbonFactsTestData.ExpectedERF, carbonFacts.ERF);
            Assert.AreEqual(CarbonFactsTestData.ExpectedWUE, carbonFacts.WUE);
        }

        [TestCase(null)]
        [TestCase("test-datacenter")]
        public async Task TestGetCarbonFactsOfPool(string datacenterInParameter)
        {
            var pool = new QPool();
            var poolApi = new PoolApi() { Uuid = Guid.NewGuid() };
            await pool.InitializeAsync(Connect, poolApi);
            var carbonFacts = await pool.GetCarbonFactsAsync(datacenterInParameter);

            var request = HttpHandler.ParsedRequests.FirstOrDefault();
            StringAssert.Contains($"pools/{poolApi.Uuid}/carbon-facts", request.Uri);
            if (datacenterInParameter != null)
            {
                StringAssert.Contains($"?comparisonDatacenter={datacenterInParameter}", request.Uri);
            }
            else
            {
                StringAssert.DoesNotContain($"?comparisonDatacenter", request.Uri);
            }

            Assert.IsNotNull(carbonFacts);
            Assert.AreEqual(CarbonFactsTestData.ExpectedConsumedEnergy, carbonFacts.TotalConsumedEnergyWh);
            Assert.AreEqual(CarbonFactsTestData.ExpectedEnergyIT, carbonFacts.TotalEnergyITWh);
            Assert.AreEqual(CarbonFactsTestData.ExpectedReuseEnergy, carbonFacts.TotalReuseEnergyWh);
            Assert.AreEqual(CarbonFactsTestData.ExpectedCarbonFootprint, carbonFacts.CarbonFootprint_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedEquivalentDCCarbonFootprint, carbonFacts.EquivalentDCCarbonFootprint_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintCompute, carbonFacts.SavedCarbonFootprintCompute_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintHeat, carbonFacts.SavedCarbonFootprintHeat_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintComputeHeat, carbonFacts.SavedCarbonFootprintComputeHeat_gCO2Eq);
            Assert.AreEqual(CarbonFactsTestData.ExpectedSavedCarbonFootprintPercent, carbonFacts.SavedCarbonFootprintPercentage);
            Assert.AreEqual(CarbonFactsTestData.ExpectedPUE, carbonFacts.PUE);
            Assert.AreEqual(CarbonFactsTestData.ExpectedERE, carbonFacts.ERE);
            Assert.AreEqual(CarbonFactsTestData.ExpectedERF, carbonFacts.ERF);
            Assert.AreEqual(CarbonFactsTestData.ExpectedWUE, carbonFacts.WUE);
        }
    }
}
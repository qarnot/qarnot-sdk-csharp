namespace QarnotSDK.UnitTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class PaginatedResponseTests
    {
        private PaginatedResponseAPI<int> CreateApiObj()
        {
            PaginatedResponseAPI<int> api = new PaginatedResponseAPI<int>();
            api.Token = "token";
            api.NextToken = "next";
            api.IsTruncated = true;
            api.Data = new List<int>() { 0, 1, 2, 3, 400 };
            return api;
        }

        private void TestSdkPaginatedResponseFromApiObj(PaginatedResponse<string> response)
        {
            Assert.AreEqual(response.Data[0], "0");
            Assert.AreEqual(response.Data[1], "1");
            Assert.AreEqual(response.Data[2], "2");
            Assert.AreEqual(response.Data[3], "3");
            Assert.AreEqual(response.Data[4], "400");
            Assert.AreEqual(response.Token, "token");
            Assert.AreEqual(response.NextToken, "next");
            Assert.AreEqual(response.IsTruncated, true);
        }

        [Test]
        public async Task TestCreateANewAsyncResponseFormAPIRespose()
        {
            Connection connection = null;
            PaginatedResponseAPI<int> api = CreateApiObj();
            PaginatedResponse<string> response = await PaginatedResponse<string>.CreateAsync<int>(connection, api, (conn, nbr) => Task.FromResult(nbr.ToString()));
            TestSdkPaginatedResponseFromApiObj(response);
        }
    }
}

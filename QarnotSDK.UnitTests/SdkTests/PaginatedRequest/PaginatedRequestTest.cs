namespace QarnotSDK.UnitTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class PaginatedRequestTests
    {
        [Test]
        public void TestPrepareNextPageSetTheResponseToken()
        {
            var response = new PaginatedResponse<QPool>("", "next_token", true);
            var request = new PaginatedRequest<QPool>(5, "token", QFilter<QPool>.Gt(t => t.Name, "name"));
            Assert.IsTrue(request.PrepareNextPage(response));
            Assert.AreEqual(request.Token, response.NextToken);
        }
    }
}

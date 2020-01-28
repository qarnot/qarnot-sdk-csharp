namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class UtilsTests
    {
        private HttpClient Client { get; set; }

        [SetUp]
        public void SetUp()
        {
            Client = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            Client.Dispose();
        }

        [Test]
        public void LookForErrorAndThrowAsyncCheckAllHttpStatusCodeLowerthan200()
        {
            foreach (HttpStatusCode elem in Enum.GetValues(typeof(HttpStatusCode)))
            {
                if ((int)elem < 200)
                {
                    using var response = new HttpResponseMessage(elem);
                    QarnotApiException tmpExcept = Assert.ThrowsAsync<QarnotApiException>(async () => await Utils.LookForErrorAndThrowAsync(Client, response, default).ConfigureAwait(false));
                    Assert.IsNotNull(tmpExcept);
                }
            }
        }

        [Test]
        public void LookForErrorAndThrowAsyncCheckAllInvalidHttpStatusCodeUpperthan300()
        {
            foreach (HttpStatusCode elem in Enum.GetValues(typeof(HttpStatusCode)))
            {
                if ((int)elem >= 300)
                {
                    using var response = new HttpResponseMessage(elem);

                    if ((int)elem == 404)
                    {
                        QarnotApiResourceNotFoundException tmpExcept = Assert.ThrowsAsync<QarnotApiResourceNotFoundException>(async () => await Utils.LookForErrorAndThrowAsync(Client, response, default).ConfigureAwait(false));
                        Assert.IsNotNull(tmpExcept);
                    }
                    else
                    {
                        QarnotApiException tmpExcept2 = Assert.ThrowsAsync<QarnotApiException>(async () => await Utils.LookForErrorAndThrowAsync(Client, response, default).ConfigureAwait(false));
                        Assert.IsNotNull(tmpExcept2);
                    }
                }
            }
        }

        [Test]
        public void LookForErrorAndThrowAsyncCheckAllValidHttpStatusCode()
        {
            foreach (HttpStatusCode elem in Enum.GetValues(typeof(HttpStatusCode)))
            {
                if ((int)elem >= 200 && (int)elem < 300)
                {
                    using var response = new HttpResponseMessage(elem);
                    Utils.LookForErrorAndThrowAsync(Client, response, default).ConfigureAwait(false);
                }
            }
        }

        [Test]
        public void IsNullOrEmptyFunctionTestIsNullRetrunTrue()
        {
            List<string> test = null;
            Assert.True(test.IsNullOrEmpty());
            List<string> test2 = new List<string>();
            Assert.True(test2.IsNullOrEmpty());
        }

        [Test]
        public void IsNullOrEmptyFunctionTestIsNotNullRetrunFalse()
        {
            List<string> test3 = new List<string>()
            { "test" };
            Assert.False(test3.IsNullOrEmpty());
            List<string> test4 = new List<string>()
            { "test", "test", "test", "test", "test", "test", "test", "test" };
            Assert.False(test4.IsNullOrEmpty());
        }
    }
}

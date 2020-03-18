namespace QarnotSDK.UnitTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class NoRetryHandlerTests
    {
        [Test]
        public void NoRetryHandlerSendAsyncNotFailPass()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = 0;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new NoRetryHandler();
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            job0.Submit();
        }

        [Test]
        public void NoRetryHandlerSendAsyncFailOneTimeThrowError()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = 1;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new NoRetryHandler();
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            Exception ex = Assert.Throws<QarnotApiException>(() => job0.Submit());
            Assert.IsNotNull(ex);
        }
    }

    public class UnitTestExponentialRetryHandler
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "UnitTest are not multilangues.")]
        [Test]
        public async Task SendAsyncCheckTimeAfter6Fail()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = 6;
            int nbrOfTry = nbrOfFail + 1;
            int milliSecondWaitTime = 20;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new ExponentialRetryHandler(nbrOfTry, milliSecondWaitTime);
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            DateTime now = DateTime.Now;
            int j = 0;
            for (int i = 0; i <= nbrOfFail; i++)
            {
                j += i;
            }

            DateTime min_time = now.AddMilliseconds((double)(milliSecondWaitTime * j));
            DateTime max_time = now.AddMilliseconds((double)((milliSecondWaitTime * j) + 1000));
            job0.Submit();
            if (DateTime.Now < min_time)
            {
                throw new Exception($"wait job is to short... start: {now}, actual:{DateTime.Now} < {min_time}");
            }
            else if (DateTime.Now > max_time)
            {
                throw new Exception($"wait job is good... start: {now}, actual:{DateTime.Now} > {max_time}");
            }
            else
            {
                string message = "wait job is good...";
            }

            await job0.UpdateStatusAsync();
            await job0.TerminateAsync();
        }

        [Test]
        public void SendAsyncCheckVariablesWaitTimeShouldFailWhenNegative()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = 9;
            int nbrOfTry = nbrOfFail + 1;
            int milliSecondWaitTime = -100;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new ExponentialRetryHandler(nbrOfTry, milliSecondWaitTime);
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            Exception ex = Assert.Throws<System.ArgumentOutOfRangeException>(() => job0.Submit());
            Assert.IsNotNull(ex);
        }
    }

    public class UnitTestLinearRetryHandler
    {
        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "UnitTest are not multilangues.")]
        [Test]
        public async Task SendAsyncCheckTimeAfter9Fail()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfTry = 10;
            int nbrOfFail = 9;
            int milliSecondWaitTime = 10;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new LinearRetryHandler(nbrOfTry, milliSecondWaitTime);
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            DateTime now = DateTime.Now;
            DateTime min_time = now.AddMilliseconds((double)(milliSecondWaitTime * nbrOfFail));
            DateTime max_time = now.AddMilliseconds((double)((milliSecondWaitTime * nbrOfFail) + 1000));
            job0.Submit();
            if (DateTime.Now < min_time)
            {
                throw new Exception($"wait job is to short... start: {now}, actual:{DateTime.Now} < {min_time}");
            }
            else if (DateTime.Now > max_time)
            {
                throw new Exception($"wait job is to long... start: {now}, actual:{DateTime.Now} > {max_time}");
            }

            await job0.UpdateStatusAsync();
            await job0.TerminateAsync();
        }

        [Test]
        public async Task SendAsyncCheckBadNbrOfTryVariable()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = -10;
            int nbrOfTry = nbrOfFail + 1;
            int milliSecondWaitTime = 100;
            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new LinearRetryHandler(nbrOfTry, milliSecondWaitTime);
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);

            DateTime now = DateTime.Now;
            DateTime min_time = now.AddMilliseconds((double)(milliSecondWaitTime * nbrOfFail));
            DateTime max_time = now.AddMilliseconds((double)((milliSecondWaitTime * nbrOfFail) + 1000));

            await job0.SubmitAsync();
            await job0.TerminateAsync();
        }

        [Test]
        public void SendAsyncCheckBadWaitTimeVariableShouldFail()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = 9;
            int nbrOfTry = nbrOfFail + 1;
            int milliSecondWaitTime = -100;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new LinearRetryHandler(nbrOfTry, milliSecondWaitTime);
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            Exception ex = Assert.Throws<System.ArgumentOutOfRangeException>(() => job0.Submit());
            Assert.IsNotNull(ex);
        }
    }
}

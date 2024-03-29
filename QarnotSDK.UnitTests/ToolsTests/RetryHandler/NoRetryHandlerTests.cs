namespace QarnotSDK.UnitTests
{
    using System;
    using System.Linq;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class NoRetryHandlerTests
    {
        [Test]
        public async Task NoRetryHandlerSendAsyncNotFailPass()
        {
            string url = "https://localhost/";
            string token = "token";
            int nbrOfFail = 0;

            using var handler = new RetryFakeHTTPHandler(nbrOfFail);
            using var retryHandler = new NoRetryHandler();
            var connect = new Connection(url, token, handler, retryHandler);
            var job0 = connect.CreateJob("job", pool: null, UseTaskDependencies: false);
            await job0.SubmitAsync();
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
            Exception ex = Assert.ThrowsAsync<QarnotApiException>(async () => await job0.SubmitAsync());
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

            // We multiply by 2 because there are two requests performed in `SubmitAsync`.
            var expectedWaitTime = 2 * Enumerable
                .Range(0, nbrOfFail)
                .Select(i => milliSecondWaitTime * Math.Pow(2, i))
                .Sum();

            DateTime now = DateTime.Now;
            DateTime min_time = now.AddMilliseconds(expectedWaitTime);
            DateTime max_time = now.AddMilliseconds(expectedWaitTime + 1000);

            await job0.SubmitAsync();

            Assert.GreaterOrEqual(DateTime.Now.Truncate(), min_time.Truncate(), "wait time is too short");
            Assert.LessOrEqual(DateTime.Now.Truncate(), max_time.Truncate(), "wait time is too long");

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
            Exception ex = Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(async () => await job0.SubmitAsync());
            Assert.IsNotNull(ex);
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan = default)
        {
            timeSpan = timeSpan == default ? TimeSpan.FromMilliseconds(1): timeSpan; // By default keep milliseconds precision
            if (timeSpan == TimeSpan.Zero) return dateTime;
            if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
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
            await job0.SubmitAsync();

            Assert.GreaterOrEqual(DateTime.Now.Truncate(), min_time.Truncate(), "wait time is too short");
            Assert.LessOrEqual(DateTime.Now.Truncate(), max_time.Truncate(), "wait time is too long");

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
            Exception ex = Assert.ThrowsAsync<System.ArgumentOutOfRangeException>(async () => await job0.SubmitAsync());
            Assert.IsNotNull(ex);
        }
    }
}

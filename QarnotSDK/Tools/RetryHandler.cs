using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK {

    /// <summary>
    /// Interface for retry handler
    /// </summary>
    public abstract class IRetryHandler : DelegatingHandler
    {
        /// <summary>
        /// Default retry interval between 2 tries in milliseconds.
        /// </summary>
        public const int DefaultRetryInterval = 500;
        /// <summary>
        /// Default number of retry
        /// </summary>
        public const int DefaultMaxRetry = 3;
        /// <summary>
        /// Maximum retry number
        /// </summary>
        public virtual int MaxRetries { get; set; }
        /// <summary>
        /// Handler retry interval
        /// </summary>
        public virtual int RetryInterval { get; set; }

        /// <summary>
        /// List of transient status code that will result in a retry
        /// </summary>
        protected static readonly List<HttpStatusCode> TransientStatusCodes =
            new List<HttpStatusCode>
            {
                HttpStatusCode.RequestTimeout,      // 408
                HttpStatusCode.BadGateway,          // 502
                HttpStatusCode.ServiceUnavailable,  // 503
                HttpStatusCode.GatewayTimeout,      // 504
            };

        /// <summary>
        /// IRetryHandler constructor
        /// </summary>
        /// <param name="maxRetries">maximum number of retry</param>
        /// <param name="interval">handler retry interval</param>
        protected IRetryHandler(int maxRetries = DefaultMaxRetry, int interval = DefaultRetryInterval)
            : base()
        {
            MaxRetries = maxRetries;
            RetryInterval = interval;
        }

        /// <summary>
        /// IRetryHandler constructor
        /// </summary>
        /// <param name="innerHandler">http inner handler</param>
        /// <param name="maxRetries">maximum number of retry</param>
        /// <param name="interval">handler retry interval</param>
        protected internal IRetryHandler(HttpMessageHandler innerHandler, int maxRetries = DefaultMaxRetry,
            int interval = DefaultRetryInterval) : base(innerHandler)
        {
            MaxRetries = maxRetries;
            RetryInterval = interval;
        }

        /// <summary>
        /// Handle asynchrone request, defaulting to the base delegating.
        /// </summary>
        /// <param name="request">the Http request</param>
        /// <param name="cancellationToken">cancellationg token to cancel the async request</param>
        /// <returns>the HttpResponseMessage</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => await base.SendAsync(request, cancellationToken);

        /// <summary>
        /// Dispose the handler
        /// </summary>
        /// <param name="disposing">is disposing</param>
        protected override void Dispose(bool disposing) { base.Dispose(disposing); }
    }

    /// <summary>
    /// No retry handler, is an IRetryHandler implementation without retry
    /// </summary>
    public class NoRetryHandler : IRetryHandler
    {
        /// <summary>
        /// Maximum retry, 0 for NoRetryHandler
        /// </summary>
        public override int MaxRetries { get => 0; set { ; } }
        /// <summary>
        /// Interval between retries, 0 for NoRetryHandler
        /// </summary>
        public override int RetryInterval { get => 0; set { ; } }

        /// <summary>
        /// NoRetryHandler constructor
        /// </summary>
        public NoRetryHandler() : base() {}

        /// <summary>
        /// internal NoRetryHandler constructor
        /// </summary>
        /// <param name="innerHandler">the http handler</param>
        internal NoRetryHandler(HttpMessageHandler innerHandler) : base(innerHandler) {}
    }

    /// <summary>
    /// ExponentialRetryHandler is an IRetryHandler implementation, with a growing interval between retries
    /// </summary>
    public class ExponentialRetryHandler : IRetryHandler
    {
        /// <summary>
        /// ExponentialRetryHandler constructor
        /// </summary>
        /// <param name="maxRetries">maximum number of retry</param>
        /// <param name="interval">interval between retries in milliseconds</param>
        public ExponentialRetryHandler(int maxRetries=DefaultMaxRetry, int interval=DefaultRetryInterval)
            : base(maxRetries, interval) { }

        /// <summary>
        /// Internal ExponentialRetryHandler constructor
        /// </summary>
        /// <param name="innerHandler">The http handler</param>
        /// <param name="maxRetries">the maximum number of retry</param>
        /// <param name="interval">interval between retries in milliseconds</param>
        internal ExponentialRetryHandler(HttpMessageHandler innerHandler, int maxRetries=DefaultMaxRetry,
            int interval=DefaultRetryInterval) : base(innerHandler, maxRetries, interval) { }

        /// <summary>
        /// Handle asynchrone request with the retry policy
        /// </summary>
        /// <param name="request">the Http request</param>
        /// <param name="cancellationToken">cancellationg token to cancel the async request</param>
        /// <returns>the HttpResponseMessage</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            int tries = 0;
            while(true) {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode) // Success!
                    break;
                if (!TransientStatusCodes.Contains(response.StatusCode)) // Not a transient error
                    break;
                if (tries++ >= MaxRetries) // No more retry
                    break;
                // Transient error, wait & retry
                await Task.Delay(RetryInterval * tries);
            }
            return response;
        }
    }

    /// <summary>
    /// LinearRetryHandler is an IRetryHandler implementation, with a fixed interval between retries
    /// </summary>
    public class LinearRetryHandler : IRetryHandler
    {
        /// <summary>
        /// LinearRetryHandler constructor
        /// </summary>
        /// <param name="maxRetries">maximum number of retry</param>
        /// <param name="interval">interval between retries in milliseconds</param>
        public LinearRetryHandler(int maxRetries=DefaultMaxRetry, int interval=DefaultRetryInterval)
            : base(maxRetries, interval) { }

        internal LinearRetryHandler(HttpMessageHandler innerHandler, int maxRetries=DefaultMaxRetry,
            int interval=DefaultRetryInterval) : base(innerHandler, maxRetries, interval)
        { }

        /// <summary>
        /// Handle asynchrone request with the retry policy
        /// </summary>
        /// <param name="request">the Http request</param>
        /// <param name="cancellationToken">cancellationg token to cancel the async request</param>
        /// <returns>the HttpResponseMessage</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            int tries = 0;
            while(true) {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)// Success!
                    break;
                if (!TransientStatusCodes.Contains(response.StatusCode))// Not a transient error
                    break;
                if (tries++ >= MaxRetries)// No more retry
                    break;
                // Transient error, wait & retry
                await Task.Delay(RetryInterval);
            }
            return response;
        }
    }
}

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK {
    internal class RetryHandler : DelegatingHandler {
        public int MaxRetries { get; set; }

        private static readonly List<HttpStatusCode> TransientStatusCodes =
             new List<HttpStatusCode>
             {
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.BadGateway,
             };

        public RetryHandler(HttpMessageHandler innerHandler, int maxRetries)
            : base(innerHandler) {
            MaxRetries = maxRetries;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            HttpResponseMessage response;

            int tries = MaxRetries;
            int count = 0;
            while(true) {
                response = await base.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode) {
                    // Success!
                    break;
                }
                if (!TransientStatusCodes.Contains(response.StatusCode)) {
                    // Not a transient error
                    break;
                }

                if (--tries <= 0) {
                    // No more retry
                    break;
                }

                // Transient error, wait & retry
                await Task.Delay(500 * count++);
                if (count > 5) count = 5;
            }

            return response;
        }
    }
}

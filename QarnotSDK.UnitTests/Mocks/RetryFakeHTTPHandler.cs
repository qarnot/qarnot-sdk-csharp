namespace QarnotSDK.UnitTests
{
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class RetryFakeHTTPHandler : HttpClientHandler
    {
        private readonly int Init = 0;
        private int Counter = 0;
        internal string ReturnMessage = "{\"Your\":\"response\"}";

        public RetryFakeHTTPHandler(int c) : base() { Init = c; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Counter < Init)
            {
                Counter++;
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
            Counter = 0;
            var response = await Task.FromResult( new HttpResponseMessage(HttpStatusCode.Accepted)).ConfigureAwait(false);
            response.Content = new StringContent(ReturnMessage, Encoding.UTF8, "application/json");
            return response;
        }
    }
}

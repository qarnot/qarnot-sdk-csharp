namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using QarnotSDK;

    public class ParsedRequest
    {
        public string Method { get; set; }

        public string Uri { get; set; }

        public string Content { get; set; }

        public ParsedRequest() {}

        public override string ToString() => $"[{Method}-{Uri}] : {Content}";
    }

    public class InterceptingFakeHttpHandler : HttpClientHandler
    {
        private const string STORAGE_RESPONSE = @"{""storage"":""https://localhost/""}";
        private const string SUCCESS_RESPONSE = "{\"Your\":\"response\"}";
        public readonly List<ParsedRequest> ParsedRequests = new List<ParsedRequest>();

        public string ResponseBody { get; set; }

        public InterceptingFakeHttpHandler()
            : base()
        {
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters", Justification = "UnitTest are not multilangues.")]
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var parsedRequest = await ParseRequest(request);
            ParsedRequests.Add(parsedRequest);

            if (parsedRequest.Uri.Contains("setting", StringComparison.InvariantCultureIgnoreCase))
            {
                return await SendHttpAsync(HttpStatusCode.Accepted, STORAGE_RESPONSE);
            }
            return await SendHttpAsync(HttpStatusCode.Accepted, ResponseBody ?? SUCCESS_RESPONSE);
        }

        private async Task<HttpResponseMessage> SendHttpAsync(HttpStatusCode statusCode, string message) =>
            await Task.FromResult(
                new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(message, Encoding.UTF8, "application/json"),
                });

        private async Task<ParsedRequest> ParseRequest(HttpRequestMessage request)
        {
            var parsedRequest = new ParsedRequest()
            {
                Method = request.Method.ToString(),
                Uri = request.RequestUri.ToString(),
            };

            if (request.Content != null)
            {
                parsedRequest.Content = await request.Content?.ReadAsStringAsync();
            }

            return parsedRequest;
        }
    }
}

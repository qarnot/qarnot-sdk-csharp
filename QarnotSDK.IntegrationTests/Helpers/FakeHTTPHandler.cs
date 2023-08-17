namespace QarnotSDK.IntegrationTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeHTTPHandler : HttpClientHandler
    {
        private int ReturnMessageListIndex = 0;

        public FakeHTTPHandler()
            : base()
        {
        }

        public string ReturnMessage { get; set; } = "{\"Your\":\"response\"}";
        public List<string> ReturnMessageList { get; set; } = null;

        /// <summary>
        /// key: url call
        /// value response message
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> ReturnMessageDictionary { get; set; } = null;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)).ConfigureAwait(false);
            var message = "";
            var uriCall = request.RequestUri.ToString();
            if (ReturnMessageDictionary != null && ReturnMessageDictionary.ContainsKey(uriCall))
            {
                message = ReturnMessageDictionary[uriCall];
            }
            else if (ReturnMessageList != null)
            {
                message = ReturnMessageList[ReturnMessageListIndex % ReturnMessageList.Count];
                ReturnMessageListIndex += 1;
            }
            else
            {
                message = ReturnMessage;
            }

            response.Content = new StringContent(message, Encoding.UTF8, "application/json");
            return response;
        }
    }
}

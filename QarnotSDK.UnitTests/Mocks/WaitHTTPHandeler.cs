namespace QarnotSDK.UnitTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using QarnotSDK;

    public class WaitHTTPHandler : HttpClientHandler
    {
        private readonly int TimeToWait = 0;
        private bool Start = false;
        private DateTime Begin;
        private DateTime End;
        private string PartialMessage = "{\"state\": \"" + QTaskStates.PartiallyExecuting + "\"}";
        private string FinalMessage = "{\"state\": \"" + QTaskStates.Success + "\", \"name\":\"vanilla_task_name\"}";

        public WaitHTTPHandler(int ttw)
            : base()
        {
            TimeToWait = ttw;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Start == false)
            {
                Start = true;
                Begin = DateTime.Now;
                End = DateTime.Now.AddSeconds(TimeToWait);
            }

            var response = await Task.FromResult( new HttpResponseMessage(HttpStatusCode.Accepted)).ConfigureAwait(false);
            if (End > DateTime.Now)
            {
                response.Content = new StringContent(PartialMessage, Encoding.UTF8, "application/json");
            }
            else
            {
                response.Content = new StringContent(FinalMessage, Encoding.UTF8, "application/json");
            }

            return response;
        }
    }
}

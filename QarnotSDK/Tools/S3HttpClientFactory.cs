namespace QarnotSDK
{
    using System.Net;
    using System.Net.Http;
    using Amazon.Runtime;

    /// <summary>
    /// An Http client Factory example to use or surcharge
    /// if a custom S3 connection handler
    /// or a custom http client is needed.
    /// </summary>
    public class S3HttpClientFactory : Amazon.Runtime.HttpClientFactory
    {

        /// <summary>
        /// S3HttpClientFactory Constructor.
        /// </summary>
        public S3HttpClientFactory()
        {}

        /// <summary>
        /// The CreateHttpClient function used by S3 to retrieve a new HttpClient every request.
        /// </summary>
        /// <param name="clientConfig">The client configuration to parse.</param>
        /// <returns>The HttpClient used by S3.</returns>
        public override HttpClient CreateHttpClient(IClientConfig clientConfig)
        {
            var httpMessageHandler = CreateClientHandler();

            if (clientConfig.MaxConnectionsPerServer.HasValue)
                httpMessageHandler.MaxConnectionsPerServer = clientConfig.MaxConnectionsPerServer.Value;
            httpMessageHandler.AllowAutoRedirect = clientConfig.AllowAutoRedirect;

            // Disable automatic decompression when Content-Encoding header is present
            httpMessageHandler.AutomaticDecompression = DecompressionMethods.None;

            var proxy = clientConfig.GetWebProxy();
            if (proxy != null)
            {
                httpMessageHandler.Proxy = proxy;
            }

            if (httpMessageHandler.Proxy != null && clientConfig.ProxyCredentials != null)
            {
                httpMessageHandler.Proxy.Credentials = clientConfig.ProxyCredentials;
            }
            var httpClient = new HttpClient(httpMessageHandler);

            if (clientConfig.Timeout.HasValue)
            {
                // Timeout value is set to ClientConfig.MaxTimeout for S3 and Glacier.
                // Use default value (100 seconds) for other services.
                httpClient.Timeout = clientConfig.Timeout.Value;
            }

            return httpClient;
        }

        /// <summary>
        /// The HttpClientHandler Factory function.
        /// This function can be override to define a new client handler.
        /// </summary>
        /// <returns>The Http client handler used to create the HttpClient.</returns>
        protected virtual HttpClientHandler CreateClientHandler() =>
            new HttpClientHandler();
    }
}

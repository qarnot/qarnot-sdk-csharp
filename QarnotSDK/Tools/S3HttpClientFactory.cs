namespace QarnotSDK
{
    using System;
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

#if (NET45)
        [Obsolete("This function is not implemented in the .NET framework 4.5")]
#endif
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

#if (!NET45)
            if (clientConfig.MaxConnectionsPerServer.HasValue)
                httpMessageHandler.MaxConnectionsPerServer = clientConfig.MaxConnectionsPerServer.Value;
#endif
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

#if (NET45)
namespace Amazon.Runtime
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// A factory class to be compliant with the net45 framework.
    /// </summary>
    public abstract class HttpClientFactory
    {
        /// <summary>
        /// HttpClientFactory constructor.
        /// Throw an exception if called.
        /// </summary>
        [Obsolete("This function is not implemented in the .NET framework 4.5")]
        public HttpClientFactory()
        {
            throw new NotImplementedException("This function is not implemented in the .NET framework 4.5");
        }
        /// <summary>
        /// Create and configure an HttpClient.
        /// </summary>
        /// <returns>Http client</returns>
        public abstract HttpClient CreateHttpClient(IClientConfig clientConfig);
    }
}
#endif

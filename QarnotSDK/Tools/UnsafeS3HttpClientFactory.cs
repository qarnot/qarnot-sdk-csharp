namespace QarnotSDK
{
    using System.Net.Http;

    /// <summary>
    /// Crete a S3 Http Factory that will not check the SSL certificate.
    /// </summary>
    public class UnsafeS3HttpClientFactory : S3HttpClientFactory
    {
        /// <summary>
        /// UnsafeS3HttpClientFactory Constructor.
        /// </summary>
        public UnsafeS3HttpClientFactory()
        {}

        /// <summary>
        /// Create an UnsafeClientHandler
        /// </summary>
        /// <returns>A Http Client Handler with no SSL certificate check.</returns>
        protected override HttpClientHandler CreateClientHandler() =>
            new UnsafeClientHandler();
    }
}

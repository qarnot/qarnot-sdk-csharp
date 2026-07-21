namespace QarnotSDK
{
    using System.Net.Http;

    /// <summary>
    /// unsafe handler, it is an HttpClientHandler implementation without ssl certification verification
    /// It is not recommended to use it outside tests an debbugs implementations
    /// </summary>
    public class UnsafeClientHandler : HttpClientHandler
    {
        /// <summary>
        /// public UnsafeClientHandler constructor
        /// </summary>
        public UnsafeClientHandler() : base()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
    }
}

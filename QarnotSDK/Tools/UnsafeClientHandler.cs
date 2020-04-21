namespace QarnotSDK
{
    using System.Net.Http;

    /// <summary>
    /// unsafe handler, it is an HttpClientHandler implementation without ssl certification verification
    /// It is not recommended to use it outside tests an debbugs implementations
    /// </summary>
#if (NET45)
    // NETFRAMEWORK compatibility, remove this condition if the project pass to NETFRAMEWORK 4.7.1
    public class UnsafeClientHandler : WebRequestHandler
    {
        /// <summary>
        /// public UnsafeClientHandler constructor
        /// </summary>
        public UnsafeClientHandler() : base()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual;
            ServerCertificateValidationCallback = delegate { return true; };
        }
    }
#elif (NETSTANDARD2_0)
    // NETSTANDARD compatibility, remove this condition if the project pass to NETSTANDARD2_1
    public class UnsafeClientHandler : HttpClientHandler
    {
        /// <summary>
        /// public UnsafeClientHandler constructor
        /// </summary>
        public UnsafeClientHandler() : base()
        {
            ServerCertificateCustomValidationCallback = delegate { return true; };
        }
    }
#else
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
#endif
}

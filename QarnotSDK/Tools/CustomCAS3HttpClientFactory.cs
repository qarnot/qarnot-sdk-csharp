namespace QarnotSDK
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Crete a S3 Http Client Handler Factory that can accept custom SSL certificate given.
    /// </summary>
    public class CustomCAS3HttpClientFactory : S3HttpClientFactory
    {
        private IEnumerable<X509Certificate2> certificateList;
        private string[] certificatePathList;

        /// <summary>
        /// Create a storage httpClient custom certificate check factory from certificate objects
        /// </summary>
        /// <param name="localCertificates">X509 certificates objects</param>
#if (NET45)
        [Obsolete("This function is not implemented in the .NET framework 4.5")]
#endif
        public CustomCAS3HttpClientFactory(IEnumerable<X509Certificate2> localCertificates)
        {
            certificateList = localCertificates;
            certificatePathList = null;
        }

        /// <summary>
        /// Create a storage httpClient custom certificate check factory from certificate path
        /// </summary>
        /// <param name="localCertPaths">Certificates paths.</param>
#if (NET45)
        [Obsolete("This function is not implemented in the .NET framework 4.5")]
#endif
        public CustomCAS3HttpClientFactory(params string[] localCertPaths)
        {
            certificateList = null;
            certificatePathList = localCertPaths;
        }

        /// <summary>
        /// Create an CustomCAClientHandler
        /// </summary>
        /// <returns>A Http Client Handler that can accept the custom SSL certificate given.</returns>
        protected override HttpClientHandler CreateClientHandler() =>
            certificateList != null ? new CustomCAClientHandler(certificateList) : new CustomCAClientHandler(certificatePathList);
    }
}

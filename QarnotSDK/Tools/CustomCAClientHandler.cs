namespace QarnotSDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;


    /// <summary>
    /// public CustomCertificateAutorisationClientHandler compatibility constructor
    /// </summary>
#if (NET45)
    // NETFRAMEWORK compatibility, remove this condition if the project pass to NETFRAMEWORK 4.7.1
    public abstract class ACustomCAClientHandler : WebRequestHandler
    {
        /// <summary>
        /// Add the ServerCertificateValidationCallback function
        /// </summary>
        protected void AddValidityFunction()
        {
            // NETFRAMEWORK compatibility, remove this condition if the project pass to NETFRAMEWORK 4.7.1
            ClientCertificateOptions = ClientCertificateOption.Manual;
            ServerCertificateValidationCallback = (sender, apiCertificate, chain, sslPolicyErrors) =>
            {
                return ValidateCertificateChain(new X509Certificate2(apiCertificate), chain);
            };
        }

        /// <summary>
        /// Validate certificate function
        /// </summary>
        /// <param name="apiCertificate">The final certificate</param>
        /// <param name="chain">The certificate chain</param>
        /// <returns>the chain is valid</returns>
        public abstract bool ValidateCertificateChain(X509Certificate2 apiCertificate, X509Chain chain);

    }
#else
    public abstract class ACustomCAClientHandler : HttpClientHandler
    {
        /// <summary>
        /// Add the ServerCertificateCustomValidationCallback function
        /// </summary>
        protected void AddValidityFunction()
        {
            // NETFRAMEWORK compatibility, remove this condition if the project pass to NETFRAMEWORK 4.7.1
            ClientCertificateOptions = ClientCertificateOption.Manual;
            ServerCertificateCustomValidationCallback = (sender, apiCertificate, chain, sslPolicyErrors) =>
            {
                return ValidateCertificateChain(apiCertificate, chain);
            };
        }

        /// <summary>
        /// Validate certificate function
        /// </summary>
        /// <param name="apiCertificate">The final certificate</param>
        /// <param name="chain">The certificate chain</param>
        /// <returns>the chain is valid</returns>
        public abstract bool ValidateCertificateChain(X509Certificate2 apiCertificate, X509Chain chain);

    }
#endif

    /// <summary>
    /// public CustomCertificateAutorisationClientHandler constructor
    /// </summary>
    public class CustomCAClientHandler : ACustomCAClientHandler
    {
        private readonly IEnumerable<X509Certificate2> LocalCertificates;
        private bool TestCertificate(X509Certificate2 apiCertificate, X509Certificate2 localCertificate)
        {
            // check the certificate subject
            // check the certificate public key
            // check the validity start and end periods
            var ret = apiCertificate.Subject == localCertificate.Subject &&
                apiCertificate.GetPublicKeyString() == localCertificate.GetPublicKeyString() &&
                apiCertificate.SerialNumber == localCertificate.SerialNumber &&
                apiCertificate.RawData.Length == localCertificate.RawData.Length &&
                apiCertificate.NotBefore < DateTime.Now &&
                apiCertificate.NotAfter > DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Validate certificate function
        /// </summary>
        /// <param name="apiCertificate">The final certificate</param>
        /// <param name="chain">The certificate chain</param>
        /// <returns>the chain is valid</returns>
        public override bool ValidateCertificateChain(X509Certificate2 apiCertificate, X509Chain chain)
        {
            if (chain == null || apiCertificate == null || !LocalCertificates.Any(cert => TestCertificate(new X509Certificate2(apiCertificate), cert)))
            {
                return false;
            }

            foreach (var element in chain.ChainElements)
            {
                foreach (var status in element.ChainElementStatus)
                {
                    if (status.Status == X509ChainStatusFlags.UntrustedRoot)
                    {
                        if (LocalCertificates.Any(cert => TestCertificate(element.Certificate, cert)))
                        {
                            continue;
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// public CustomCertificateAutorisationClientHandler constructor.
        /// </summary>
        /// <param name="localCertificates">List of the the X509Certificate2 certificates.</param>
        public CustomCAClientHandler(IEnumerable<X509Certificate2> localCertificates) : base()
        {
            LocalCertificates = localCertificates;
            AddValidityFunction();
        }

        /// <summary>
        /// public CustomCertificateAutorisationClientHandler constructor.
        /// </summary>
        /// <param name="localCertPaths">List of the custom certificate paths.</param>
        public CustomCAClientHandler(params string[] localCertPaths) : this(localCertPaths.Select(localCertPath => new X509Certificate2(localCertPath)))
        {
        }
    }
}

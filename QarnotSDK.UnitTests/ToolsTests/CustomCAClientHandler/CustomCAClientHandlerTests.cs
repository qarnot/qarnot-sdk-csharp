namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class CustomCAClientHandlerTester
    {
        private static X500DistinguishedName subjectName = new X500DistinguishedName("CN=mydomain.com, O=\"Company, Inc.\", S=QC, C=CA");

        private static ECDsa key = ECDsa.Create();

        private static HashAlgorithmName hashAlgorithmName = new HashAlgorithmName("SHA256");

        private static CertificateRequest certificateRequest = new CertificateRequest(subjectName, key, hashAlgorithmName);

        [Test]
        public void CustomCAClientHandlerAccepteDoubleCertificate()
        {
            using var certificateToCheck1 = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(150));
            using var certificateToCheck2 = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(150));

            var customCertificateList = new List<X509Certificate2> { certificateToCheck1, certificateToCheck2 };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck1);
            x509Chain.Build(certificateToCheck2);

            Assert.IsTrue(handler.ServerCertificateCustomValidationCallback(null, certificateToCheck1, x509Chain, SslPolicyErrors.None));
        }

        [Test]
        public void CustomCAClientHandlerRejectZeroTestCertificate()
        {
            using var certificateToCheck = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(150));

            var customCertificateList = new List<X509Certificate2> { };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck);
            using var cert = new X509Certificate2();

            Assert.IsFalse(handler.ServerCertificateCustomValidationCallback(null, cert, x509Chain, SslPolicyErrors.None));
        }

        [Test]
        public void CustomCAClientHandlerRejectOneInvalidCertificateInTheChainOfCertificates()
        {
            using var certificateToCheck1 = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(50));
            using var certificateToCheck2 = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(50));
            using var certificateToCheck3 = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(50));

            var customCertificateList = new List<X509Certificate2> { certificateToCheck1, certificateToCheck2 };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck1);
            x509Chain.Build(certificateToCheck2);
            x509Chain.Build(certificateToCheck3);

            Assert.IsFalse(handler.ServerCertificateCustomValidationCallback(null, certificateToCheck1, x509Chain, SslPolicyErrors.None));
        }

        [Test]
        public void CustomCAClientHandlerRejectWrongCertificate()
        {
            using var certificateToCheck = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(50));
            using var customDifferentCertificate = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(50));

            var customCertificateList = new List<X509Certificate2> { customDifferentCertificate };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck);

            Assert.IsFalse(handler.ServerCertificateCustomValidationCallback(null, certificateToCheck, x509Chain, SslPolicyErrors.None));
        }

        [Test]
        public void CustomCAClientHandlerRejectBeforeBeginPreiodCertificate()
        {
            using var certificateToCheck = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(100), DateTime.Now.AddHours(150));

            var customCertificateList = new List<X509Certificate2> { certificateToCheck };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck);

            Assert.IsFalse(handler.ServerCertificateCustomValidationCallback(null, certificateToCheck, x509Chain, SslPolicyErrors.None));
        }

        [Test]
        public void CustomCAClientHandlerRejectExpiredCertificate()
        {
            using var certificateToCheck = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(-50));

            var customCertificateList = new List<X509Certificate2> { certificateToCheck };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck);

            Assert.IsFalse(handler.ServerCertificateCustomValidationCallback(null, certificateToCheck, x509Chain, SslPolicyErrors.None));
        }

        [Test]
        public void CustomCAClientHandlerAccepteSingleCertificate()
        {
            using var certificateToCheck = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-100), DateTime.Now.AddHours(50));

            var customCertificateList = new List<X509Certificate2> { certificateToCheck };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(certificateToCheck);

            Assert.IsTrue(handler.ServerCertificateCustomValidationCallback(null, certificateToCheck, x509Chain, SslPolicyErrors.None));
        }

        private X509Certificate2 getOneValidCertificate()
        {
            foreach (StoreLocation storeLocation in (StoreLocation[]) Enum.GetValues(typeof(StoreLocation)))
            {
                using var store = new X509Store(StoreName.Root, storeLocation);
                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    if (store.Certificates.Count > 0)
                    {
                        return store.Certificates[0];
                    }
                }
                catch (CryptographicException ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }
            }

            return null;
        }

        [Test]
        public void CustomCAClientHandlerAccepteValidRootCertificate()
        {
            using var validCertificate = getOneValidCertificate();

            if (validCertificate == null)
            {
                return ;
            }

            var customCertificateList = new List<X509Certificate2> { validCertificate };
            using var handler = new CustomCAClientHandler(customCertificateList);

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(validCertificate);

            Assert.IsTrue(handler.ServerCertificateCustomValidationCallback(null, validCertificate, x509Chain, SslPolicyErrors.None));
        }
    }
}

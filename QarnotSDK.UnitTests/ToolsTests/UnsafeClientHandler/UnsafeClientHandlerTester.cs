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
    public class UnsafeClientHandlerTester
    {
        private static X500DistinguishedName subjectName = new X500DistinguishedName("CN=mydomain.com, O=\"Company, Inc.\", S=QC, C=CA");

        private static ECDsa key = ECDsa.Create();

        private static HashAlgorithmName hashAlgorithmName = new HashAlgorithmName("SHA256");

        private static CertificateRequest certificateRequest = new CertificateRequest(subjectName, key, hashAlgorithmName);

        [Test]
        public void UnsafeClientHandlerWithNoValueReturnTrue()
        {
            using var handler = new UnsafeClientHandler();

            Assert.IsTrue(handler.ServerCertificateCustomValidationCallback(null, null, null, SslPolicyErrors.None));
        }

        [Test]
        public void UnsafeClientHandlerWithExpiredCertificateReturnTrue()
        {
            using var invalidCertificate = certificateRequest.CreateSelfSigned(DateTime.Now.AddHours(-1000), DateTime.Now.AddHours(-150));

            using var handler = new UnsafeClientHandler();

            using var x509Chain = new X509Chain();
            x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

            x509Chain.Build(invalidCertificate);

            Assert.IsTrue(handler.ServerCertificateCustomValidationCallback(null, invalidCertificate, x509Chain, SslPolicyErrors.None));
        }
    }
}

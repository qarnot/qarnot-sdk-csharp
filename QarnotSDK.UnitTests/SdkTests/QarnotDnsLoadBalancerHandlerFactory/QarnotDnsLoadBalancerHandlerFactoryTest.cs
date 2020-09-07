namespace QarnotSDK.UnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;
    using Moq;

    [TestFixture]
    public class QarnotDnsLoadBalancerHandlerFactoryTest
    {
        [Test]
        public async Task CheckTheEnvVariableReturnValues()
        {
            Mock<IEnvironmentWrapper> envWrapper = new Mock<IEnvironmentWrapper>();
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME")).Returns("5");
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns("true");
            Uri uri = new Uri("https://api.qrnot.com:80/test");
            QarnotDnsLoadBalancerHandlerFactory dnsHandler = new QarnotDnsLoadBalancerHandlerFactory(uri);
            dnsHandler.EnvironmentWrapper = envWrapper.Object;

            Assert.AreEqual("5", dnsHandler.DNS_LOAD_BALANCING_CACHE_TIME);
            Assert.AreEqual("true", dnsHandler.ENABLE_DNS_LOAD_BALANCING);
        }

        [Test]
        public async Task DnsBalancingMessageHandlerReturnNull()
        {
            Mock<IEnvironmentWrapper> envWrapper = new Mock<IEnvironmentWrapper>();
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME")).Returns("5");
            Uri uri = new Uri("https://api.qrnot.com:80/test");
            QarnotDnsLoadBalancerHandlerFactory dnsHandler = new QarnotDnsLoadBalancerHandlerFactory(uri);
            dnsHandler.EnvironmentWrapper = envWrapper.Object;

            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns("false");
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns("");
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns((string)null);
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns("test");
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns("true");
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME")).Returns("null");
            dnsHandler = new QarnotDnsLoadBalancerHandlerFactory(uri);
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME")).Returns("test");
            dnsHandler = new QarnotDnsLoadBalancerHandlerFactory(uri);
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME")).Returns("-5");
            dnsHandler = new QarnotDnsLoadBalancerHandlerFactory(uri);
            Assert.AreEqual(null, dnsHandler.DnsBalancingMessageHandler);
        }

        [Test]
        public async Task DnsBalancingMessageHandlerReturnBalanceHandler()
        {
            Mock<IEnvironmentWrapper> envWrapper = new Mock<IEnvironmentWrapper>();
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME")).Returns("5");
            envWrapper.Setup(foo => foo.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING")).Returns("true");
            Uri uri = new Uri("https://api.qrnot.com:80/test");
            QarnotDnsLoadBalancerHandlerFactory dnsHandler = new QarnotDnsLoadBalancerHandlerFactory(uri);
            dnsHandler.EnvironmentWrapper = envWrapper.Object;

            Assert.IsTrue(dnsHandler.DnsBalancingMessageHandler != null);
        }
    }
}

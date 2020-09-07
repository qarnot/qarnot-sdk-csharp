namespace QarnotSDK
{
    using System;
    using System.Net.Http;
    using System.Net.Sockets;
    using DnsClient;
    using DnsSrvTool;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// QarnotDnsLoadBalancerHandlerFactory class
    /// Factory of the DnsServiceBalancingMessageHandler.
    /// </summary>
    public class QarnotDnsLoadBalancerHandlerFactory
    {
        /// <summary>
        /// The QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME env variable.
        /// If it is set and represent a positive number,
        /// it represent the Dns load balancer cache time
        /// "null" retpresent a null object and a number represent the cache time.
        /// </summary>
        /// <returns>DNS_LOAD_BALANCING_CACHE_TIME env variable string</returns>
        public string DNS_LOAD_BALANCING_CACHE_TIME => EnvironmentWrapper.GetEnvironmentVariable("QARNOT_SDK_CSHARP_DNS_LOAD_BALANCING_CACHE_TIME");

        /// <summary>
        /// The QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING env variable.
        /// "true" enable the DNS load balancer.
        /// </summary>
        /// <returns>ENABLE_DNS_LOAD_BALANCING env variable string</returns>
        public string ENABLE_DNS_LOAD_BALANCING => EnvironmentWrapper.GetEnvironmentVariable("QARNOT_SDK_CSHARP_ENABLE_DNS_LOAD_BALANCING");

        private Uri Uri;
        private uint? CacheTime;
        private ILogger Logger;
        internal IEnvironmentWrapper EnvironmentWrapper;

        /// <summary>
        /// Builder of the Qarnot DnsSrvHandler.
        /// </summary>
        /// <value>A new DnsSrvHandler.</value>
        public DelegatingHandler DnsBalancingMessageHandler {
            get
            {
                if (ShouldCreateDnsLoadBalancer())
                {
                    return CreateDnsServiceBalancingMessageHandler();
                }

                return null;
            }
        }

        private QarnotDnsLoadBalancerHandlerFactory()
        {
            EnvironmentWrapper = new EnvironmentWrapper();
        }

        /// <summary>
        /// QarnotDnsLoadBalancerHandlerFactory constructor.
        /// </summary>
        /// <param name="uri">Uri to be call.</param>
        /// <param name="cacheTime">Quarantine cache time.</param>
        /// <param name="logger">Logger if needed.</param>
        public QarnotDnsLoadBalancerHandlerFactory(Uri uri, uint? cacheTime = 5, ILogger logger=null) : this()
        {
            Uri = uri;
            CacheTime = GetDnsLoadBalancingCacheTimeEnvironementVariable(cacheTime);
            Logger = logger;
        }

        private uint? GetDnsLoadBalancingCacheTimeEnvironementVariable(uint? dnsSrvLoadBalancingCacheTime)
        {
            string loadBalancerCacheTime = DNS_LOAD_BALANCING_CACHE_TIME;
            if (loadBalancerCacheTime == null)
            {
                return dnsSrvLoadBalancingCacheTime;
            }
            else if (loadBalancerCacheTime == "null")
            {
                return null;
            }

            try
            {
                uint cacheTime = uint.Parse(loadBalancerCacheTime);
                if (dnsSrvLoadBalancingCacheTime > 0)
                {
                    dnsSrvLoadBalancingCacheTime = cacheTime;
                }
            }
            catch(FormatException) {}

            return dnsSrvLoadBalancingCacheTime;
        }

        private bool ShouldCreateDnsLoadBalancer()
        {
            string createTheLoadBalancer = ENABLE_DNS_LOAD_BALANCING;
            return CacheTime != null && createTheLoadBalancer != null && createTheLoadBalancer.ToLower() == "true";
        }

        private DelegatingHandler CreateDnsServiceBalancingMessageHandler()
        {
            IDnsServiceExtractor extractor = new DnsServiceExtractorFirstLabelConvention(ProtocolType.Tcp);

            IDnsSrvQuerier querier = new DnsSrvQuerier(new LookupClient());
            DnsSrvServiceDescription service = extractor.FromUri(Uri);

            IDnsServiceTargetSelector selector = new DnsServiceTargetSelectorReal(querier, new DnsSrvSortResult(), CacheTime.Value);
            ITargetQuarantinePolicy quarantinePolice = new TargetQuarantinePolicyServerUnavailable();

            return new DnsServiceBalancingMessageHandler(service, selector, quarantinePolice, Logger);
        }
    }
}

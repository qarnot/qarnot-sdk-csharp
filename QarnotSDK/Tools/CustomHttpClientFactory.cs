using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Reflection;

namespace QarnotSDK
{
    internal class CustomHttpClientFactory
    {
        public HttpClientHandler HttpClientHandler { get; set; }
        public IRetryHandler RetryHandler { get; set; }
        public uint? DnsSrvLoadBalancingCacheTime { get; set; }
        public List<DelegatingHandler> DelegatingHandlers { get; set; }
        public string Token { get; set; }
        public Uri BaseAddress { get; set; }
        public string UserAgent { get; set; }

        public CustomHttpClientFactory(string token, Uri baseAddress)
        {
            HttpClientHandler = null;
            RetryHandler = null;
            DnsSrvLoadBalancingCacheTime = null;
            DelegatingHandlers = null;

            Token = token;
            BaseAddress = baseAddress;
            UserAgent = $"qarnot-sdk-csharp/{Assembly.GetExecutingAssembly().GetName().Version}";
        }

        public CustomHttpClientFactory WithHttpClientHandler(HttpClientHandler httpClientHandler)
        {
            HttpClientHandler = httpClientHandler;
            return this;
        }

        public CustomHttpClientFactory WithRetryHandler(IRetryHandler retryHandler)
        {
            RetryHandler = retryHandler;
            return this;
        }

        public CustomHttpClientFactory WithLoadBalancingCacheTime(uint? loadBalancingCacheTime)
        {
            DnsSrvLoadBalancingCacheTime = loadBalancingCacheTime;
            return this;
        }

        public CustomHttpClientFactory WithDelegatingHandlers(List<DelegatingHandler> delegatingHandlers)
        {
            DelegatingHandlers = delegatingHandlers;
            return this;
        }

        public CustomHttpClientFactory WithToken(string token)
        {
            Token = token;
            return this;
        }

        public CustomHttpClientFactory WithBaseAddress(Uri uri)
        {
            BaseAddress = uri;
            return this;
        }

        public CustomHttpClientFactory WithUserAgent(string userAgent)
        {
            UserAgent = userAgent;
            return this;
        }

        public CustomHttpClientFactoryResult Build()
        {
            var httpClientHandler = HttpClientHandler ?? new HttpClientHandler();
            var retryHandler = RetryHandler ?? new ExponentialRetryHandler();

            var delegatingHandlers = DelegatingHandlers ?? new List<DelegatingHandler>();
            AddDnsLoadBalancerToTheDelegateHandlers(DnsSrvLoadBalancingCacheTime, delegatingHandlers);
            delegatingHandlers.Add(retryHandler);

            var httpClient = new HttpClient(Utils.LinkHandlers(delegatingHandlers, httpClientHandler));
            httpClient.BaseAddress = BaseAddress;
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token);
            httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);

            return new CustomHttpClientFactoryResult(
                httpClient,
                httpClientHandler,
                retryHandler
            );
        }

        private void AddDnsLoadBalancerToTheDelegateHandlers(uint? dnsSrvLoadBalancingCacheTime, List<DelegatingHandler> delegatingHandlers)
        {
            var qarnotDnsLoadBalancerHandlerFactory = new QarnotDnsLoadBalancerHandlerFactory(BaseAddress, dnsSrvLoadBalancingCacheTime);
            var qarnotDnsLoadBalancerHandler = qarnotDnsLoadBalancerHandlerFactory.DnsBalancingMessageHandler;
            if (qarnotDnsLoadBalancerHandler != null)
            {
                delegatingHandlers.Add(qarnotDnsLoadBalancerHandler);
            }
        }
    }

    internal class CustomHttpClientFactoryResult
    {
        public HttpClient HttpClient { get; }
        public HttpClientHandler HttpClientHandler { get; }
        public IRetryHandler RetryHandler { get; }

        public CustomHttpClientFactoryResult(
            HttpClient httpClient,
            HttpClientHandler httpClientHandler,
            IRetryHandler retryHandler
        )
        {
            HttpClient = httpClient;
            HttpClientHandler = httpClientHandler;
            RetryHandler = retryHandler;
        }
    }
}

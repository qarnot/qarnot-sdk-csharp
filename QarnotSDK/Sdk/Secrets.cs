using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace QarnotSDK
{
    /// <summary>
    /// Client used to interact with the Qarnot secrets API.
    /// </summary>
    public class Secrets
    {
        /// <summary>The URI targeted by the client</summary>
        public Uri Uri { get; }
        /// <summary>The token used for authentication against the API</summary>
        public string Token { get; }

        private HttpClient _client { get; }
        private static string _dataPrefix = "secrets-manager/data";
        private static string _searchPrefix = "secrets-maanger/search";

        /// <summary>Instantiate a new Qarnot secrets API client</summary>
        /// <param name="token">Token used for authentication, available at https://account.qarnot.com</param>
        /// <param name="uri">Change default API location, defaults to https://secrets.qarnot.com</param>
        /// <param name="httpClientHandler">An optional HttpClientHandler for the api, if you need to setup a proxy for example.</param>
        /// <param name="retryHandler">An optional IRetryHandler if you need to setup retry for transient error (default to exponential).</param>
        /// <param name="delegatingHandlers">A list of hander used by the api connection. Default will create a list with the QarnotSrvHandler.</param>
        /// <param name="dnsSrvLoadBalancingCacheTime">the cache time in minutes before retrieving the values of the QarnotSrvHandler. Unless you have a strong reason, you should keep the default value</param>
        public Secrets(
                string token,
                string uri = null,
                HttpClientHandler httpClientHandler = null,
                IRetryHandler retryHandler = null,
                List<DelegatingHandler> delegatingHandlers = null,
                uint? dnsSrvLoadBalancingCacheTime = 5
            )
        {
            Token = token;
            Uri = new Uri(uri ?? "https://secrets.qarnot.com");

            _client = new CustomHttpClientFactory(Token, Uri)
                .WithHttpClientHandler(httpClientHandler)
                .WithRetryHandler(retryHandler)
                .WithDelegatingHandlers(delegatingHandlers)
                .WithLoadBalancingCacheTime(dnsSrvLoadBalancingCacheTime)
                .Build()
                .HttpClient;
        }

        /// <summary>Retrieve the value of the secret with the key <paramref name="key" /></summary>
        public async Task<string> GetAsync(string key, CancellationToken ct = default)
        {
            key = key.Trim('/');
            using (var resp = await _client.GetAsync($"{_dataPrefix}/{key}", ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var parsed = await resp.Content.ReadAsAsync<GetSecretApiResponse>(ct);
                return parsed.Value;
            }
        }

        /// <summary>Create a new secret at <paramref name="key" /> with a value of <paramref name="value" /></summary>
        public async Task<string> CreateAsync(string key, string value, CancellationToken ct = default)
        {
            key = key.Trim('/');
            var req = new CreateSecretApiRequest { Value = value };
            using (var resp = await _client.PutAsJsonAsync($"{_dataPrefix}/{key}", req, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                return key;
            }
        }

        /// <summary>Create a new secret at <paramref name="key" /> with a value of <paramref name="value" /></summary>
        public async Task<string> CreateAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var serialized = JsonConvert.SerializeObject(value);
            return await CreateAsync(key, serialized, ct);
        }

        /// <summary>Update the secret at <paramref name="key" /> with a new value of <paramref name="value" /></summary>
        public async Task UpdateAsync(string key, string value, CancellationToken ct = default)
        {
            key = key.Trim('/');

            var payload = new UpdateSecretApiRequest { Value = value };
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_dataPrefix}/{key}");
            request.Content = new StringContent(JsonConvert.SerializeObject(payload));

            using (var resp = await _client.SendAsync(request, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
            }
        }

        /// <summary>Update the secret at <paramref name="key" /> with a new value of <paramref name="value" /></summary>
        public async Task UpdateAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var serialized = JsonConvert.SerializeObject(value);
            await UpdateAsync(key, value, ct);
        }

        /// <summary>Delete the secret at <paramref name="key" /></summary>
        public async void DeleteAsync(string key, CancellationToken ct = default)
        {
            key = key.Trim('/');

            using (var resp = await _client.DeleteAsync($"{_dataPrefix}/{key}", ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
            }
        }

        /// <summary>List all the secrets starting with <paramref name="prefix" /></summary>
        /// <remarks>
        /// When not using recursive mode, only keys and folders directly under <paramref name="prefix" /> are
        /// returned. For example, listing with a prefix of "prefix" will return "prefix/a" but won't return
        /// "prefix/a/b". Folders can be identified by a trailing "/", for example "prefix/nested/". <br />
        /// When in recursive mode, only the secrets are returned, not the folders.
        /// </remarks>
        /// <param name="prefix"></param>
        /// <param name="recursive">Perform a recursive listing</param>
        /// <param name="ct">Optional cancellation token</param>
        public async Task<IEnumerable<string>> ListAsync(string prefix = "", bool recursive = false, CancellationToken ct = default)
        {
            if (!recursive)
            {
                return await ListOnceAsync(prefix, ct);
            }

            var results = new List<string>();
            var pending = new List<string> { prefix };

            while (pending.Any())
            {
                var key = pending[pending.Count - 1];
                pending.RemoveAt(pending.Count - 1);

                var keys = await ListOnceAsync(key, ct);
                results.AddRange(keys.Where(k => !k.EndsWith("/")));
                pending.AddRange(keys.Where(k => k.EndsWith("/")));
            }

            return results;
        }

        private async Task<IEnumerable<string>> ListOnceAsync(string prefix, CancellationToken ct)
        {
            prefix = prefix.Trim('/');

            using (var resp = await _client.GetAsync($"{_searchPrefix}/{prefix}", ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var parsed = await resp.Content.ReadAsAsync<ListSecretsApiResponse>(ct);
                return parsed.Keys.Select(k => string.IsNullOrWhiteSpace(prefix) ? k : $"{prefix}/{k}");
            }
        }
    }
}

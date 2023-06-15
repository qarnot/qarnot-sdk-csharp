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
        private HttpClient _client { get; }
        private static string _dataPrefix = "secrets-manager/data";
        private static string _searchPrefix = "secrets-manager/search";

        internal Secrets(HttpClient client)
        {
            _client = client;
        }

        /// <summary>Retrieve the value of the secret with the key <paramref name="key" /></summary>
        public async Task<string> GetSecretRawAsync(string key, CancellationToken ct = default)
        {
            key = key.Trim('/');
            using (var resp = await _client.GetAsync($"{_dataPrefix}/{key}", ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var parsed = await resp.Content.ReadAsAsync<GetSecretApiResponse>(ct);
                return parsed.Value;
            }
        }

        /// <summary>
        /// Retrieve the value of the secret with the key <paramref name="key" /> and
        /// deserialize it from JSON to an instance of <typeparam name="T" />
        /// </summary>
        public async Task<T> GetSecretAsync<T>(string key, CancellationToken ct = default)
        {
            var s = await GetSecretRawAsync(key, ct);
            return JsonConvert.DeserializeObject<T>(s);
        }

        /// <summary>Create a new secret at <paramref name="key" /> with a value of <paramref name="value" /></summary>
        public async Task<string> CreateSecretRawAsync(string key, string value, CancellationToken ct = default)
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
        public async Task<string> CreateSecretAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var serialized = JsonConvert.SerializeObject(value);
            return await CreateSecretRawAsync(key, serialized, ct);
        }

        /// <summary>Update the secret at <paramref name="key" /> with a new value of <paramref name="value" /></summary>
        public async Task UpdateSecretRawAsync(string key, string value, CancellationToken ct = default)
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
        public async Task UpdateSecretAsync<T>(string key, T value, CancellationToken ct = default)
        {
            var serialized = JsonConvert.SerializeObject(value);
            await UpdateSecretRawAsync(key, serialized, ct);
        }

        /// <summary>Delete the secret at <paramref name="key" /></summary>
        public async void DeleteSecretAsync(string key, CancellationToken ct = default)
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
        public async Task<IEnumerable<string>> ListSecretsAsync(string prefix = "", bool recursive = false, CancellationToken ct = default)
        {
            if (!recursive)
            {
                return await ListSecretsOnceAsync(prefix, ct);
            }

            var results = new List<string>();
            var pending = new List<string> { prefix };

            while (pending.Any())
            {
                var key = pending[pending.Count - 1];
                pending.RemoveAt(pending.Count - 1);

                var keys = await ListSecretsOnceAsync(key, ct);
                results.AddRange(keys.Where(k => !k.EndsWith("/")));
                pending.AddRange(keys.Where(k => k.EndsWith("/")));
            }

            return results;
        }

        private async Task<IEnumerable<string>> ListSecretsOnceAsync(string prefix, CancellationToken ct)
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

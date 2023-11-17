using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QarnotSDK.Sdk;

namespace QarnotSDK
{
    /// <summary>
    /// Client used to interact with the Qarnot carbon API.
    /// </summary>
    public class CarbonCalculator
    {
        private HttpClient _client { get; }
        private static string _commonPrefix = "carbon/v1";
        private static string _tasksPrefix = "tasks";
        private static string _poolsPrefix = "pools";
        private static string _carbonSuffix = "carbon-facts";
        private static string _defaultRefereceDatacenter;

        internal CarbonCalculator(HttpClient client, string defaultReferenceDatacenter = null)
        {
            _client = client;
            _defaultRefereceDatacenter = defaultReferenceDatacenter;
        }

        /// <summary>Generate and retrieve the carbon facts of the compute element with uuid <paramref name="uuid" /></summary>
        public async Task<CarbonFacts> GetCarbonFactsAsync(string prefix, string uuid, string referenceDatacenter = null, CancellationToken ct = default)
        {
            var queryString = $"{_commonPrefix}/{prefix}/{uuid}/{_carbonSuffix}";
            if (referenceDatacenter != null && referenceDatacenter.Length > 0)
            {
                queryString += "?comparisonDatacenter=" + referenceDatacenter;
            }
            using (var resp = await _client.GetAsync(queryString, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var parsed = await resp.Content.ReadAsAsync<CarbonFacts>(ct);
                return parsed;
            }
        }

        /// <summary>
        /// Generate and retrieve the carbon facts of the task with uuid <paramref name="uuid" /> and
        /// deserialize it from JSON to an instance of <see cref="CarbonFacts" />
        /// </summary>
        public async Task<CarbonFacts> GetTaskCarbonFactsAsync(Guid uuid, string referenceDatacenter = null, CancellationToken ct = default)
        {
            var s = await GetCarbonFactsAsync(_tasksPrefix, uuid.ToString(), referenceDatacenter, ct);
            return s;
        }

        /// <summary>
        /// Generate and retrieve the carbon facts of the pool with uuid <paramref name="uuid" /> and
        /// deserialize it from JSON to an instance of <see cref="CarbonFacts" />
        /// </summary>
        public async Task<CarbonFacts> GetPoolCarbonFactsAsync(Guid uuid, string referenceDatacenter = null, CancellationToken ct = default)
        {
            var s = await GetCarbonFactsAsync(_poolsPrefix, uuid.ToString(), referenceDatacenter, ct);
            return s;
        }
    }
}

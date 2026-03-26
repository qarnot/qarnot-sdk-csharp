using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QarnotSDK.Sdk;

namespace QarnotSDK
{
    /// <summary>
    /// Client used to interact with the Qarnot budget API.
    /// </summary>
    public class CreditsClient
    {
        private HttpClient _client { get; }
        private static string _commonPrefix = "credits/v1";
        private static string _budgetsSuffix = "budgets";

        internal CreditsClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Retrieve the budgets for a project.
        /// </summary>
        /// <param name="projectUuid">The UUID of the project.</param>
        /// <param name="activeOnly">If true, only active budgets are retrieved. If false, all budgets including archived ones are returned. Default is true.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of budgets for the project.</returns>
        public async Task<List<Budget>> GetBudgetsAsync(Guid projectUuid, bool activeOnly = true, CancellationToken ct = default)
        {
            if (projectUuid == default)
            {
                throw new ArgumentException("Invalid projectUuid when requesting budgets. Should not be default Guid");
            }
            var queryString = $"{_commonPrefix}/projects/{projectUuid}/{_budgetsSuffix}";
            queryString += $"?activeOnly={activeOnly.ToString().ToLower()}";

            using (var resp = await _client.GetAsync(queryString, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var response = await resp.Content.ReadAsAsync<BudgetListResponse>(ct);
                return response?.Budgets ?? new List<Budget>();
            }
        }

        private class BudgetListResponse
        {
            [JsonProperty("budgets")]
            public List<Budget> Budgets { get; set; }
        }
    }
}

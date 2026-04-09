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
        private static string _computeCreditsSuffix = "credits";

        internal CreditsClient(HttpClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Retrieve the credits consumed by the task
        /// </summary>
        /// <param name="taskUuid">The UUID of the project.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The credits consumed by the execution of the task.</returns>
        public async Task<Credits> GetTaskCreditsAsync(Guid taskUuid, CancellationToken ct = default)
        {
            if (taskUuid == default)
            {
                throw new ArgumentException("Invalid taskUuid when requesting consumed credits. Should not be default Guid");
            }
            var queryString = $"{_commonPrefix}/tasks/{taskUuid}/{_computeCreditsSuffix}";

            using (var resp = await _client.GetAsync(queryString, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var creditsResponse = await resp.Content.ReadAsAsync<CreditsResponse>(ct);
                return creditsResponse != null ? new Credits(creditsResponse.CreditsInEuros): null;
            }
        }

        /// <summary>
        /// Retrieve the credits consumed by the pool
        /// </summary>
        /// <param name="poolUuid">The UUID of the project.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The credits consumed by the execution of the pool.</returns>
        public async Task<Credits> GetPoolCreditsAsync(Guid poolUuid, CancellationToken ct = default)
        {
            if (poolUuid == default)
            {
                throw new ArgumentException("Invalid poolUuid when requesting consumed credits. Should not be default Guid");
            }
            var queryString = $"{_commonPrefix}/pools/{poolUuid}/{_computeCreditsSuffix}";

            using (var resp = await _client.GetAsync(queryString, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var creditsResponse = await resp.Content.ReadAsAsync<CreditsResponse>(ct);
                return creditsResponse != null ? new Credits(creditsResponse.CreditsInEuros): null;
            }
        }

        /// <summary>
        /// Retrieve the credits of the account
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The credits left in the account.</returns>
        public async Task<Credits> GetAccountCreditsAsync(CancellationToken ct = default)
        {
            var queryString = $"{_commonPrefix}/accounts/{_computeCreditsSuffix}";

            using (var resp = await _client.GetAsync(queryString, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_client, resp, ct);
                var creditsResponse = await resp.Content.ReadAsAsync<CreditsResponse>(ct);
                return creditsResponse != null ? new Credits(creditsResponse.CreditsInEuros): null;
            }
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

        private class CreditsResponse
        {
            [JsonProperty("Credits")]
            public long CreditsInEuros { get; set; } // in euros
        }
    }

    /// <summary>
    /// Representation for the credits
    /// </summary>
    public class Credits
    {
        private long _creditsInEuros;
        /// <summary>
        /// Amount of credits in euros, round to cent
        /// </summary>
        public long CreditsInEuros => _creditsInEuros;
        /// <summary>
        /// Amount of credits in cents
        /// </summary>
        public int CreditsInCents => (int)(_creditsInEuros * 100);
        /// <summary>
        /// Constructor for the credits
        /// </summary>
        /// <param name="creditsInEuros"></param>
        public Credits(long creditsInEuros)
        {
            _creditsInEuros = creditsInEuros;
        }
    }
}

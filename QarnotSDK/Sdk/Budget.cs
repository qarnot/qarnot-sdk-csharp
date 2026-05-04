using System;
using Newtonsoft.Json;

namespace QarnotSDK.Sdk
{
    /// <summary>
    /// Budget overrun policy that will act on project's compute if the budget is exceeded.
    /// </summary>
    public enum BudgetOverrunPolicy
    {
        /// <summary>Alert only will be sent when budget is exceeded. No action on the compute.</summary>
        AlertOnly,

        /// <summary>Block new compute when budget is exceeded.</summary>
        BlockNew,

        /// <summary>Stop all running compute and block new ones when budget is exceeded.</summary>
        StopAndBlockAll,
    }

    /// <summary>
    /// Represents a budget for a project.
    /// </summary>
    public class Budget
    {
        /// <summary>
        /// The unique identifier of the budget.
        /// </summary>
        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        /// <summary>
        /// The display name of the budget.
        /// </summary>
        [JsonProperty("alias")]
        public string Alias { get; set; }

        /// <summary>
        /// The amount of the budget already consumed in cents.
        /// </summary>
        [JsonProperty("consumedAmountInCents")]
        public long ConsumedAmountInCents { get; set; }

        /// <summary>
        /// The total budget in cents.
        /// </summary>
        [JsonProperty("totalAmountInCents")]
        public long TotalAmountInCents { get; set; }

        /// <summary>
        /// The remaining amount of the budget in cents.
        /// </summary>
        [JsonProperty("remainingAmountInCents")]
        public long RemainingAmountInCents { get; set; }

        /// <summary>
        /// The start date of the budget in UTC.
        /// From this date the budget is consumed and can trigger its BudgetOverrunPolicy.
        /// </summary>
        [JsonProperty("startDateUtc")]
        public DateTime StartDateUtc { get; set; }

        /// <summary>
        /// The expiration date of the budget in UTC (if any).
        /// If ExpirationDateUtc is null, the budget will always be active and consumed until it is archived.
        /// </summary>
        [JsonProperty("expirationDateUtc")]
        public DateTime? ExpirationDateUtc { get; set; }

        /// <summary>
        /// The policy that will act on project's compute if the budget is exceeded.
        /// </summary>
        [JsonProperty("budgetOverrunPolicy")]
        public BudgetOverrunPolicy BudgetOverrunPolicy { get; set; }

        /// <summary>
        /// Whether the budget is archived.
        /// An archived budget is not active: it is not consumed anymore and cannot trigger the BudgetOverrunPolicy
        /// </summary>
        [JsonProperty("isArchived")]
        public bool IsArchived { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[Budget {Uuid} ({Alias}): remaining {RemainingAmountInCents}/{TotalAmountInCents} cents (archived: {IsArchived}, active from {StartDateUtc} to {ExpirationDateUtc.ToString() ?? "-"})]";
        }
    }
}

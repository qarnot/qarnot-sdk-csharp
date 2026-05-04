using QarnotSDK.Sdk;

namespace QarnotSDK.UnitTests.SdkTests.Budget
{
    public static class BudgetTestData
    {
        public const string BudgetUuid = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
        public const string BudgetAlias = "Q1 Budget";
        public const long ConsumedAmountInCents = 50000;
        public const long TotalAmountInCents = 100000;
        public const long RemainingAmountInCents = 50000;
        public const string StartDateUtc = "2026-03-26T09:53:57.414Z";
        public const string ExpirationDateUtc = "2026-06-27T09:53:57.414Z";
        public const BudgetOverrunPolicy BudgetPolicy = BudgetOverrunPolicy.AlertOnly;
        public const bool IsArchived = false;

        private static string budgetResponseBody = $@"
        {{
            ""budgets"": [
                {{
                    ""uuid"": ""{BudgetUuid}"",
                    ""alias"": ""{BudgetAlias}"",
                    ""consumedAmountInCents"": {ConsumedAmountInCents},
                    ""totalAmountInCents"": {TotalAmountInCents},
                    ""remainingAmountInCents"": {RemainingAmountInCents},
                    ""startDateUtc"": ""{StartDateUtc}"",
                    ""expirationDateUtc"": ""{ExpirationDateUtc}"",
                    ""budgetOverrunPolicy"": ""{BudgetPolicy}"",
                    ""isArchived"": {IsArchived.ToString().ToLower()}
                }}
            ]
        }}
        ";

        public static string BudgetResponseBody { get => budgetResponseBody; set => budgetResponseBody = value; }

        public static string EmptyBudgetsResponseBody = @"
        {
            ""budgets"": []
        }
        ";
    }
}

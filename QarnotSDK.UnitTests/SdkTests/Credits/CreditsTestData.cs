namespace QarnotSDK.UnitTests.SdkTests.Credits
{
    public static class CreditsTestData
    {
        public const long CreditsInEuro = (long)50000.42;

        private static string creditsResponseBody = $@"
        {{
            ""credits"": {CreditsInEuro}
        }}
        ";

        public static string CreditsResponseBody { get => creditsResponseBody; set => creditsResponseBody = value; }
    }
}

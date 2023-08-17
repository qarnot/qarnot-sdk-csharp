namespace QarnotSDK.IntegrationTests
{
    public class CephClientOptions
    {
        public CephClientOptions() { }
        public CephClientOptions(
            string endpoint,
            string accessKey,
            string secretKey,
            string adminPath = "/admin")
        {
            RadosGWEndpoint = endpoint;
            RadosGWAdminAccessKey = accessKey;
            RadosGWAdminSecretKey = secretKey;
            RadosGWAdminAdminPath = adminPath;
        }

        public string RadosGWEndpoint { get; set; }
        public string RadosGWAdminAccessKey { get; set; }
        public string RadosGWAdminSecretKey { get; set; }
        public string RadosGWAdminAdminPath { get; set; } = "/admin";
    }
}
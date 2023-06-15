using System.Collections.Generic;

namespace QarnotSDK
{
    internal class GetSecretApiResponse
    {
        public string Value { get; set; }
    }

    internal class CreateSecretApiRequest
    {
        public string Value { get; set; }
    }

    internal class UpdateSecretApiRequest
    {
        public string Value { get; set; }
    }

    internal class ListSecretsApiResponse
    {
        public List<string> Keys { get; set; }
    }
}

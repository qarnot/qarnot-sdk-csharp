namespace QarnotSDK
{
    using System.Collections.Generic;

    internal class PaginatedResponseAPI<T>
    {
        public string Token { get; set; }
        public string NextToken { get; set; }
        public bool IsTruncated { get; set; }
        public IEnumerable<T> Data { get; set; }

        public PaginatedResponseAPI()
        {
            Token = default;
            NextToken = default;
            IsTruncated = false;
            Data = new List<T>();
        }
    }
}
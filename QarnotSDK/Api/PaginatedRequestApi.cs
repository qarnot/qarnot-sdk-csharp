namespace QarnotSDK
{
    internal class PaginatedRequestApi<T>
    {
        public string Token { get; set; }
        public FilterApi<T> Filter { get; set; }
        public int? MaximumResults { get; set; }

        internal PaginatedRequestApi()
        {
        }

        public PaginatedRequestApi(string nextToken, FilterApi<T> filter, int maxResultNumber)
        {
            Token = nextToken;
            Filter = filter;
            MaximumResults = maxResultNumber;
        }

        public override string ToString()
            => $"<PageDetail= {Token}, Filter: {Filter}, MaximumResults: {MaximumResults}>";
    }
}
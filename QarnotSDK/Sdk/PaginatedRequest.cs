namespace QarnotSDK
{
    /// <summary>
    /// This class manage the Api request pages list for a specified class
    /// </summary>
    /// <typeparam name="T">the class to call</typeparam>
    public class PaginatedRequest<T>
    {
        internal PaginatedRequestApi<T> _pageRequestApi { get; } = new PaginatedRequestApi<T>();

        /// <summary>
        /// Construct a new PaginatedRequest object empty.
        /// </summary>
        public PaginatedRequest()
        {
        }

        /// <summary>
        /// Construct a new PaginatedRequest object.
        /// </summary>
        /// <param name="maxResultByPage">Maximum list number of objects to be retrun.</param>
        /// <param name="token">Pagination token to get the next list.</param>
        /// <param name="filter">Filter.</param>
        public PaginatedRequest(int maxResultByPage, string token = null, QFilter<T> filter = null)
        {
            MaximumResults = maxResultByPage;
            Token = token;
            Filter = filter;
        }

        /// <summary>
        /// Token given by the last page call
        /// return it to have the next page
        /// set it to it default value (null) to restart the paging from the start.
        /// </summary>
        /// <value>The token of the next page.</value>
        public string Token
        {
            get => _pageRequestApi.Token;
            set => _pageRequestApi.Token = value;
        }

        /// <summary>
        /// The filtering part, specification logical filters on the object properties
        /// </summary>
        /// <value>the filter</value>
        public QFilter<T> Filter
        {
            get => new QFilter<T>() { _filterApi = _pageRequestApi.Filter };
            set => _pageRequestApi.Filter = value?._filterApi;
        }

        /// <summary>
        /// Maximum results number of result by page.
        /// </summary>
        /// <value>The maximum result for the query.</value>
        public int? MaximumResults
        {
            get => _pageRequestApi.MaximumResults;
            set => _pageRequestApi.MaximumResults = value;
        }

        /// <summary>
        /// Change the request to get the next page send by the Api.
        /// </summary>
        /// <param name="response">Last response send by the api.</param>
        /// <returns>Is there a new page to get.</returns>
        public bool PrepareNextPage(PaginatedResponse<T> response)
        {
            if (response.IsTruncated)
            {
                Token = response.NextToken;
            }

            return response.IsTruncated;
        }

        /// <summary>
        /// Override to string
        /// </summary>
        /// <returns>string format</returns>
        public override string ToString()
            => $"<PageDetail= {Token}, Filter: {Filter}, MaximumResults: {MaximumResults}>";
    }
}
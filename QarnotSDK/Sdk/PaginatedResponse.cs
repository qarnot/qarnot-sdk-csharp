namespace QarnotSDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// This class manages the Api Response pages list `PaginatedResponseAPI` for a specified class.
    /// </summary>
    /// <typeparam name="TSdk">the return class</typeparam>
    public class PaginatedResponse<TSdk>
    {
        /// <summary>
        /// The list of objects created.
        /// </summary>
        /// <value>A class list.</value>
        public List<TSdk> Data { get; private set; }

        /// <summary>
        /// The token used by the request call.
        /// </summary>
        /// <value>token value</value>
        public string Token { get; }

        /// <summary>
        /// The token to be used to have the next page.
        /// </summary>
        /// <value>The token value.</value>
        public string NextToken { get; }

        /// <summary>
        /// There is a next page to retrieve.
        /// </summary>
        /// <value>A next page can be call.</value>
        public bool IsTruncated { get; }

        private PaginatedResponse()
        {
        }

        internal PaginatedResponse(string token, string nextToken, bool isTruncated)
        {
            Token = token;
            NextToken = nextToken;
            IsTruncated = isTruncated;
        }

        internal async Task<List<TSdk>> ConvertApiResponseAsync<TApi>(Connection connection, PaginatedResponseAPI<TApi> apiResponse, Func<Connection, TApi, Task<TSdk>> CreateSdkObject)
        {
            return (await Task.WhenAll(apiResponse
                                      .Data
                                      .Select(async data => await CreateSdkObject(connection, data))))
                               .ToList();
        }

        internal async static Task<PaginatedResponse<TSdk>> CreateAsync<TApi>(Connection connection, PaginatedResponseAPI<TApi> apiResponse, Func<Connection, TApi, Task<TSdk>> CreateSdkObject)
        {
            var pageResponse = new PaginatedResponse<TSdk>(apiResponse.Token, apiResponse.NextToken, apiResponse.IsTruncated);
            pageResponse.Data = await pageResponse.ConvertApiResponseAsync<TApi>(connection, apiResponse, CreateSdkObject);
            return pageResponse;
        }
    }
}
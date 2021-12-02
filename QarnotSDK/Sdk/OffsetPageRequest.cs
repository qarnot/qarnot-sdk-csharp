namespace QarnotSDK
{
    using System;

    /// <summary>
    /// Create an API request with a limited page in response
    /// </summary>
    public class OffsetPageRequest
    {
        private const int MAX_PAGE_SIZE = 50;
        private int limit;
        /// <summary>
        /// The limit number of elements to display in the response page. Cannot be over the default value <see cref="MAX_PAGE_SIZE"/>
        /// </summary>
        /// <value></value>
        public int Limit
        {
            get => limit;
            set => limit = value <= 0 || value > MAX_PAGE_SIZE ? MAX_PAGE_SIZE : value;
        }

        /// <summary>
        /// The number of elements to ignore.
        /// </summary>
        /// <value></value>
        public int Offset { get; set; }

        /// <summary>
        /// Create a new empty OffsetPageRequest
        /// </summary>
        public OffsetPageRequest()
        {
            Limit = MAX_PAGE_SIZE;
            Offset = 0;
        }

        /// <summary>
        /// Create a new OffsetPageRequest with custom limit and offset.
        /// </summary>
        /// <param name="limit">The maximum number of elements of a page (cannot be more than the default value <see cref="MAX_PAGE_SIZE"/>)</param>
        /// <param name="offset">The response page offset. Default to 0.</param>
        public OffsetPageRequest(int limit, int offset = 0)
        {
            Limit = limit;
            Offset = offset;
        }

        /// <summary>
        /// Override to string
        /// </summary>
        /// <returns>string format</returns>
        public override string ToString()
            => $"<OffsetPageRequest= Limit: {Limit}, Offset: {Offset}>";

        /// <summary>
        /// Get the query with the request as parameters
        /// </summary>
        /// <returns></returns>
        public string GetAsQueryString()
        {
            var limitString = String.Format("Limit={0}", Limit);
            var offsetString = String.Format("Offset={0}", Offset);
            return String.Format("{0}&{1}", limitString, offsetString);
        }
    }
}
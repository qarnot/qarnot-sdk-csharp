using System.Collections.Generic;

namespace QarnotSDK
{
    /// <summary>
    /// API limited page response with an offset
    /// </summary>
    /// <typeparam name="TSdk">the return class</typeparam>
    public class OffsetPageResponse<TSdk>
    {
        /// <summary>
        /// The list of objects created.
        /// </summary>
        /// <value>A class list.</value>
        public List<TSdk> Data { get; set; }

        /// <summary>
        /// The number of elements ignored before starting to display the response.
        /// </summary>
        /// <value></value>
        public int Offset { get; set; }

        /// <summary>
        /// The limit number of elements displayed in the response page.
        /// </summary>
        /// <value></value>
        public int Limit { get; set; }

        /// <summary>
        /// The total number of elements in the original response.
        /// </summary>
        /// <value></value>
        public int Total { get; set; }
    }
}
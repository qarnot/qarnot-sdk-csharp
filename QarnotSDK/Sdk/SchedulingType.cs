using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QarnotSDK
{
    /// <summary>
    /// Available scheduling Types for a compute item.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SchedulingType
    {
        /// <summary>
        /// Flex scheduling type which is the default scheduling, with low priority & low pricing
        /// </summary>
        Flex,

        /// <summary>
        /// On-demand scheduling type, higher priority and pricing
        /// </summary>
        OnDemand,

        /// <summary>
        /// Reserved scheduling type, associated to as specific reserved machine
        /// </summary>
        Reserved
    }
}
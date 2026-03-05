using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QarnotSDK
{
    /// <summary>
    /// Represents the final state a task can reach, used as a condition in advanced dependencies.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TaskFinalState
    {
        /// <summary>
        /// The task completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The task failed.
        /// </summary>
        Failure,

        /// <summary>
        /// The task was cancelled.
        /// </summary>
        Cancelled
    }
}

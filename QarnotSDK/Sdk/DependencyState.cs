using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QarnotSDK
{
    /// <summary>
    /// Represents the state of a dependency (overall or per-dependency).
    /// Populated from API responses only.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DependencyState
    {
        /// <summary>
        /// The dependency is still waiting for the depended-on task to complete.
        /// </summary>
        Waiting,

        /// <summary>
        /// The dependency conditions have been fulfilled, i.e all depended-on tasks have completed, all in the listed final states
        /// </summary>
        DependencyConditionsFulfilled,

        /// <summary>
        /// The dependency conditions have not been fulfilled, i.e at least one of the depended-on tasks has completed in a non-listed final state.
        /// </summary>
        /// <remark>
        /// Some depended-on tasks may still be running, only one has to completed in a non-listed state for the current task to be in this state.
        /// </remark>
        DependencyConditionsNotFulfilled
    }
}

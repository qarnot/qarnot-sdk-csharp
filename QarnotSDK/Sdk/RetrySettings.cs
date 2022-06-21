namespace QarnotSDK
{
    /// <summary>
    /// Retry configuration for the failed instance.
    /// If neither `MaxTotalRetries` nor `MaxPerInstanceRetries` are set (or if they are equal to 0), the instances will not retry.
    /// If both `MaxTotalRetries` and `MaxPerInstanceRetries` are set, then the most restrictive applies.
    /// </summary>
    public class RetrySettings
    {
        /// <summary>
        /// Maximum total number of retries for the whole task.
        /// Default to null (equivalent to 0, meaning no global limitation).
        /// </summary>
        /// <example>12</example>
        public uint? MaxTotalRetries { get; set; }

        /// <summary>
        /// Maximum number of retries for each task instance.
        /// Default to null (equivalent to 0, meaning no limitation per instance).
        /// </summary>
        /// <example>12</example>
        public uint? MaxPerInstanceRetries { get; set; }
    }
}
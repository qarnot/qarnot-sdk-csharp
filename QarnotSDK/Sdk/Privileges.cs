namespace QarnotSDK
{
    /// <summary>
    /// List of privileges the task or pool is granted by the user
    /// </summary>
    public class Privileges
    {
        /// <summary>
        /// Allow the api and storage credentials to be exported into the environment through constants.
        /// Default is false.
        /// </summary>
        /// <example>true</example>
        public bool? ExportApiAndStorageCredentialsInEnvironment { get; set; }
    }
}
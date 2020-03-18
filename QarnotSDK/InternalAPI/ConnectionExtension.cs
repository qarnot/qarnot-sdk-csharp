using System.Threading.Tasks;
using System.Threading;

namespace QarnotSDK.Internal
{
    /// <summary>
    /// Providing extension to allow the use of sdk internals
    /// </summary>
    public static class ConnectionExtension 
    {
        /// <summary>
        /// Retrieve the configured S3 client used by the SDK
        /// </summary>
        /// <param name="connection">the Qarnot SDK Connection</param>
        /// <param name="ct">cancellation token</param>
        /// <returns>an Amazon s3 client configured to reach the storage service</returns>
        public static async Task<Amazon.S3.AmazonS3Client> GetConfiguredS3ClientAsync(this Connection connection, CancellationToken ct = default)
        {
            return await connection.GetS3ClientAsync(ct);
        }
    }
}

using System.Threading;
using System;

namespace QarnotSDK {

    /// <summary>
    /// This class manages pools life cycle: submission, monitor, delete.
    /// </summary>
    public abstract partial class AQPool {
        #region public methods
        /// <summary>
        /// Stop the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        [Obsolete("use Close")]
        public virtual void Stop(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                StopAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Abort a running pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public virtual void Close(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CloseAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the pool. If the pool is running, the pool is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        public abstract void Delete(CancellationToken cancellationToken, bool failIfDoesntExist = false, bool purgeResources=false);

        /// <summary>
        /// Delete the pool. If the pool is running, the pool is aborted and deleted.
        /// </summary>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        public virtual void Delete(bool failIfDoesntExist = false, bool purgeResources=false)
            => Delete(default(CancellationToken) ,failIfDoesntExist, purgeResources);
        #endregion
        
	/// <summary>
	/// Request made on a running pool to re-sync the resource buckets to the compute nodes.
        ///  1 - Upload new files on your resource bucket,
        ///  2 - Call this method,
        ///  3 - The new files will appear on all the compute nodes in the $DOCKER_WORKDIR folder
        /// Note: There is no way to know when the files are effectively transfered. This information is available on the compute node only.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
	public virtual void UpdateResources(CancellationToken cancellationToken = default(CancellationToken))
	{
            try {
                UpdateResourcesAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
	}
    }
}

using System;
using System.Threading;

namespace QarnotSDK
{
    public partial class QPool : AQPool {
        #region public methods
        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        [Obsolete("Start is deprecated, please use StartAsync instead.")]
        public virtual void Start(string profile = null, uint initialNodeCount = 0) {
            try {
                StartAsync(profile, initialNodeCount).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        [Obsolete("Start is deprecated, please use StartAsync instead.")]
        public virtual void Start(CancellationToken cancellationToken, string profile = null, uint initialNodeCount = 0) {
            try {
                StartAsync(cancellationToken, profile, initialNodeCount).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }


        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="updateQBucketsInfo">If set to true, the resources bucket objects are also updated.</param>
        /// <returns></returns>
        [Obsolete("UpdateStatus is deprecated, please use UpdateStatusAsync instead.")]
        public virtual void UpdateStatus(bool updateQBucketsInfo = false) {
            try {
                UpdateStatusAsync(updateQBucketsInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateQBucketsInfo">If set to true, the resources bucket objects are also updated.</param>
        /// <returns></returns>
        [Obsolete("UpdateStatus is deprecated, please use UpdateStatusAsync instead.")]
        public virtual void UpdateStatus(CancellationToken cancellationToken, bool updateQBucketsInfo = false) {
            try {
                UpdateStatusAsync(cancellationToken, updateQBucketsInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }


        /// <summary>
        /// Delete the pool. If the pool is running, the pool is closed and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        [Obsolete("Delete is deprecated, please use DeleteAsync instead.")]
        public override void Delete(CancellationToken cancellationToken = default(CancellationToken), bool failIfDoesntExist = false, bool purgeResources=false)
        {
            try {
                DeleteAsync(cancellationToken, failIfDoesntExist,purgeResources).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Commit the local pool changes.
        /// </summary>
        [Obsolete("Commit is deprecated, please use CommitAsync instead.")]
        public virtual void Commit(CancellationToken cancellationToken = default(CancellationToken)) {
           try {
                CommitAsync().Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}
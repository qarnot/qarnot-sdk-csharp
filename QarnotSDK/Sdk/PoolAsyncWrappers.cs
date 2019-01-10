using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK
{
    public partial class QPool {
        #region public methods
        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        public void Start(string profile = null, uint initialNodeCount = 0) {
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
        public void Start(CancellationToken cancellationToken, string profile = null, uint initialNodeCount = 0) {
            try {
                StartAsync(cancellationToken, profile, initialNodeCount).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Stop the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        [Obsolete("use Close")]
        public void Stop(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                StopAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Close the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Close(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CloseAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the pool. If the pool is running, the pool is closed and deleted.
        /// </summary>
        /// <param name="failIfDoesntExist">If set to true and the pool doesn't exist, an exception is thrown. Default is false.</param>
        /// <returns></returns>
        public void Delete(bool failIfDoesntExist = false) {
            try {
                DeleteAsync(failIfDoesntExist).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the pool. If the pool is running, the pool is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <returns></returns>
        public void Delete(CancellationToken cancellationToken, bool failIfDoesntExist = false) {
            try {
                DeleteAsync(cancellationToken, failIfDoesntExist).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="updateDisksInfo">If set to true, the resources disk objects are also updated.</param>
        /// <returns></returns>
        public void UpdateStatus(bool updateDisksInfo = false) {
            try {
                UpdateStatusAsync(updateDisksInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateDisksInfo">If set to true, the resources disk objects are also updated.</param>
        /// <returns></returns>
        public void UpdateStatus(CancellationToken cancellationToken, bool updateDisksInfo = false) {
            try {
                UpdateStatusAsync(cancellationToken, updateDisksInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Commit the local pool changes.
        /// </summary>
        public void Commit(CancellationToken cancellationToken = default(CancellationToken)) {
           try {
                CommitAsync().Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}
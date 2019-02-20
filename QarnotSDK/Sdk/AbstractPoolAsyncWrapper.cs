using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
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
        public void Stop(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                StopAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Abort a running task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Abort(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CloseAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="failIfDoesntExist">If set to true and the task doesn't exist, an exception is thrown. Default is false.</param>
        public void Delete(bool failIfDoesntExist = false) {
            try {
                DeleteAsync(failIfDoesntExist).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to true and the task doesn't exist, an exception is thrown. Default is false.</param>
        public void Delete(CancellationToken cancellationToken, bool failIfDoesntExist = false) {
            try {
                DeleteAsync(cancellationToken, failIfDoesntExist).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}

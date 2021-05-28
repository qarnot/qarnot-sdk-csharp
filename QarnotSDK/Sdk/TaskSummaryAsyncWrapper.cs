using System.Threading;
using System;


namespace QarnotSDK {

    /// <summary>
    /// This class manages tasks life cycle: submission, monitor, delete.
    /// </summary>
    public partial class QTaskSummary : AQTask {
        /// <summary>
        /// Get The Full Task from this task summary.
        /// <param name="ct">Optional token to cancel the request.</param>
        /// </summary>
        [Obsolete("GetFullQTask is deprecated, please use GetFullQTaskAsync instead.")]
        public virtual QTask GetFullQTask(CancellationToken ct = default(CancellationToken)) {
            try {
                return GetFullQTaskAsync(ct).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the task doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <param name="purgeResults">Boolean to trigger result storage deletion. Default is false.</param>
        /// <returns></returns>
        [Obsolete("Delete is deprecated, please use DeleteAsync instead.")]
        public override void Delete(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false, bool purgeResults=false)
        {
            try {
                DeleteAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
    }
}

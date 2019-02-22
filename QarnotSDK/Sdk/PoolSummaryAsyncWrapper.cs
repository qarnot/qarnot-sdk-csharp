using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using System;


namespace QarnotSDK {

    /// <summary>
    /// This class manages pools life cycle: submission, monitor, delete.
    /// </summary>
    public partial class QPoolSummary : AQPool {
        /// <summary>
        /// Get The Full Pool from this task summary.
        /// <param name="ct">Optional token to cancel the request.</param>
        /// </summary>
        public QPool GetFullQPool(CancellationToken ct = default(CancellationToken)) {
            try {
                return GetFullQPoolAsync(ct).Result;
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
        public override void Delete(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false)
        {
            try {
                DeleteAsync(cancellationToken, failIfDoesntExist, purgeResources).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
    }
}

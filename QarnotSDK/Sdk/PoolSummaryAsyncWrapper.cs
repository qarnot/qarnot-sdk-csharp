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
    }
}

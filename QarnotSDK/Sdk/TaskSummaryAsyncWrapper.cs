using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
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
        public QTask GetFullQTask(CancellationToken ct = default(CancellationToken)) {
            try {
                return GetFullQTaskAsync(ct).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
    }
}

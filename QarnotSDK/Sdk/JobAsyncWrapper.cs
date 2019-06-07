using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using System;


namespace QarnotSDK {
    /// <summary>
    /// This class manages Jobs life cycle: logical group of tasks.
    /// </summary>
    public partial class QJob
    {

        /// <summary>
        /// Submit this job.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Submit(CancellationToken cancellationToken=default(CancellationToken))
        {
            try {
                SubmitAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this job.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UpdateStatus(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UpdateStatusAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Terminate an active job. (will cancel all remaining tasks)
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Terminate(CancellationToken cancellationToken = default(CancellationToken))
        {
            try {
                TerminateAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the job. If the job is active, the job is terminated and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Delete(CancellationToken cancellationToken = default(CancellationToken))
        {
            try {
                DeleteAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
    }
}
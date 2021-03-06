using System.Threading;
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
        [Obsolete("Submit is deprecated, please use SubmitAsync instead.")]
        public virtual void Submit(CancellationToken cancellationToken=default(CancellationToken))
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
        [Obsolete("UpdateStatus is deprecated, please use UpdateStatusAsync instead.")]
        public virtual void UpdateStatus(CancellationToken cancellationToken = default(CancellationToken)) {
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
        [Obsolete("Terminate is deprecated, please use TerminateAsync instead.")]
        public virtual void Terminate(CancellationToken cancellationToken = default(CancellationToken))
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
        /// <param name="force">Optional boolean to force inner tasks to be deleted.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        [Obsolete("Delete is deprecated, please use DeleteAsync instead.")]
        public virtual void Delete(bool force = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            try {
                DeleteAsync(force, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
    }
}
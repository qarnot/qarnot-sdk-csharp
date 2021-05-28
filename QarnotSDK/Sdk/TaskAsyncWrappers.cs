using System.Threading;
using System;

namespace QarnotSDK {
    public partial class QTask {
        #region public methods
        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="instanceCount">How many times the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to 0.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        [Obsolete("Submit is deprecated, please use SubmitAsync instead.")]
        public virtual void Submit(string profile=null, uint instanceCount=0, CancellationToken cancellationToken=default(CancellationToken)) {
            try {
                SubmitAsync(profile, instanceCount, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="range">Which instance ids of the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to null.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        [Obsolete("Submit is deprecated, please use SubmitAsync instead.")]
        public virtual void Submit(string profile, AdvancedRanges range, CancellationToken cancellationToken=default(CancellationToken)) {
            try {
                SubmitAsync(profile, range, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }


        /// <summary>
        /// Run this task.
        /// </summary>
        /// <param name="taskTimeoutSeconds">Optional number of second before abort is called.</param>
        /// <param name="outputDirectory">local directory for the retrieved files</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns></returns>
        [Obsolete("Run is deprecated, please use RunAsync instead.")]
        public virtual void Run(int taskTimeoutSeconds=-1, string outputDirectory=default, CancellationToken ct=default) {
            try {
                RunAsync(taskTimeoutSeconds, outputDirectory, ct).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Wait this task completion.
        /// </summary>
        /// <param name="taskTimeoutSeconds">Optional maximum number of second to wait for completion.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns></returns>
        [Obsolete("Wait is deprecated, please use WaitAsync instead.")]
        public virtual bool Wait(int taskTimeoutSeconds=-1, CancellationToken ct =default(CancellationToken)) {
            try {
                return WaitAsync(taskTimeoutSeconds, ct).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="updateQBucketsInfo">If set to true, the resources and results bucket objects are also updated.</param>
        [Obsolete("UpdateStatus is deprecated, please use UpdateStatusAsync instead.")]
        public virtual void UpdateStatus(bool updateQBucketsInfo = true) {
            try {
                UpdateStatusAsync(updateQBucketsInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateQBucketsInfo">If set to true, the resources and results bucket objects are also updated.</param>
        [Obsolete("UpdateStatus is deprecated, please use UpdateStatusAsync instead.")]
        public virtual void UpdateStatus(CancellationToken cancellationToken, bool updateQBucketsInfo = true) {
            try {
                UpdateStatusAsync(cancellationToken, updateQBucketsInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to true and the task doesn't exist, an exception is thrown. Default is false.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <param name="purgeResults">Boolean to trigger result storage deletion. Default is false.</param>
        [Obsolete("Delete is deprecated, please use DeleteAsync instead.")]
        public override void Delete(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false, bool purgeResults=false)
        {
            try {
                DeleteAsync(cancellationToken, failIfDoesntExist, purgeResources, purgeResults).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Commit the local task changes.
        /// </summary>
        [Obsolete("Commit is deprecated, please use CommitAsync instead.")]
        public virtual void Commit(CancellationToken cancellationToken = default) {
           try {
                CommitAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Download result in the given directory
        /// warning: Will override *output_dir* content.
        /// </summary>
        /// <param name="outputDirectory">local directory for the retrieved files</param>
        /// <param name="cancellationToken">Optional token to cancel the request</param>
        [Obsolete("DownloadResult is deprecated, please use DownloadResultAsync instead.")]
        public virtual void DownloadResult (string outputDirectory, CancellationToken cancellationToken=default) {
            try {
                DownloadResultAsync(outputDirectory, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        #endregion
    }
}

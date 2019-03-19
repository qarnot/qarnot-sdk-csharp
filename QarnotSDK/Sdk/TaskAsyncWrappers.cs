using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
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
        public void Submit(string profile=null, uint instanceCount=0, CancellationToken cancellationToken=default(CancellationToken)) {
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
        public void Submit(string profile, AdvancedRanges range, CancellationToken cancellationToken=default(CancellationToken)) {
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
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Run(int taskTimeoutSeconds=-1, CancellationToken ct =default(CancellationToken)) {
            try {
                RunAsync(taskTimeoutSeconds, ct).Wait();
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
        public void Wait(int taskTimeoutSeconds=-1, CancellationToken ct =default(CancellationToken)) {
            try {
                WaitAsync(taskTimeoutSeconds, ct).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="updateQBucketsInfo">If set to true, the resources and results bucket objects are also updated.</param>
        public void UpdateStatus(bool updateQBucketsInfo = true) {
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
        public void UpdateStatus(CancellationToken cancellationToken, bool updateQBucketsInfo = true) {
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

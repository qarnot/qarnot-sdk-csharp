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
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        public void Submit(string profile = null, bool autoCreateResultDisk = true) {
            try {
                SubmitAsync(profile, autoCreateResultDisk).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="instanceCount">How many times the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to 0.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        public void Submit(string profile, uint instanceCount = 0, bool autoCreateResultDisk = true) {
            try {
                SubmitAsync(profile, instanceCount, autoCreateResultDisk).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="range">Which instance ids of the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        public void Submit(string profile, AdvancedRanges range, bool autoCreateResultDisk = true) {
            try {
                SubmitAsync(profile, range, autoCreateResultDisk).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        public void Submit(CancellationToken cancellationToken, string profile = null, bool autoCreateResultDisk = true) {
            try {
                SubmitAsync(cancellationToken, profile, autoCreateResultDisk).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="instanceCount">How many times the task will run. Optional if the instance count has already been defined in the constructor, it can be set to 0.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        public void Submit(CancellationToken cancellationToken, string profile, uint instanceCount = 0, bool autoCreateResultDisk = true) {
            try {
                SubmitAsync(cancellationToken, profile, instanceCount, autoCreateResultDisk).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="range">Which instance ids of the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        public void Submit(CancellationToken cancellationToken, string profile, AdvancedRanges range, bool autoCreateResultDisk = true) {
            try {
                SubmitAsync(cancellationToken, profile, range, autoCreateResultDisk).Wait();
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
        /// <param name="updateDisksInfo">If set to true, the resources and results disk objects are also updated.</param>
        public void UpdateStatus(bool updateDisksInfo = true) {
            try {
                UpdateStatusAsync(updateDisksInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateDisksInfo">If set to true, the resources and results disk objects are also updated.</param>
        public void UpdateStatus(CancellationToken cancellationToken, bool updateDisksInfo = true) {
            try {
                UpdateStatusAsync(cancellationToken, updateDisksInfo).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}

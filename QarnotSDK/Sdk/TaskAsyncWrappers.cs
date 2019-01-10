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
        /// Abort a running task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Abort(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                AbortAsync(cancellationToken).Wait();
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

        #region resource updates/snapshots
        /// <summary>
        /// Request made on a running task to re-sync the resource disks to the compute nodes.
        ///  1 - Upload new files on your resource disk,
        ///  2 - Call this method,
        ///  3 - The new files will appear on all the compute nodes in the $DOCKER_WORKDIR folder
        /// Note: There is no way to know when the files are effectively transfered. This information is available on the compute node only.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void UpdateResources(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UpdateResourcesAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Request made on a running task to sync the result files in $DOCKER_WORKDIR on the compute node to the result disk.
        /// Note: There is no way to know when all the results are effectively transfered. This information is available by monitoring the
        /// task ResultsCount or by checking the result disk.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void Snapshot(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                SnapshotAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Start a periodic snapshotting of the results.
        /// </summary>
        /// <param name="interval">Interval in seconds between two snapshots.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void SnapshotPeriodic(uint interval, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                SnapshotPeriodicAsync(interval, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion

        #region stdin/stdout
        /// <summary>
        /// Copies the full standard output to the given stream.
        /// Note: the standard output will rotate if it's too large.
        /// </summary>
        /// <param name="destinationStream">The destination stream.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void CopyStdoutTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CopyStdoutToAsync(destinationStream, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Copies the full standard error to the given stream.
        /// Note: the standard error will rotate if it's too large.
        /// </summary>
        /// <param name="destinationStream">The destination stream.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void CopyStderrTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CopyStderrToAsync(destinationStream, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Copies the fresh new standard output since the last call to the given stream.
        /// </summary>
        /// <param name="destinationStream">The destination stream.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void CopyFreshStdoutTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CopyFreshStdoutToAsync(destinationStream, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Copies the fresh new standard error since the last call to the given stream.
        /// </summary>
        /// <param name="destinationStream">The destination stream.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void CopyFreshStderrTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CopyFreshStderrToAsync(destinationStream, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Returns the full standard output.
        /// Note: the standard output will rotate if it's too large.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task standard output.</returns>
        public string Stdout(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return StdoutAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Return the full standard error.
        /// Note: the standard error will rotate if it's too large.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task standard error.</returns>
        public string Stderr(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return StderrAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Returns the fresh new standard output since the last call.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task fresh standard output.</returns>
        public string FreshStdout(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return FreshStdoutAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Returns the fresh new standard error since the last call.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task fresh standard error.</returns>
        public string FreshStderr(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return FreshStderrAsync(cancellationToken).Result;
            } catch(AggregateException ex) {
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

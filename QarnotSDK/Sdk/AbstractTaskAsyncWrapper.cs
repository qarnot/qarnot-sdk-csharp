using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;

namespace QarnotSDK {
    public abstract partial class AQTask {
        #region public methods
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
        #endregion

    }
}

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
        public virtual void Abort(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                AbortAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the task doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <param name="purgeResults">Boolean to trigger result storage deletion. Default is false.</param>
        /// <returns></returns>
        public abstract void Delete(CancellationToken cancellationToken,
            bool failIfDoesntExist = false, bool purgeResources=false, bool purgeResults=false);

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="failIfDoesntExist">If set to false and the task doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <param name="purgeResults">Boolean to trigger result storage deletion. Default is false.</param>
        /// <returns></returns>
        public virtual void Delete(bool failIfDoesntExist = false, bool purgeResources=false, bool purgeResults=false)
            => Delete(default(CancellationToken), failIfDoesntExist, purgeResources, purgeResults);
        #endregion

        #region resource updates/snapshots
        /// <summary>
        /// Request made on a running task to re-sync the resource buckets to the compute nodes.
        ///  1 - Upload new files on your resource bucket,
        ///  2 - Call this method,
        ///  3 - The new files will appear on all the compute nodes in the $DOCKER_WORKDIR folder
        /// Note: There is no way to know when the files are effectively transfered. This information is available on the compute node only.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public virtual void UpdateResources(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UpdateResourcesAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Request made on a running task to sync the result files in $DOCKER_WORKDIR on the compute node to the result bucket.
        /// Note: There is no way to know when all the results are effectively transfered. This information is available by monitoring the
        /// task ResultsCount or by checking the result bucket.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public virtual void Snapshot(CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual void SnapshotPeriodic(uint interval, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual void CopyStdoutTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual void CopyStderrTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual void CopyFreshStdoutTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual void CopyFreshStderrTo(Stream destinationStream, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual string Stdout(CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual string Stderr(CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual string FreshStdout(CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual string FreshStderr(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return FreshStderrAsync(cancellationToken).Result;
            } catch(AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion

    }
}

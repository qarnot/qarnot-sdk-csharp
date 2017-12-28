using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace QarnotSDK {
    public partial class Connection {
        #region RetrieveX
        /// <summary>
        /// Retrieve the list of tasks.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public List<QTask> RetrieveTasks(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTasksAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of pools.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public List<QPool> RetrievePools(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolsAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }

        }

        /// <summary>
        /// Retrieve the list of disks.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of disks.</returns>
        public List<QDisk> RetrieveDisks(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveDisksAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of buckets.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of buckets.</returns>
        public List<QBucket> RetrieveBuckets(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveBucketsAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the storages list (buckets and disks).
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of disks and buckets.</returns>
        public List<QAbstractStorage> RetrieveStorages(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveStoragesAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the user quotas and disks information for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and disks information.</returns>
        public UserInformation RetrieveUserInformation(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveUserInformationAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of profiles available for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of profile names.</returns>
        public List<string> RetrieveProfiles(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveProfilesAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of the constants you can override for a specific profile.
        /// </summary>
        /// <param name="profile">Name of the profile.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of constants.</returns>
        public List<Constant> RetrieveConstants(string profile, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveConstantsAsync(profile, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion

        #region RetrieveXByName
        /// <summary>
        /// Retrieve a task by its name.
        /// </summary>
        /// <param name="name">Name of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that name or null if it hasn't been found.</returns>
        public QTask RetrieveTaskByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskByNameAsync(name, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a pool by its name.
        /// </summary>
        /// <param name="name">Name of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that name or null if it hasn't been found.</returns>
        public QPool RetrievePoolByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolByNameAsync(name, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a disk by its name.
        /// </summary>
        /// <param name="name">Name of the disk to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The disk object for that name or null if it hasn't been found.</returns>
        public QDisk RetrieveDiskByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveDiskByNameAsync(name, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}

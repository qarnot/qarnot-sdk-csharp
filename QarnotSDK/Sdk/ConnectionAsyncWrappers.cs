using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace QarnotSDK {
    public partial class Connection {
        #region CreateX
        /// <summary>
        /// Submit a list of task as a bulk.
        /// </summary>
        /// <param name="tasks">The task list to submit as a bulk.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>void.</returns>
        public void SubmitTasks(List<QTask> tasks, bool autoCreateResultDisk = true, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                SubmitTasksAsync(tasks, autoCreateResultDisk, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion


        #region RetrieveX
        /// <summary>
        /// Retrieve the list of tasks.
        /// </summary>
        /// <param name="summary">Optional token to choose between full tasks and tasks summaries.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public List<QTask> RetrieveTasks(bool summary = true, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTasksAsync(summary, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of tasks filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for task filtering.</param>
        /// <param name="summary">Optional token to choose between full tasks and tasks summaries.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public List<QTask> RetrieveTasksByTags(List<string> tags, bool summary = true, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTasksByTagsAsync(tags, summary, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }


        /// <summary>
        /// Retrieve the list of pools.
        /// </summary>
        /// <param name="summary">Optional token to choose between full tasks and tasks summaries.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public List<QPool> RetrievePools(bool summary = true, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolsAsync(summary, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of pools filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for pool filtering.</param>
        /// <param name="summary">Optional token to choose between full pools and pools summaries.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public List<QPool> RetrievePoolsByTags(List<string> tags, bool summary = true, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolsByTagsAsync(tags, summary, cancellationToken).Result;
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
        /// Retrieve the bucket
        /// </summary>
        /// <param name="bucketName">Unique name of the bucket to retrieve.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <remarks>If the bucket is not found, null is return</remarks>
        /// <returns>Corresponding bucket.</returns>
        public QBucket RetrieveBucket(string bucketName, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveBucketAsync(bucketName, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the bucket, if it does not exist, create it
        /// </summary>
        /// <param name="bucketName">Unique name of the bucket to retrieve.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>Corresponding bucket.</returns>
        public QBucket RetrieveOrCreateBucket(string bucketName, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveOrCreateBucketAsync(bucketName, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the buckets list with each bucket file count and used space.
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
        /// Retrieve the buckets list.
        /// </summary>
        /// <param name="retrieveBucketStats">If set to true, the file count and used space of each bucket is also retrieved. If set to false, it is faster but only the bucket names are returned.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of buckets.</returns>
        public List<QBucket> RetrieveBuckets(bool retrieveBucketStats, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveBucketsAsync(retrieveBucketStats, cancellationToken).Result;
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
        /// Retrieve the user quotas and disks information for your account.
        /// Note: BucketCount field is retrieved with a second request to the bucket Api.
        /// </summary>
        /// <param name="retrieveBucketCount">If set to false, the BucketCount field is not filled but the request is faster.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and disks information without BucketCount.</returns>
        public UserInformation RetrieveUserInformation(bool retrieveBucketCount, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveUserInformationAsync(retrieveBucketCount, cancellationToken).Result;
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
        /// Retrieve a task by its uuid or shortname(unique and dns compliant).
        /// </summary>
        /// <param name="uuid">uuid or shortname of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that uuid or null if it hasn't been found.</returns>
        public QTask RetrieveTaskByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskByUuidAsync(uuid, cancellationToken).Result;
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
        /// Retrieve a pool by its uuid or shortname(unique and dns compliant).
        /// </summary>
        /// <param name="uuid">uuid or shortname of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that uuid or null if it hasn't been found.</returns>
        public QPool RetrievePoolByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolByUuidAsync(uuid, cancellationToken).Result;
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

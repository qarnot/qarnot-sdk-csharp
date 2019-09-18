using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace QarnotSDK {
    public partial class Connection {
        #region CreateX
        /// <summary>
        /// Create a new bucket.
        /// </summary>
        /// <param name="name">The name of the bucket.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns>A new Bucket.</returns>
        public virtual QBucket CreateBucket(string name, CancellationToken ct=default(CancellationToken)) {
            try {
                return CreateBucketAsync(name, ct).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Submit a list of task as a bulk.
        /// </summary>
        /// <param name="tasks">The task list to submit as a bulk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>void.</returns>
        public virtual void SubmitTasks(List<QTask> tasks, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                SubmitTasksAsync(tasks, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion


        #region RetrieveX
        /// <summary>
        /// Retrieve the tasks list. (deprecated)
        /// </summary>
        /// <param name="summary">Obsolete params to get a summary version of a task.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual List<QTask> RetrieveTasks(bool summary, CancellationToken cancellationToken = default(CancellationToken))
            => RetrieveTasks(cancellationToken);

        /// <summary>
        /// Retrieve the list of tasks.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual List<QTask> RetrieveTasks(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTasksAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of tasks summary.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual List<QTaskSummary> RetrieveTaskSummaries(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskSummariesAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the tasks list with custom filtering.
        /// </summary>
        /// <param name="level">the qtask filter object</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual List<QTask> RetrieveTasks(QDataDetail<QTask> level, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTasksAsync(level, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of tasks filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for task filtering.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual List<QTask> RetrieveTasksByTags(List<string> tags, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTasksByTagsAsync(tags, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of tasks summary filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for task filtering.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks summary.</returns>
        public virtual List<QTaskSummary> RetrieveTaskSummariesByTags(List<string> tags, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskSummariesByTagsAsync(tags, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the pools list. (deprecated)
        /// </summary>
        /// <param name="summary">Obsolete params to get a summary version of a pool.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual List<QPool> RetrievePools(bool summary, CancellationToken cancellationToken = default(CancellationToken))
            => RetrievePools(cancellationToken);

        /// <summary>
        /// Retrieve the list of pools.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual List<QPool> RetrievePools(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolsAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of pools summary.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools summary.</returns>
        public virtual List<QPoolSummary> RetrievePoolSummaries(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolSummariesAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the pools list with custom filtering.
        /// </summary>
        /// <param name="level">the qpool filter object</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual List<QPool> RetrievePools(QDataDetail<QPool> level, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolsAsync(level, cancellationToken).Result;
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
        public virtual List<QPool> RetrievePoolsByTags(List<string> tags, bool summary = true, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolsByTagsAsync(tags, summary, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the list of jobs.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of jobs.</returns>
        public virtual List<QJob> RetrieveJobs(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveJobsAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the jobs list with custom filtering.
        /// </summary>
        /// <param name="level">the sjob filter object</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of jobs.</returns>
        public virtual List<QJob> RetrieveJobs(QDataDetail<QJob> level, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveJobsAsync(level, cancellationToken).Result;
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
        public virtual QBucket RetrieveBucket(string bucketName, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual QBucket RetrieveOrCreateBucket(string bucketName, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual List<QBucket> RetrieveBuckets(CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual List<QBucket> RetrieveBuckets(bool retrieveBucketStats, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveBucketsAsync(retrieveBucketStats, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the user quotas and buckets information for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and buckets information.</returns>
        public virtual UserInformation RetrieveUserInformation(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveUserInformationAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve the user quotas and buckets information for your account.
        /// Note: BucketCount field is retrieved with a second request to the bucket Api.
        /// </summary>
        /// <param name="retrieveBucketCount">If set to false, the BucketCount field is not filled but the request is faster.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and buckets information without BucketCount.</returns>
        public virtual UserInformation RetrieveUserInformation(bool retrieveBucketCount, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual List<string> RetrieveProfiles(CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual List<Constant> RetrieveConstants(string profile, CancellationToken cancellationToken = default(CancellationToken)) {
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
        public virtual QTask RetrieveTaskByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskByNameAsync(name, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a task summary by its name.
        /// </summary>
        /// <param name="name">Name of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task summary object for that name or null if it hasn't been found.</returns>
        public virtual QTaskSummary RetrieveTaskSummaryByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskSummaryByNameAsync(name, cancellationToken).Result;
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
        public virtual QTask RetrieveTaskByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskByUuidAsync(uuid, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a task summary by its uuid.
        /// </summary>
        /// <param name="uuid">uuid of the task summary to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task summary object for that uuid or null if it hasn't been found.</returns>
        public virtual QTaskSummary RetrieveTaskSummaryByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveTaskSummaryByUuidAsync(uuid, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a job by its uuid or shortname.
        /// </summary>
        /// <param name="uuid">uuid or shortname of the job to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The job object for that uuid or null if it hasn't been found.</returns>
        public virtual QJob RetrieveJobByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrieveJobByUuidAsync(uuid, cancellationToken).Result;
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
        public virtual QPool RetrievePoolByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolByNameAsync(name, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a pool summary by its name.
        /// </summary>
        /// <param name="name">Name of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool summary object for that name or null if it hasn't been found.</returns>
        public virtual QPoolSummary RetrievePoolSummaryByName(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolSummaryByNameAsync(name, cancellationToken).Result;
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
        public virtual QPool RetrievePoolByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolByUuidAsync(uuid, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Retrieve a pool summary by its uuid or shortname.
        /// </summary>
        /// <param name="uuid">uuid or shortname of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool summary object for that uuid or null if it hasn't been found.</returns>
        public virtual QPoolSummary RetrievePoolSummaryByUuid(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return RetrievePoolSummaryByUuidAsync(uuid, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}

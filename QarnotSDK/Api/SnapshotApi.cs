
using System;

namespace QarnotSDK
{
    /// <summary>
    /// Represents the Snapshot objects send to te API.
    /// </summary>
    internal class SnapshotConfigurationApi
    {
        public string Whitelist { get; set; }
        public string Blacklist { get; set; }
        public string Bucket { get; set; }
        public string BucketPrefix { get; set; }
    }

    internal class PeriodicSnapshotConfiguration : SnapshotConfigurationApi
    {
        public override string ToString()
        {
            return string.Format("[Whitelist: {0}, Blacklist: {1}, Bucket: {2}, BucketPrefix: {3}, Interval: {4}]", Whitelist, Blacklist, Bucket, BucketPrefix, Interval);
        }

        public int Interval { get; set; }

        internal PeriodicSnapshotConfiguration()
        {
        }
    }

    internal class UniqueSnapshot : SnapshotConfigurationApi
    {
        public override string ToString()
        {
            return string.Format("[Whitelist: {0}, Blacklist: {1}, Bucket:{2}, BucketPrefix{3}]", Whitelist, Blacklist, Bucket, BucketPrefix);
        }

        internal UniqueSnapshot()
        {
        }
    }

    /// <summary>
    /// Snapshot current status.
    /// </summary>
    public enum SnapshotStatusApi
    {
        /// <summary>
        /// Snapshot has been triggered. In waiting of snapshot new information.
        /// </summary>
        Triggered,

        /// <summary>
        /// Snapshot is in Progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Snapshot complete with success.
        /// </summary>
        Success,

        /// <summary>
        /// At least one instance throw an error during the snapshot upload.
        /// </summary>
        Failure
    }
    
    internal class SnapshotApi
    {
        /// <summary>
        /// The snapshot Id.
        /// Construct like : snap_{task_uuid}_{date}_{4_random_chars}
        /// </summary>
        /// <example>snap_52c10b2d-0687-41e1-985e-7279f6dd543a_20251228234559</example>
        public string Id { get; set; }

        /// <summary>
        /// The task uuid corresponding to this snapshot.
        /// </summary>
        /// <example>52c10b2d-0687-41e1-985e-7279f6dd543a</example>
        public Guid TaskUuid { get; set; }

        /// <summary>
        /// The date when this snapshot has been triggered.
        /// </summary>
        public DateTime TriggerDate { get; set;}

        /// <summary>
        /// Last update time.
        /// <br/> Null : the snapshot information has never been updated.
        /// </summary>
        public DateTime? LastUpdateDate { get; set;}

        /// <summary>
        /// Snapshot configuration
        /// Include whitelist and blacklist filter, with outside bucket setup.
        /// </summary>
        public SnapshotConfigurationApi SnapshotConfig { get; set; }

        /// <summary>
        /// Status of the snapshot.
        /// </summary>
        /// <example>InProgress</example>
        public SnapshotStatusApi Status { get; set; }

        /// <summary>
        /// Total count of bytes to upload.
        /// <br/> Null : the snapshot information has never been updated.
        /// </summary>
        /// <example>100</example>
        public long? SizeToUpload { get; set; }

        /// <summary>
        /// Current count of bytes already uploaded.
        /// <br/> Null : the snapshot information has never been updated.
        /// </summary>
        /// <example>50</example>
        public long? TransferredSize { get; set; }
    }

}

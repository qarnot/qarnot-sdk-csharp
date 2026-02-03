using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK.Sdk
{
    /// <summary>
    /// Snapshot current status.
    /// </summary>
    public enum SnapshotStatus
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

    /// <summary>
    /// Carbon facts details of a computed element
    /// </summary>
    public class SnapshotConfiguration
    {
        /// <summary>
        /// Whitelist filter.
        /// </summary>
        /// <example>.*white.*</example>
        public string Whitelist { get; set; }

        /// <summary>
        /// Blacklist filter.
        /// </summary>
        /// <example>.*black.*</example>
        public string Blacklist { get; set; }

        /// <summary>
        /// Bucket name.
        /// </summary>
        /// <example>customBucket</example>
        public string Bucket { get; set; }

        /// <summary>
        /// Bucket prefix.
        /// </summary>
        /// <example>prefix-</example>
        public string BucketPrefix { get; set; }

        internal SnapshotConfiguration(SnapshotConfigurationApi snapshotConfigApi)
        {
            Whitelist = snapshotConfigApi.Whitelist;
            Blacklist = snapshotConfigApi.Blacklist;
            Bucket = snapshotConfigApi.Bucket;
            BucketPrefix = snapshotConfigApi.BucketPrefix;
        }

        /// <summary>
        /// Override of the string representation of the Snapshot configuration
        /// </summary>
        public override string ToString()
        {
            return string.Format("[SnapshotConfiguration, Whitelist={0}, Blacklist={1}, Bucket={2}, BucketPrefix={3}]",
                    Whitelist, 
                    Blacklist,
                    Bucket,
                    BucketPrefix
                    );
        }
    }

    /// <summary>
    /// The representation of an instant snapshot with its status
    /// </summary>
    [Serializable]
    public class Snapshot
    {
        /// <summary>
        /// The snapshot Id.
        /// Construct like : snap_{task_uuid}_{date}_{4_random_chars}
        /// </summary>
        /// <example>snap_52c10b2d-0687-41e1-985e-7279f6dd543a_20251228234559</example>
        public string Id => _snapshotApi.Id;

        /// <summary>
        /// The task uuid corresponding to this snapshot.
        /// </summary>
        /// <example>52c10b2d-0687-41e1-985e-7279f6dd543a</example>
        public Guid TaskUuid => _snapshotApi.TaskUuid;

        /// <summary>
        /// The date when this snapshot has been triggered.
        /// </summary>
        public DateTime TriggerDate => _snapshotApi.TriggerDate;

        /// <summary>
        /// Last update time.
        /// <br/> Null : the snapshot information has never been updated.
        /// </summary>
        public DateTime? LastUpdateDate => _snapshotApi.LastUpdateDate;

        /// <summary>
        /// Snapshot configuration
        /// Include whitelist and blacklist filter, with outside bucket setup.
        /// </summary>
        public SnapshotConfiguration SnapshotConfig => new SnapshotConfiguration(_snapshotApi.SnapshotConfig);

        /// <summary>
        /// Status of the snapshot.
        /// </summary>
        /// <example>InProgress</example>
        public SnapshotStatus Status => (SnapshotStatus) Enum.Parse(typeof(SnapshotStatus), _snapshotApi.Status.ToString());

        /// <summary>
        /// Total count of bytes to upload.
        /// <br/> Null : the snapshot information has never been updated.
        /// </summary>
        /// <example>100</example>
        public long? SizeToUpload => _snapshotApi.SizeToUpload;

        /// <summary>
        /// Current count of bytes already uploaded.
        /// <br/> Null : the snapshot information has never been updated.
        /// </summary>
        /// <example>50</example>
        public long? TransferredSize => _snapshotApi.TransferredSize;

        /// <summary>
        /// The inner Connection object.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public  virtual Connection Connection { get { return _api; } }

        /// <summary>
        /// Reference to the api connection.
        /// </summary>
        protected Connection _api;

        /// <summary>
        /// The task resource uri.
        /// </summary>
        protected string _uri = null;

        internal SnapshotApi _snapshotApi { get; set; }

        internal Snapshot(Connection qapi, string taskUri, SnapshotApi snapshotApi)
        {
            _api = qapi;
            _uri = taskUri + "/snapshot/" + snapshotApi.Id;
            _snapshotApi = snapshotApi;
        }

        internal async Task UpdateStatusAsync(CancellationToken cancellationToken = default)
        {
            using (var response = await _api._client.GetAsync(_uri, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response, cancellationToken);
                var snapshotApi = await response.Content.ReadAsAsync<SnapshotApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                _snapshotApi = snapshotApi;
            }
        }

        /// <summary>
        /// Whether the snapshot is completed
        /// </summary>
        /// <returns></returns>
        public bool IsCompleted() => Status == SnapshotStatus.Success || Status == SnapshotStatus.Failure;

        /// <summary>
        /// Wait for snasphot completion.
        /// </summary>
        /// <param name="timeoutSeconds">Optional maximum number of second to wait for completion.</param>
        /// <param name="updateIntervalSeconds">Optional time in second between two updates. Defualt is 10s.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns>true if the snapshot is completed</returns>
        public virtual async Task<bool> WaitCompletionAsync(int timeoutSeconds=-1, int updateIntervalSeconds=10, CancellationToken ct =default(CancellationToken)) {
            double period = TimeSpan.FromSeconds(updateIntervalSeconds).TotalMilliseconds;
            double sleepingTimeMs;
            var start = DateTime.Now;
            while (!IsCompleted()) {
                await UpdateStatusAsync(ct);
                var elasped = (DateTime.Now - start).Seconds;

                // loop timeout exit condition
                if(timeoutSeconds > 0 && elasped > timeoutSeconds) return false;

                // loop delay
                if(timeoutSeconds > 0)
                    sleepingTimeMs = Math.Min(period, TimeSpan.FromSeconds(timeoutSeconds - elasped).TotalMilliseconds);
                else
                    sleepingTimeMs = period;
                await Task.Delay(TimeSpan.FromMilliseconds(sleepingTimeMs), ct);
            }
            return true;
        }

        /// <summary>
        /// Override of the string representation of the Snapshot
        /// </summary>
        public override string ToString()
        {
            return string.Format("[Snapshot, Id={0}, TaskUuid={1}, TriggerDate={2}, LastUpdateDate={3}, SnapshotConfiguration={4}, SnapshotStatus={5}, SizeToUpload={6}, TransferredSize={7}]",
                Id,
                TaskUuid.ToString(),
                TriggerDate.ToString(),
                LastUpdateDate.ToString(),
                SnapshotConfig.ToString(),
                nameof(Status),
                SizeToUpload.ToString(),
                TransferredSize.ToString()
            );
        }
    }
}
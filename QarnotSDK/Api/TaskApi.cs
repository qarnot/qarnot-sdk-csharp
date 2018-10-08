using System;
using System.Collections.Generic;

namespace QarnotSDK {

    /// <summary>
    /// Represents an error that occur during a task execution.
    /// </summary>
    public class QBulkTaskResponse {
        /// <summary>
        /// Task unique Guid
        /// </summary>
        public Guid Uuid { get; set; }
        /// <summary>
        /// Http Status Code describing the success of the resource creation(Task)
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// Human error message.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Property to ensure that the submission succeeded.
        /// (Ensure task guid is set and correct status code.)
        /// </summary>
        public bool IsSuccesResponse {
            get { return Uuid != default(Guid) && ((int)StatusCode >= 200) && ((int)StatusCode <= 299); }
        }

        internal QBulkTaskResponse() {
        }

        /// <summary>
        /// Returns all the information about this task submission response.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return String.Format("[{0}] {1} ({2})", StatusCode, Uuid, Message);
        }
    }
    /// <summary>
    /// Represents an error that occur during a task execution.
    /// </summary>
    public class QTaskError {
        /// <summary>
        /// Qarnot error code.
        /// Note: error code descriptions are available here:
        ///  https://computing.qarnot.com/developers/develop/common-errors
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Human error message.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Debug code to send to Qarnot support team.
        /// </summary>
        public string Debug { get; set; }

        internal QTaskError() {
        }
        /// <summary>
        /// Returns all the information about this error in one string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if (String.IsNullOrEmpty(Debug))
                return String.Format("[{0}] {1}", Code, Message);
            else
                return String.Format("[{0}] {1} ({2})", Code, Message, Debug);
        }
    }

    /// <summary>
    /// Represents a public TCP port of a task.
    /// </summary>
    public class QTaskStatusActiveForwards {
        /// <summary>
        /// Port of the application running on the compute node.
        /// </summary>
        public UInt16 ApplicationPort { get; set; }
        /// <summary>
        /// Port where this application can be reached on the public host.
        /// </summary>
        public UInt16 ForwarderPort { get; set; }
        /// <summary>
        /// Public host where this application can be reached.
        /// </summary>
        public string ForwarderHost { get; set; }

        internal QTaskStatusActiveForwards() {
        }
    }

    /// <summary>
    /// Represents the status and the statistics of a running task instance.
    /// </summary>
    public class QTaskStatusPerRunningInstanceInfo {
        /// <summary>
        /// Phase.
        /// </summary>
        public string Phase { get; set; }
        /// <summary>
        /// Instance Id.
        /// </summary>
        public UInt32 InstanceId { get; set; }
        /// <summary>
        /// Maximum frequency in Ghz.
        /// </summary>
        public float MaxFrequencyGHz { get; set; }
        /// <summary>
        /// Current frequency in Ghz.
        /// </summary>
        public float CurrentFrequencyGHz { get; set; }
        /// <summary>
        /// Cpu usage in percentage (0 to 100).
        /// </summary>
        public float CpuUsage { get; set; }
        /// <summary>
        /// Maximum memory in MB.
        /// </summary>
        public float MaxMemoryMB { get; set; }
        /// <summary>
        /// Current memory usage in MB.
        /// </summary>
        public float CurrentMemoryMB { get; set; }
        /// <summary>
        /// Inbound network traffic in Kbps.
        /// </summary>
        public float NetworkInKbps { get; set; }
        /// <summary>
        /// Outbound network traffic in Kbps.
        /// </summary>
        public float NetworkOutKbps { get; set; }
        /// <summary>
        /// Progress.
        /// </summary>
        public float Progress { get; set; }
        /// <summary>
        /// Execution time in seconds.
        /// </summary>
        public float ExecutionTimeSec { get; set; }
        /// <summary>
        /// Virtual execution time Ghz.
        /// Note:
        ///  - An execution time of 10 seconds @4Ghz will return here 40 seconds.
        ///  - An execution time of 10 seconds @0.5Ghz will return here 5 seconds.
        /// </summary>
        public float ExecutionTimeGHz { get; set; }
        /// <summary>
        /// Processor model.
        /// </summary>
        public string CpuModel { get; set; }
        /// <summary>
        /// Memory usage in percentage (0.0f to 1.0f).
        /// </summary>
        public float MemoryUsage { get; set; }
        /// <summary>
        /// Inbound ports forwarding information.
        /// </summary>
        public List<QTaskStatusActiveForwards> ActiveForwards { get; set; }

        internal QTaskStatusPerRunningInstanceInfo() {
            ActiveForwards = new List<QTaskStatusActiveForwards>();
        }
    }

    /// <summary>
    /// Represents the statistics of a running task.
    /// </summary>
    public class QTaskStatusRunningInstancesInfo {
        public DateTime Timestamp { get; set; }
        public float AverageFrequencyGHz { get; set; }
        public float MaxFrequencyGHz { get; set; }
        public float MinFrequencyGHz { get; set; }
        public float AverageMaxFrequencyGHz { get; set; }
        public float AverageCpuUsage { get; set; }
        public float ClusterPowerIndicator { get; set; }
        public float AverageMemoryUsage { get; set; }
        public float AverageNetworkInKbps { get; set; }
        public float AverageNetworkOutKbps { get; set; }
        public float TotalNetworkInKbps { get; set; }
        public float TotalNetworkOutKbps { get; set; }
        public List<QTaskStatusPerRunningInstanceInfo> PerRunningInstanceInfo { get; set; }

        internal QTaskStatusRunningInstancesInfo() {
            PerRunningInstanceInfo = new List<QTaskStatusPerRunningInstanceInfo>();
        }
    }

    /// <summary>
    /// Represents the status of a running task.
    /// </summary>
    public class QTaskStatus {
        public float DownloadProgress { get; set; }
        public float ExecutionProgress { get; set; }
        public float UploadProgress { get; set; }
        public uint InstanceCount { get; set; }
        public long DownloadTimeSec { get; set; }
        public long ExecutionTimeSec { get; set; }
        public long UploadTimeSec { get; set; }
        public string SucceededRange { get; set; }
        public string ExecutedRange { get; set; }
        public string FailedRange { get; set; }
        public QTaskStatusRunningInstancesInfo RunningInstancesInfo { get; set; }

        internal QTaskStatus() {
        }
    }

    /// <summary>
    /// Represents the status and the statistics of a completed task instance.
    /// </summary>
    public class QTaskCompletedInstance {
        public UInt32 InstanceId { get; set; }
        public float WallTimeSec { get; set; }
        public float ExecTimeSec { get; set; }
        public float ExecTimeSecGhz { get; set; }
        public float PeakMemoryMB { get; set; }
        public string State { get; set; }
        public QTaskError Error { get; set; }
        public float AverageGhz { get; set; }

        internal QTaskCompletedInstance() {
        }
    }

    internal class TaskApi {
        public override string ToString() {
            return string.Format("[TaskApi: Name={0}, Profile={1}, InstanceCount={2}, ResultDisk={3}, State={4}, SnapshotInterval={5}, ResultsCount={6}, CreationDate={7}, Uuid={8}]", Name, Profile, InstanceCount, ResultDisk, State, SnapshotInterval, ResultsCount, CreationDate, Uuid);
        }

        public string Name { get; set; }
        public string Profile { get; set; }
        public string PoolUuid { get; set; }
        public uint InstanceCount { get; set; }
        public List<string> ResourceDisks { get; set; }
        public List<string> ResourceBuckets { get; set; }
        public string ResultDisk { get; set; }
        public string ResultBucket { get; set; }
        public string State { get; set; }
        public List<QTaskError> Errors { get; set; }
        public int SnapshotInterval { get; set; }
        public uint ResultsCount { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public List<String> Tags { get; set; }
        public Guid Uuid { get; set; }
        public string Shortname { get; set; }
        public QTaskStatus Status { get; set; }
        public string AdvancedRanges { get; set; }
        public List<QTaskCompletedInstance> CompletedInstances { get; set; }
        public string SnapshotWhitelist { get; set; }
        public string SnapshotBlacklist { get; set; }
        public string ResultsWhitelist { get; set; }
        public string ResultsBlacklist { get; set; }

        internal TaskApi() {
            Constants = new List<KeyValHelper>();
            Tags = new List<String>();
            ResourceDisks = new List<String>();
            ResourceBuckets = new List<String>();
            Errors = new List<QTaskError>();
            CompletedInstances = new List<QTaskCompletedInstance>();
        }
    }

    internal class Snapshot {
        public int Interval { get; set; }
        internal Snapshot() {
        }
    }
}

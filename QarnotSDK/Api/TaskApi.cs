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
        public Guid? Uuid { get; set; }
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
    /// Represents the execution time by cpu infos.
    /// </summary>
    public class QTaskStatusExecutionTimeByCpuModel {
        /// <summary>
        /// CPU name and Model.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// CPU time used in seconds.
        /// </summary>
        public ulong Time { get; set; }
        /// <summary>
        /// CPU core numbers.
        /// </summary>
        public uint Core { get; set; }

        internal QTaskStatusExecutionTimeByCpuModel() {
        }
    }

    /// <summary>
    /// Represents the execution cpu ratio for each task.
    /// </summary>
    public class QTaskStatusExecutionTimeGhzByCpuModel
    {
        /// <summary>
        /// CPU name and Model.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// CPU gigahertz time used in seconds.
        /// </summary>
        public double TimeGhz { get; set; }
        /// <summary>
        /// CPU core numbers.
        /// </summary>
        public uint Core { get; set; }

        /// <summary>
        /// CPU clock ratio.
        /// </summary>
        public double ClockRatio { get; set; }

        internal QTaskStatusExecutionTimeGhzByCpuModel()
        {
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

        /// <summary>
        /// Time at which this information has been reported
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Average running instance frequency (in Ghz).
        /// </summary>
        public float AverageFrequencyGHz { get; set; }

        /// <summary>
        /// Maximum running instance frequency (in Ghz).
        /// </summary>
        public float MaxFrequencyGHz { get; set; }

        /// <summary>
        /// Minimum running instance frequency (in Ghz).
        /// </summary>
        public float MinFrequencyGHz { get; set; }

        /// <summary>
        /// Average running instance frequency (in Ghz).
        /// </summary>
        public float AverageMaxFrequencyGHz { get; set; }

        /// <summary>
        /// Average running instance cpu usage.
        /// </summary>
        public float AverageCpuUsage { get; set; }

        /// <summary>
        /// Average running instance power indicator.
        /// The power indicator shows a ratio between actual frequency and maximum frequenc 
        /// </summary>
        public float ClusterPowerIndicator { get; set; }

        /// <summary>
        /// Average running instance memory usage
        /// </summary>
        public float AverageMemoryUsage { get; set; }

        /// <summary>
        /// Average running instance network in (in kbps)
        /// </summary>
        public float AverageNetworkInKbps { get; set; }

        /// <summary>
        /// Average running instance network out (in kbps)
        /// </summary>
        public float AverageNetworkOutKbps { get; set; }

        /// <summary>
        /// Total running instance network in (in kbps)
        /// </summary>
        public float TotalNetworkInKbps { get; set; }

        /// <summary>
        /// Total running instance network out (in kbps)
        /// </summary>
        public float TotalNetworkOutKbps { get; set; }

        /// <summary>
        /// Running instance information (see QTaskStatusPerRunningInstanceInfo)
        /// </summary>
        public List<QTaskStatusPerRunningInstanceInfo> PerRunningInstanceInfo { get; set; }

        internal QTaskStatusRunningInstancesInfo() {
            PerRunningInstanceInfo = new List<QTaskStatusPerRunningInstanceInfo>();
        }
    }

    /// <summary>
    /// Represents the status of a running task.
    /// </summary>
    public class QTaskStatus {
        /// <summary>
        /// Retrieve the instance download progress indicator
        /// </summary>
        public float DownloadProgress { get; set; }

        /// <summary>
        /// Retrieve the instance execution progress indicator
        /// </summary>
        public float ExecutionProgress { get; set; }

        /// <summary>
        /// Retrieve the instance execution upload indicator
        /// </summary>
        public float UploadProgress { get; set; }

        /// <summary>
        /// The task instance number.
        /// </summary>
        public uint InstanceCount { get; set; }

        /// <summary>
        /// Download time (in second)
        /// </summary>
        public long DownloadTimeSec { get; set; }

        /// <summary>
        /// Execution time (in second)
        /// </summary>
        public long ExecutionTimeSec { get; set; }

        /// <summary>
        /// Upload time (in second)
        /// </summary>
        public long UploadTimeSec { get; set; }

        /// <summary>
        /// Range for the succeeded instances
        /// </summary>
        public string SucceededRange { get; set; }

        /// <summary>
        /// Range for the executed instances
        /// </summary>
        public string ExecutedRange { get; set; }

        /// <summary>
        /// Range for the failed instances
        /// </summary>
        public string FailedRange { get; set; }

        /// <summary>
        /// Running instances information(see QTaskStatusRunningInstancesInfo)
        /// </summary>
        public QTaskStatusRunningInstancesInfo RunningInstancesInfo { get; set; }

        /// <summary>
        /// Execution cpu times for each Running instance 
        /// </summary>
        public List<QTaskStatusExecutionTimeByCpuModel> ExecutionTimeByCpuModel {get; set ;}

        /// <summary>
        /// Execution cpu ratio for each Running instance 
        /// </summary>
        public List<QTaskStatusExecutionTimeGhzByCpuModel> ExecutionTimeGhzByCpuModel {get; set ;}

        internal QTaskStatus() {
            ExecutionTimeByCpuModel = new List<QTaskStatusExecutionTimeByCpuModel>();
            ExecutionTimeGhzByCpuModel = new List<QTaskStatusExecutionTimeGhzByCpuModel>();
        }
    }

    /// <summary>
    /// Represents the status and the statistics of a completed task instance.
    /// </summary>
    public class QTaskCompletedInstance {

        /// <summary>
        /// The instance id.
        /// </summary>
        public UInt32 InstanceId { get; set; }

        /// <summary>
        /// Instance wall time (in second)
        /// </summary>
        public float WallTimeSec { get; set; }

        /// <summary>
        /// Instance execution time (in second)
        /// </summary>
        public float ExecTimeSec { get; set; }

        /// <summary>
        /// Instance execution time frequency
        /// </summary>
        public float ExecTimeSecGhz { get; set; }

        /// <summary>
        /// Completed instance memory peak (in Mb)
        /// </summary>
        public float PeakMemoryMB { get; set; }

        /// <summary>
        /// Completed instance state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Completed instance error
        /// </summary>
        public QTaskError Error { get; set; }

        /// <summary>
        /// Completed instance average frequency (in Ghz)
        /// </summary>
        public float AverageGhz { get; set; }

        internal QTaskCompletedInstance() {
        }
    }

    internal class TaskApi {
        public override string ToString() {
            return string.Format("[TaskApi: Name={0}, Profile={1}, InstanceCount={2}, State={3}, SnapshotInterval={4}, CreationDate={5}, Uuid={6}]", Name, Profile, InstanceCount, State, SnapshotInterval, CreationDate, Uuid);
        }

        public string Name { get; set; }
        public string Profile { get; set; }
        public string PoolUuid { get; set; }
        public string JobUuid { get; set; }
        public uint InstanceCount { get; set; }
        public List<string> ResourceBuckets { get; set; }
        public string ResultBucket { get; set; }
        public string State { get; set; }
        public List<QTaskError> Errors { get; set; }
        public int SnapshotInterval { get; set; }
        public uint ResultsCount { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public List<KeyValHelper> Constraints { get; set; }
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
        public Dependency Dependencies { get; set; }

        internal TaskApi() {
            Constants = new List<KeyValHelper>();
            Constraints = new List<KeyValHelper>();
            Tags = new List<String>();
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

    internal class Dependency
    {
        public List<Guid> DependsOn { get; set; }
        internal Dependency()
        {
            DependsOn = new List<Guid>();
        }
    }
}

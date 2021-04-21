using System;
using System.Collections.Generic;

namespace QarnotSDK {
    /// <summary>
    /// Represents an error that occur during a pool execution.
    /// </summary>
    public class QPoolError {
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

        internal QPoolError() {
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
    /// Represents a public TCP port of a pool.
    /// </summary>
    public class QPoolStatusActiveForwards {
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

        internal QPoolStatusActiveForwards() {
        }
    }

    /// <summary>
    /// Represents the pool elastic properties
    /// </summary>
    internal class QPoolElasticProperty {

        public const bool DEFAULT_IS_ELASTIC = false;
        public const uint DEFAULT_MIN_TOTAL_SLOTS = 0;
        public const uint DEFAULT_MAX_TOTAL_SLOTS = 0;
        public const uint DEFAULT_MIN_IDLING_SLOTS = 0;
        public const uint DEFAULT_RESIZE_PERIOD = 90;
        public const float DEFAULT_RAMP_RESIZE_FACTOR = 0.4f;
        public const uint DEFAULT_MIN_IDLING_TIME = 90;

        /// <summary>
        /// if the elastic behaviour is activated
        /// </summary>
        public bool IsElastic { get; set; } = DEFAULT_IS_ELASTIC;

        /// <summary>
        /// Lower bound of slots number
        /// </summary>
        public uint MinTotalSlots { get; set; } = DEFAULT_MIN_TOTAL_SLOTS;

        /// <summary>
        /// Upper bound of slots number
        /// </summary>
        public uint MaxTotalSlots { get; set; } = DEFAULT_MAX_TOTAL_SLOTS;

        /// <summary>
        /// Keep a number of slot doing nothing, but up and waiting
        /// </summary>
        public uint MinIdleSlots { get; set; } = DEFAULT_MIN_IDLING_SLOTS;

        /// <summary>
        /// Period for time for resizing idling slots (in seconds)
        /// </summary>
        /// <remarks> ResizePeriod minimum is 90 secondes </remarks>
        public uint ResizePeriod { get; set; } = DEFAULT_RESIZE_PERIOD;

        /// <summary>
        /// In order to close or open progresssively slots.
        /// </summary>
        /// <remarks> RampResizeFactor is in [0:1[ </remarks>
        public float RampResizeFactor { get; set; } = DEFAULT_RAMP_RESIZE_FACTOR;

        /// <summary>
        /// when a slot is empty, wait MinIdleTimeSeconds seconds before allowing the slot to be closed or reused
        /// </summary>
        public uint MinIdleTimeSeconds { get; set; } = DEFAULT_MIN_IDLING_TIME;

        internal QPoolElasticProperty() {}
    }

    /// <summary>
    /// Represents the status and the statistics of a running pool node.
    /// </summary>
    public class QPoolStatusPerRunningInstanceInfo {
        /// <summary>
        /// Phase.
        /// </summary>
        public string Phase { get; set; }
        /// <summary>
        /// Instance Id.
        /// </summary>
        public UInt32 InstanceId { get; set; }
        /// <summary>
        /// Core Count.
        /// </summary>
        public UInt32 CoreCount { get; set; }
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
        /// Memory usage in percent (0.0f to 1.0f).
        /// </summary>
        public float MemoryUsage { get; set; }
        /// <summary>
        /// Inbound ports forwarding information.
        /// </summary>
        public List<QPoolStatusActiveForwards> ActiveForwards { get; set; }

        internal QPoolStatusPerRunningInstanceInfo() {
            ActiveForwards = new List<QPoolStatusActiveForwards>();
        }
    }

    /// <summary>
    /// Represents the execution time by cpu infos.
    /// </summary>
    public class QPoolStatusExecutionTimeByCpuModel
    {
        /// <summary>
        /// CPU name and Model.
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// CPU time used in seconds.
        /// </summary>
        public double Time { get; set; }
        /// <summary>
        /// CPU core numbers.
        /// </summary>
        public uint Core { get; set; }

        internal QPoolStatusExecutionTimeByCpuModel()
        {
        }
    }

    /// <summary>
    /// Represents the execution cpu ratio for each task.
    /// </summary>
    public class QPoolStatusExecutionTimeGhzByCpuModel
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

        internal QPoolStatusExecutionTimeGhzByCpuModel()
        {
        }
    }

    /// <summary>
    /// Represents the statistics of a running pool.
    /// </summary>
    public class QPoolStatusRunningInstancesInfo {
        /// <summary>
        /// Last information update timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Average Frequency in GHz.
        /// </summary>
        public float AverageFrequencyGHz { get; set; }
        /// <summary>
        /// Maximum Frequency in GHz.
        /// </summary>
        public float MaxFrequencyGHz { get; set; }
        /// <summary>
        /// Minimum Frequency in GHz.
        /// </summary>
        public float MinFrequencyGHz { get; set; }
        /// <summary>
        /// Average Maximum Frequency in GHz.
        /// </summary>
        public float AverageMaxFrequencyGHz { get; set; }
        /// <summary>
        /// Average CPU Usage.
        /// </summary>
        public float AverageCpuUsage { get; set; }
        /// <summary>
        /// Cluster Power Indicator.
        /// </summary>
        public float ClusterPowerIndicator { get; set; }
        /// <summary>
        /// Average Memory Usage.
        /// </summary>
        public float AverageMemoryUsage { get; set; }
        /// <summary>
        /// Average Network Input in Kbps.
        /// </summary>
        public float AverageNetworkInKbps { get; set; }
        /// <summary>
        /// Average Network Output in Kbps.
        /// </summary>
        public float AverageNetworkOutKbps { get; set; }
        /// <summary>
        /// Total Network Input in Kbps.
        /// </summary>
        public float TotalNetworkInKbps { get; set; }
        /// <summary>
        /// Total Network Output in Kbps.
        /// </summary>
        public float TotalNetworkOutKbps { get; set; }

        /// <summary>
        /// Total Network Output in Kbps.
        /// </summary>
        public List<QPoolStatusPerRunningInstanceInfo> PerRunningInstanceInfo { get; set; }

        internal QPoolStatusRunningInstancesInfo() {
            PerRunningInstanceInfo = new List<QPoolStatusPerRunningInstanceInfo>();
        }
    }

    /// <summary>
    /// Represents the status of a running pool.
    /// </summary>
    public class QPoolStatus {
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
        /// The pool instance number.
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
        /// Running instances information(see QPoolStatusRunningInstancesInfo)
        /// </summary>
        public QPoolStatusRunningInstancesInfo RunningInstancesInfo { get; set; }

        /// <summary>
        /// Execution cpu times for each Running instance
        /// </summary>
        public List<QPoolStatusExecutionTimeByCpuModel> ExecutionTimeByCpuModel { get; set; }

        /// <summary>
        /// Execution cpu ratio for each Running instance
        /// </summary>
        public List<QPoolStatusExecutionTimeGhzByCpuModel> ExecutionTimeGhzByCpuModel { get; set; }

        internal QPoolStatus() {
            ExecutionTimeByCpuModel = new List<QPoolStatusExecutionTimeByCpuModel>();
            ExecutionTimeGhzByCpuModel = new List<QPoolStatusExecutionTimeGhzByCpuModel>();
        }
    }

    /// <summary>
    /// Represents the preparation commands and environment of each task running by this pool.
    /// </summary>
    public class PoolPreparationTask
    {
        /// <summary>
        /// Command line value.
        /// </summary>
        /// <value></value>
        public string CommandLine { get; set; }

        /// <summary>
        /// Command line constructor.
        /// </summary>
        /// <param name="commandLine">command line to execute before launching a task</param>
        public PoolPreparationTask(string commandLine)
        {
            CommandLine = commandLine;
        }
    }

    internal class PoolApi {
        public override string ToString() {
            return string.Format("[PoolApi: Name={0}, Profile={1}, InstanceCount={2}, State={3}, CreationDate={4}, Uuid={5}]", Name, Profile, InstanceCount, State, CreationDate, Uuid);
        }

        public string Name { get; set; }
        public string Profile { get; set; }
        public uint InstanceCount { get; set; }
        public uint? RunningInstanceCount { get; set; }
        public uint? RunningCoreCount { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public TimeSpan? WallTime { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> ResourceBuckets { get; set; }
        public List<ApiAdvancedResourceBucket> AdvancedResourceBuckets { get; set; }
        public string State { get; set; }
        public string PreviousState { get; set; }
        public DateTime StateTransitionTime { get; set; }
        public DateTime PreviousStateTransitionTime { get; set; }
        public DateTime LastModified { get; set; }
        public List<QPoolError> Errors { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public List<KeyValHelper> Constraints { get; set; }
        public List<String> Tags { get; set; }
        public Guid Uuid { get; set; }
        public string Shortname { get; set; }
        public QPoolStatus Status { get; set; }
        public QPoolElasticProperty ElasticProperty { get; set; }
        public PoolPreparationTask PreparationTask { get; set; }
        public bool AutoDeleteOnCompletion { get; set; }
        public TimeSpan CompletionTimeToLive { get; set; }

        internal PoolApi() {
            Constants = new List<KeyValHelper>();
            Constraints = new List<KeyValHelper>();
            Tags = new List<String>();
            ResourceBuckets = new List<String>();
            AdvancedResourceBuckets = new List<ApiAdvancedResourceBucket>();
            Errors = new List<QPoolError>();
            ElasticProperty = new QPoolElasticProperty();
            AutoDeleteOnCompletion = false;
        }
    }
}

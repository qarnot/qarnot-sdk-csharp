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
        public List<QPoolStatusPerRunningInstanceInfo> PerRunningInstanceInfo { get; set; }

        internal QPoolStatusRunningInstancesInfo() {
            PerRunningInstanceInfo = new List<QPoolStatusPerRunningInstanceInfo>();
        }
    }

    /// <summary>
    /// Represents the status of a running pool.
    /// </summary>
    public class QPoolStatus {
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
        public QPoolStatusRunningInstancesInfo RunningInstancesInfo { get; set; }

        internal QPoolStatus() {
        }
    }

    internal class PoolApi {
        public override string ToString() {
            return string.Format("[PoolApi: Name={0}, Profile={1}, InstanceCount={2}, State={3}, CreationDate={4}, Uuid={5}]", Name, Profile, InstanceCount, State, CreationDate, Uuid);
        }

        public string Name { get; set; }
        public string Profile { get; set; }
        public uint InstanceCount { get; set; }
        public List<string> ResourceDisks { get; set; }
        public List<string> ResourceBuckets { get; set; }
        public string State { get; set; }
        public List<QPoolError> Errors { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public List<KeyValHelper> Constraints { get; set; }
        public List<String> Tags { get; set; }
        public Guid Uuid { get; set; }
        public string Shortname { get; set; }
        public QPoolStatus Status { get; set; }

        internal PoolApi() {
            Constants = new List<KeyValHelper>();
            Constraints = new List<KeyValHelper>();
            Tags = new List<String>();
            ResourceDisks = new List<String>();
            ResourceBuckets = new List<String>();
            Errors = new List<QPoolError>();
        }
    }
}
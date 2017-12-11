using System;
using System.Collections.Generic;

namespace QarnotSDK {
    /// <summary>
    /// Represents an error that occur during a task execution.
    /// </summary>
    public class QTaskError {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Debug { get; set; }
        public QTaskError() {
        }
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
        public UInt16 ApplicationPort { get; set; }
        public UInt16 ForwarderPort { get; set; }
        public string ForwarderHost { get; set; }
        public QTaskStatusActiveForwards() {
        }
    }

    /// <summary>
    /// Represents the status and the statistics of a running task instance.
    /// </summary>
    public class QTaskStatusPerRunningInstanceInfo {
        public string Phase { get; set; }
        public UInt32 InstanceId { get; set; }
        public float MaxFrequencyGHz { get; set; }
        public float CurrentFrequencyGHz { get; set; }
        public float CpuUsage { get; set; }
        public float MaxMemoryMB { get; set; }
        public float CurrentMemoryMB { get; set; }
        public float NetworkInKbps { get; set; }
        public float NetworkOutKbps { get; set; }
        public float Progress { get; set; }
        public float ExecutionTimeSec { get; set; }
        public float ExecutionTimeGHz { get; set; }
        public string CpuModel { get; set; }
        public float MemoryUsage { get; set; }
        public List<QTaskStatusActiveForwards> ActiveForwards { get; set; }

        public QTaskStatusPerRunningInstanceInfo() {
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

        public QTaskStatusRunningInstancesInfo() {
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

        public QTaskStatus() {
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

        public QTaskCompletedInstance() {
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
        public string ResultDisk { get; set; }
        public string State { get; set; }
        public List<QTaskError> Errors { get; set; }
        public int SnapshotInterval { get; set; }
        public uint ResultsCount { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public Guid Uuid { get; set; }
        public string Shortname { get; set; }
        public QTaskStatus Status { get; set; }
        public string AdvancedRanges { get; set; }
        public List<QTaskCompletedInstance> CompletedInstances { get; set; }

        public TaskApi() {
            Constants = new List<KeyValHelper>();
            ResourceDisks = new List<String>();
            Errors = new List<QTaskError>();
            CompletedInstances = new List<QTaskCompletedInstance>();
        }
    }

    internal class Snapshot {
        public int Interval { get; set; }
        public Snapshot() {
        }
    }
}

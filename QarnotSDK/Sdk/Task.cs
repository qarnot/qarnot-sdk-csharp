﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using System;
using QarnotSDK.Sdk;


namespace QarnotSDK {
    /// <summary>
    /// Task states.
    /// </summary>
    public sealed class QTaskStates {
        /// <summary>
        /// The Task has been submitted and is waiting for dispatch.
        /// </summary>
        public static readonly string Submitted             = "Submitted";
        /// <summary>
        /// Some of the instances have been dispatched.
        /// </summary>
        public static readonly string PartiallyDispatched   = "PartiallyDispatched";
        /// <summary>
        /// All the instances have been dispatched.
        /// </summary>
        public static readonly string FullyDispatched       = "FullyDispatched";
        /// <summary>
        /// Some of the instances are running.
        /// </summary>
        public static readonly string PartiallyExecuting    = "PartiallyExecuting";
        /// <summary>
        /// All the instances are running.
        /// </summary>
        public static readonly string FullyExecuting        = "FullyExecuting";
        /// <summary>
        /// The results are being uploaded from the compute nodes to the result bucket.
        /// </summary>
        public static readonly string UploadingResults      = "UploadingResults";
        /// <summary>
        /// The task has been cancelled.
        /// </summary>
        public static readonly string Cancelled             = "Cancelled";
        /// <summary>
        /// The task has completed successfully.
        /// </summary>
        public static readonly string Success               = "Success";
        /// <summary>
        /// At least one instance of the task has failed.
        /// </summary>
        public static readonly string Failure               = "Failure";
    }

    /// <summary>
    /// This class manges tasks life cycle: submission, monitor, delete.
    /// </summary>
    public partial class QTask : AQTask {

        /// <summary>
        /// boolean to register if the task is attached to a job without a pool
        /// </summary>
        private bool _useStandaloneJob = false;
        private AdvancedRanges _advancedRange = null;
        private QPool _pool = null;
        private QJob _job = null;
        /// <summary>
        /// The task shortname identifier. The shortname is provided by the user. It has to be unique.
        /// </summary>
        [InternalDataApiName(Name="Shortname")]
        public virtual string Shortname { get { return _taskApi.Shortname == null ? _taskApi.Uuid.ToString() : _taskApi.Shortname; } }
        /// <summary>
        /// The task name.
        /// </summary>
        [InternalDataApiName(Name="Name")]
        public virtual string Name { get { return _taskApi.Name; } }
        /// <summary>
        /// The task profile.
        /// </summary>
        [InternalDataApiName(Name="Profile")]
        public virtual string Profile { get { return _taskApi.Profile; } }

        /// <summary>
        /// Qarnot resources buckets bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual List<QAbstractStorage> Resources {
            get {
                return _resources;
            }
            set {
                _resources = QBucket.GetBucketsFromResources(value).OfType<QAbstractStorage>().ToList();
            }
        }

        //private List<QBucket> _resources { get; set; }
        private List<QAbstractStorage> _resources { get; set; }

        /// <summary>
        /// Qarnot resources buckets bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        [InternalDataApiName(Name="ResourceBuckets", IsFilterable=false)]
        public virtual IEnumerable<QBucket> ResourcesBuckets {
            get {
                return _resources.OfType<QBucket>();
            }
        }

        /// <summary>
        /// Qarnot result bucket bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual QAbstractStorage Results {
            get {
                return _results;
            }
            set {
                _results = QBucket.GetBucketFromResource(value);
            }
        }

        private QBucket _results { get; set; }

        /// <summary>
        /// Qarnot result bucket bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        [InternalDataApiName(Name="ResultBucket")]
        public virtual QBucket ResultsBucket {
            get {
                return _results;
            }
        }

        /// <summary>
        /// Retrieve the task state (see QTaskStates).
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name="State")]
        public virtual string State { get { return _taskApi?.State; } }

        /// <summary>
        /// Retrieve the task previous state (see QTaskStates).
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name = "PreviousState")]
        public virtual string PreviousState { get { return _taskApi?.PreviousState; } }

        /// <summary>
        /// Retrieve the task state transition utc-time (see QTaskStates).
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name = "StateTransitionTime")]
        public virtual DateTime? StateTransitionTime
        {
            get
            {
                if (_taskApi != null)
                {
                    if (_taskApi.StateTransitionTime != default)
                    {
                        return _taskApi.StateTransitionTime;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Retrieve the task previous state transition utc-time (see QTaskStates).
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name = "PreviousStateTransitionTime")]
        public virtual DateTime? PreviousStateTransitionTime
        {
            get
            {
                if (_taskApi != null)
                {
                    if (_taskApi.PreviousStateTransitionTime != default)
                    {
                        return _taskApi.PreviousStateTransitionTime;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// The Task last modified date.
        /// </summary>
        [InternalDataApiName(Name = "LastModified")]
        public virtual DateTime? LastModified
        {
            get
            {
                if (_taskApi != null)
                {
                    if (_taskApi.LastModified != default)
                    {
                        return _taskApi.LastModified;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Retrieve the task errors.
        /// </summary>
        [InternalDataApiName(Name="Errors", IsFilterable=false)]
        public virtual List<QTaskError> Errors {
            get {
                return _taskApi != null ? _taskApi.Errors : new List<QTaskError>();
            }
        }

        /// <summary>
        /// Retrieve the task detailed status.
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name="Status", IsFilterable=false)]
        public virtual QTaskStatus Status {
            get {
                if (_taskApi == null)
                    return null;
                return _taskApi.Status;
            }
        }

        /// <summary>
        /// The task creation date.
        /// Available only after the submission.
        /// </summary>
        [InternalDataApiName(Name="CreationDate")]
        public virtual DateTime CreationDate { get { return _taskApi.CreationDate; } }

        /// <summary>
        /// Increased each time a new set of results is available, when snapshot or final results are ready.
        /// Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name="ResultsCount")]
        public virtual uint ResultsCount {
            get {
                return _taskApi.ResultsCount;
            }
        }

        /// <summary>
        /// The custom task tag list.
        /// </summary>
        [InternalDataApiName(Name="Tags")]
        public virtual List<String> Tags {
            get {
                return _taskApi.Tags;
            }
        }

        private Dictionary<string, string> _constants { get; set; }

        /// <summary>
        /// The task constants.
        /// </summary>
        /// <returns>return all Constants</returns>
        [InternalDataApiName(Name="Constants")]
        public virtual Dictionary<string, string> Constants {
            get {
                if (_constants == null)
                    _constants = new Dictionary<string, string>();
                return _constants;
            }
        }

        private Dictionary<string, string> _constraints { get; set; }

        /// <summary>
        /// The task constraints.
        /// </summary>
        /// <returns>return all Constraints</returns>
        [InternalDataApiName(Name="Constraints")]
        public virtual Dictionary<string, string> Constraints {
            get {
                if (_constraints == null)
                    _constraints = new Dictionary<string, string>();
                return _constraints;
            }
        }

        /// <summary>
        /// The task labels.
        /// </summary>
        /// <returns>return all Labels</returns>
        [InternalDataApiName(Name="Labels")]
        public virtual Dictionary<string, string> Labels {
            get {
                if (_taskApi.Labels == null) {
                    _taskApi.Labels = new Dictionary<string, string>();
                }
                return _taskApi.Labels;
            }
        }

        /// <summary>
        /// The Task Dependencies
	/// Guid list of tasks to wait before running
        /// The task need to be in a job with depencendies activated
        /// </summary>
        [InternalDataApiName(Name="Dependencies.DependsOn")]
        public virtual List<Guid> DependsOn {
            get
            {
                if (_taskApi.Dependencies == null || _taskApi.Dependencies.DependsOn.IsNullOrEmpty())
                    return new List<Guid>();
                return _taskApi.Dependencies.DependsOn.ToList();
            }
            set
            {
                if (_taskApi.Dependencies == null)
                    _taskApi.Dependencies = new Dependency();
                _taskApi.Dependencies.DependsOn = value.ToList();
            }
        }

        /// <summary>
        /// The delay in seconds between two periodic snapshots.
        /// Once the task is running, use the SnapshotPeriodic method to update.
        /// </summary>
        [InternalDataApiName(Name="SnapshotInterval")]
        public virtual int SnapshotInterval {
            get {
                return _taskApi.SnapshotInterval;
            }
        }

        /// <summary>
        /// The pool id where the task is running or default Guid if the task doesn't belong to a pool.
        /// </summary>
        [InternalDataApiName(Name="PoolUuid")]
        public virtual Guid PoolUuid { get { return _taskApi.PoolUuid.IsNullOrEmpty() ? Guid.Empty : new Guid(_taskApi.PoolUuid); } }

        /// <summary>
        /// The pool where the task is running or null if the task doesn't belong to a pool.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual QPool Pool { get {
            if (_taskApi.PoolUuid.IsNullOrEmpty() || _taskApi.PoolUuid == Guid.Empty.ToString()) {
                return null;
            }
            else if (_pool == null)
            {
                try
                {
                    _pool = new QPool(_api, new Guid(_taskApi.PoolUuid), true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Could not load pool {0} info from API for task {1}. {2}", _taskApi.PoolUuid, _taskApi.Uuid, e);
                }
            }
            return _pool;
        } }


        /// <summary>
        /// The job id where the task is running or default Guid if the task is not attached to a job.
        /// </summary>
        [InternalDataApiName(Name="JobUuid")]
        public virtual Guid JobUuid { get { return _taskApi.JobUuid.IsNullOrEmpty() ? Guid.Empty : new Guid(_taskApi.JobUuid); } }

        /// <summary>
        /// The job the task is attached to or null if the task isn't attached to a job.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual QJob Job { get {
            if (_taskApi.JobUuid.IsNullOrEmpty() || _taskApi.JobUuid == Guid.Empty.ToString()) {
                return null;
            }
            else if (_job == null)
            {
                try
                {
                    _job = new QJob(_api, new Guid(_taskApi.JobUuid), true);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Warning: Could not load job {0} info from API for task {1}. {2}", _taskApi.JobUuid, _taskApi.Uuid, e);
                }
            }
            return _job;
        } }

        /// <summary>
        /// True if the task is completed or false if the task is still running or deploying.
        /// Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual bool Completed {
            get {
                return State == QTaskStates.Success || State == QTaskStates.Failure || State == QTaskStates.Cancelled;
            }
        }

        /// <summary>
        /// True if the task is executing (PartiallyExecuting or FullyExecuting) or false if the task is in another state.
        /// Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual bool Executing {
            get {
                return State == QTaskStates.PartiallyExecuting || State == QTaskStates.FullyExecuting;
            }
        }

        /// <summary>
        /// How many times this task have to run.
        /// </summary>
        [InternalDataApiName(Name="InstanceCount")]
        public virtual uint InstanceCount {
            get {
                if (_advancedRange == null) return _taskApi.InstanceCount;
                else return _advancedRange.Count;
            }

        }
        /// <summary>
        /// How many times this task can be retrieved if it is stopped while running.
        /// </summary>
        [InternalDataApiName(Name = "MaxRetriesPerInstance", IsFilterable = false)]
        [Obsolete("Prefer the use of the RetrySettings field and its MaxPerInstanceRetries property")]
        public virtual uint MaxRetriesPerInstance {
            get {
                return _taskApi.MaxRetriesPerInstance;
            }
            set {
                _taskApi.MaxRetriesPerInstance = value;
            }
        }

        /// <summary>
        /// The actual running instance count.
        /// </summary>
        [InternalDataApiName(Name="RunningInstanceCount")]
        public virtual uint RunningInstanceCount {
            get
            {
                return _taskApi?.RunningInstanceCount ?? default(uint);
            }
        }

        /// <summary>
        /// The actual running cores count.
        /// </summary>
        [InternalDataApiName(Name="RunningCoreCount")]
        public virtual uint RunningCoreCount {
            get
            {
                return _taskApi?.RunningCoreCount ?? default(uint);
            }
        }

        /// <summary>
        /// The task execution time.
        /// </summary>
        [InternalDataApiName(Name = "ExecutionTime")]
        public virtual TimeSpan ExecutionTime
        {
            get
            {
                return _taskApi?.ExecutionTime ?? default(TimeSpan);
            }
        }

        /// <summary>
        /// The task wall time.
        /// </summary>
        [InternalDataApiName(Name = "WallTime")]
        public virtual TimeSpan WallTime
        {
            get
            {
                return _taskApi?.WallTime ?? default(TimeSpan);
            }
        }

        /// <summary>
        /// The task end date.
        /// </summary>
        [InternalDataApiName(Name = "EndDate")]
        public virtual DateTime EndDate
        {
            get
            {
                return _taskApi?.EndDate ?? default(DateTime);
            }
        }

        /// <summary>
        /// The results include only the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        [InternalDataApiName(Name="ResultsWhitelist")]
        public virtual string ResultsWhitelist {
            get {
                return _taskApi.ResultsWhitelist;
            }
            set { _taskApi.ResultsWhitelist = value; }
        }

        /// <summary>
        /// The results exclude all the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        [InternalDataApiName(Name="ResultsBlacklist")]
        public virtual string ResultsBlacklist {
            get {
                return _taskApi.ResultsBlacklist;
            }
            set { _taskApi.ResultsBlacklist = value; }
        }


        /// <summary>
        /// The results bucket prefixes.
        /// </summary>
        [InternalDataApiName(Name="ResultsBucketPrefix")]
        public virtual string ResultsBucketPrefix {
            get {
                return _taskApi?.ResultsBucketPrefix;
            }
            set {
                if (_taskApi != null)
                    _taskApi.ResultsBucketPrefix = value;
            }
        }


        /// <summary>
        /// The snapshots include only the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        [InternalDataApiName(Name="SnapshotWhitelist")]
        public virtual string SnapshotWhitelist {
            get {
                return _taskApi.SnapshotWhitelist;
            }
            set { _taskApi.SnapshotWhitelist = value; }
        }

        /// <summary>
        /// The snapshots exclude all the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        [InternalDataApiName(Name="SnapshotBlacklist")]
        public virtual string SnapshotBlacklist {
            get {
                return _taskApi.SnapshotBlacklist;
            }
            set { _taskApi.SnapshotBlacklist = value; }
        }

        private QBucket _snapshotBucket { get; set; }

        /// <summary>
        /// The snapshots bucket, can be different from the Result bucket.
        /// </summary>
        [InternalDataApiName(Name="SnapshotBucket")]
        public virtual QBucket SnapshotBucket {
            get {
                return _snapshotBucket;
            }
            set {
                if (_taskApi != null)
                    _taskApi.SnapshotBucket = value.Shortname;
                _snapshotBucket = QBucket.GetBucketFromResource(value);
            }
        }

        /// <summary>
        /// The snapshots bucket prefixes.
        /// </summary>
        [InternalDataApiName(Name="SnapshotBucketPrefix")]
        public virtual string SnapshotBucketPrefix {
            get {
                return _taskApi?.SnapshotBucketPrefix;
            }
            set {
                if (_taskApi != null)
                    _taskApi.SnapshotBucketPrefix = value;
            }
        }

        /// <summary>
        /// AutoDeleteOnCompletion: Field allowing the automatic deletion of the task when in a final state.
        /// Must be set before the submission.
        /// </summary>
        [InternalDataApiName(IsFilterable = false, IsSelectable = false)]
        public bool AutoDeleteOnCompletion {
            get {
                return _taskApi.AutoDeleteOnCompletion;
            }
            set {
                _taskApi.AutoDeleteOnCompletion = value;
            }
        }

        /// <summary>
        /// CompletionTimeToLive: Final State Duration before deletion of the task.
        /// Must be set before the submission.
        /// </summary>
        [InternalDataApiName(IsFilterable = false, IsSelectable = false)]
        public TimeSpan CompletionTimeToLive {
            get {
                return _taskApi.CompletionTimeToLive;
            }
            set {
                _taskApi.CompletionTimeToLive = value;
            }
        }

        /// <summary>
        /// The task hardware constraints list.
        /// </summary>
        [InternalDataApiName(Name="HardwareConstraints")]
        public virtual HardwareConstraints HardwareConstraints
        {
            get
            {
                return _taskApi.HardwareConstraints;
            }
            set
            {
                _taskApi.HardwareConstraints = value;
            }
        }

        /// <summary>
        /// The default TTL for the task resources cache.
        /// </summary>
        [InternalDataApiName(Name="DefaultResourcesCacheTTLSec")]
        public virtual uint? DefaultResourcesCacheTTLSec
        {
            get
            {
                return _taskApi.DefaultResourcesCacheTTLSec;
            }
            set
            {
                _taskApi.DefaultResourcesCacheTTLSec = value;
            }
        }

        /// <summary>
        /// The task privileges list.
        /// </summary>
        [InternalDataApiName(Name="Privileges")]
        public virtual Privileges Privileges
        {
            get
            {
                return _taskApi.Privileges;
            }
            set
            {
                _taskApi.Privileges = value;
            }
        }

        /// <summary>
        /// The task retry settings if instance fails.
        /// </summary>
        [InternalDataApiName(Name="RetrySettings")]
        public virtual RetrySettings RetrySettings
        {
            get
            {
                return _taskApi.RetrySettings;
            }
            set
            {
                _taskApi.RetrySettings = value;
            }
        }

        /// <summary>
        /// The list of status of completed instances.
        /// </summary>
        [InternalDataApiName(Name="CompletedInstances")]
        public virtual List<QTaskCompletedInstance> CompletedInstances
        {
            get
            {
                return _taskApi.CompletedInstances;
            }
            set
            {
                _taskApi.CompletedInstances = value;
            }
        }


        /// <summary>
        /// Queue in-pool task execution behind pool resources update.
        /// </summary>
        /// <remarks>
        /// For an in-pool task, if set to true, any task submitted after a pool resources update will be sure to see
        /// the newer pool resources during its execution. The task will be queued until a pool slot with recent enough
        /// resources is available.
        /// Setting this to false will deactivate this behavior.
        /// If left null, then the pool's TaskDefaultWaitForPoolResourcesSynchronization value will be used. If both are
        /// null, then it will default to false.
        /// </remarks>
        /// <seealso cref="QPool.TaskDefaultWaitForPoolResourcesSynchronization" />
        [InternalDataApiName(IsFilterable = true, IsSelectable = true)]
        public virtual bool? WaitForPoolResourcesSynchronization {
            get {
                return _taskApi?.WaitForPoolResourcesSynchronization;
            }
        }

        /// <summary>
        /// Description of the secrets the task will have access to when running.
        /// </summary>
        /// <remarks>
        /// Secrets can be accessible either by exact match on the key or by using a prefix
        /// in order to match all the secrets starting with said prefix.
        /// </remarks>
        [InternalDataApiName(Name="SecretsAccessRights", IsFilterable = false)]
        public virtual QSecretAccessRights SecretsAccessRights
        {
            get => _taskApi.SecretsAccessRights;
            set => _taskApi.SecretsAccessRights = value;
        }


        /// <summary>
        /// Scheduling type use for the dispatch of the task.
        /// </summary>
        /// <remarks>
        /// Available scheduling are: <see cref="SchedulingType.Flex" />, <see cref="SchedulingType.OnDemand" />
        /// and <see cref="SchedulingType.Reserved" />
        /// </remarks>
        [InternalDataApiName(Name="SchedulingType", IsFilterable=false)]
        public SchedulingType? SchedulingType {
            get
            {
                return _taskApi?.SchedulingType;
            }
            set
            {
                _taskApi.SchedulingType = value;
            }
        }


        /// <summary>
        /// The key of the reserved machine the task should be dispatch on.
        /// </summary>
        /// <remarks>
        /// To use with <see cref="SchedulingType.Reserved" /> SchedulingType
        /// </remarks>
        [InternalDataApiName(Name="TargetedReservedMachineKey", IsFilterable=false)]
        public string TargetedReservedMachineKey {
            get
            {
                return _taskApi?.TargetedReservedMachineKey;
            }
            set
            {
                _taskApi.TargetedReservedMachineKey = value;
            }
        }

        /// <summary>
        /// Network rules forced for the task.
        /// </summary>
        [InternalDataApiName(Name="ForcedNetworkRules")]
        public virtual List<ForcedNetworkRule> ForcedNetworkRules
        {
            get
            {
                return _taskApi.ForcedNetworkRules;
            }
            set
            {
                _taskApi.ForcedNetworkRules = value;
            }
        }

        /// <summary>
        /// Constant forced for the task.
        /// </summary>
        [InternalDataApiName(Name="ForcedConstants")]
        public virtual List<ForcedConstant> ForcedConstants
        {
            get
            {
                return _taskApi.ForcedConstants;
            }
            set
            {
                _taskApi.ForcedConstants = value;
            }
        }

        /// <summary>
        /// Create a new task outside of a pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task shortname.</param>
        /// <param name="profile">The task profile. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        public QTask(Connection connection, string name, string profile = null, string shortname = default(string))
            : base (connection, new TaskApi()) {
            _taskApi.Name = name;
            _taskApi.Profile = profile;
            _resources = new List<QAbstractStorage>();
            _constants = new Dictionary<string, string>();
            _constraints = new Dictionary<string, string>();
            _taskApi.Shortname = shortname;
            _uri = "tasks/" + shortname;
            SecretsAccessRights = new QSecretAccessRights();
        }

        /// <summary>
        /// Create a new task outside of a pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="profile">The task profile. If not specified, it must be given when the task is submitted.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="schedulingType">Type of scheduling used for the dispatch of the task</param>
        public QTask(Connection connection, string name, string profile, uint instanceCount = 0, string shortname = default(string), SchedulingType? schedulingType = null) : this(connection, name, profile, shortname) {
            _taskApi.InstanceCount = instanceCount;
            _taskApi.SchedulingType = schedulingType;
        }

        /// <summary>
        /// Create a new task outside of a pool with an advanced range representation.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="profile">The task profile. If not specified, it must be given when the task is submitted.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="schedulingType">Type of scheduling used for the dispatch of the task</param>
        public QTask(Connection connection, string name, string profile, AdvancedRanges range, string shortname = default(string), SchedulingType? schedulingType = null) : this(connection, name, profile, shortname) {
            _advancedRange = range ?? new AdvancedRanges(null);
            _taskApi.AdvancedRanges = _advancedRange.ToString();
            _taskApi.SchedulingType = schedulingType;
        }

        /// <summary>
        /// Create a new task inside an existing pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="pool">The pool where this task will run.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="waitForPoolResourcesSynchronization">Whether task should wait for previous pool resources
        /// update to be completed before executing. See <see cref="WaitForPoolResourcesSynchronization" /></param>
        public QTask(Connection connection, string name, QPool pool, uint instanceCount = 0, string shortname = default(string), bool? waitForPoolResourcesSynchronization=null)
            : this(connection, name, (string)null, instanceCount, shortname)
        {
            _taskApi.PoolUuid = pool.Uuid.ToString();
            _pool = pool;
            _taskApi.WaitForPoolResourcesSynchronization = waitForPoolResourcesSynchronization;
        }


        /// <summary>
        /// Create a new task inside an existing pool, with an advanced range representation.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task shortname.</param>
        /// <param name="pool">The pool where this task will run.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="waitForPoolResourcesSynchronization">Whether task should wait for previous pool resources
        /// update to be completed before executing. See <see cref="WaitForPoolResourcesSynchronization" /></param>
        public QTask(Connection connection, string name, QPool pool, AdvancedRanges range, string shortname = default(string), bool? waitForPoolResourcesSynchronization=null)
            : this(connection, name, pool, 0, shortname, waitForPoolResourcesSynchronization)
        {
            _advancedRange = range ?? new AdvancedRanges(null);
            _taskApi.AdvancedRanges = _advancedRange.ToString();
        }


        /// <summary>
        /// Create a new task inside an existing job.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="job">The job where this task will run.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="profile">profile for the task.(should be use with a job not attached to a pool)</param>
        /// <param name="schedulingType">Type of scheduling used for the dispatch of the task</param>
        public QTask(Connection connection, string name, QJob job, uint instanceCount = 0, string shortname = default(string),
            string profile=default(string), SchedulingType? schedulingType = null) : this(connection, name, profile, instanceCount, shortname, schedulingType)
        {
            _taskApi.JobUuid = job.Uuid.ToString();
            _job = job;

            if (job.PoolUuid == default(Guid))
                _useStandaloneJob = true;
        }

        /// <summary>
        /// Create a new task inside of an existing job.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task shortname.</param>
        /// <param name="job">The job where this task will run.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="profile">profile for the task.(should be use with a job not attached to a pool)</param>
        /// <param name="schedulingType">Type of scheduling used for the dispatch of the task</param>
        public QTask(Connection connection, string name, QJob job, AdvancedRanges range, string shortname = default(string),
            string profile=default(string), SchedulingType? schedulingType = null) : this(connection, name, job, 0, shortname, profile, schedulingType) {
            _advancedRange = range ?? new AdvancedRanges(null);
            _taskApi.AdvancedRanges = _advancedRange.ToString();
        }

        /// <summary>
        /// Create a task object given an existing Uuid.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="uuid">The Uuid of an already existing task.</param>
        public QTask(Connection connection, Guid uuid) : this(connection, uuid.ToString(), (string)null, 0) {
            _uri = "tasks/" + uuid.ToString();
            _taskApi.Uuid = uuid;
        }

        internal QTask() {
            _resources = new List<QAbstractStorage>();
            _constants = new Dictionary<string, string>();
            _constraints = new Dictionary<string, string>();
        }

        internal async new Task<QTask> InitializeAsync(Connection qapi, TaskApi taskApi) {
            await base.InitializeAsync(qapi, taskApi);
             _uri = "tasks/" + taskApi.Uuid.ToString();
            if (_resources == null) _resources = new List<QAbstractStorage>();
            if (_constants == null) _constants = new Dictionary<string, string>();
            if (_constraints == null) _constraints = new Dictionary<string, string>();
            await SyncFromApiObjectAsync(taskApi);
            return this;
        }

        internal async static Task<QTask> CreateAsync(Connection qapi, TaskApi taskApi) {
            return await new QTask().InitializeAsync(qapi, taskApi);
        }

        #region public methods

        /// <summary>
        /// Set the task depencencies.
        /// The task need to be in a job with depencendies activated
        /// </summary>
        /// <param name="guids">list of task guids this task depends on.</param>
        public virtual void SetTaskDependencies(params Guid [] guids)
        {
            if (_taskApi.Dependencies == null)
                _taskApi.Dependencies = new Dependency();
            _taskApi.Dependencies.DependsOn = guids.ToList();
        }

        /// <summary>
        /// Set the task depencencies.
        /// The task need to be in a job with depencendies activated
        /// </summary>
        /// <param name="tasks">list of task this task depends on.</param>
        public virtual void SetTaskDependencies(params QTask [] tasks)
        {
            if (_taskApi.Dependencies == null)
                _taskApi.Dependencies = new Dependency();
            _taskApi.Dependencies.DependsOn = tasks.Select(t => t.Uuid).ToList();
        }

        /// <summary>
        /// Set the a list of tags for the task.
        /// </summary>
        /// <param name="tags">Task tags.</param>
        public virtual void SetTags(params String [] tags) {
            _taskApi.Tags = tags.Distinct().ToList();
        }

        /// <summary>
        /// Deprecated, use SetConstant.
        /// </summary>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value.</param>
        [Obsolete("use SetConstant")]
        public virtual void AddConstant(string name, string value) {
            if (_constants == null) _constants = new Dictionary<string, string>();
            _constants.Add(name, value);
        }

        /// <summary>
        /// Set a constant. If the constant already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value. If null, the constant is deleted.</param>
        public virtual void SetConstant(string name, string value) {
            if (_constants == null)
            {
                _constants = new Dictionary<string, string>();
            }

            if (value != null)
            {
                _constants[name] = value;
            }
            else
            {
                _constants.Remove(name);
            }
        }

        /// <summary>
        /// Set a constraint. If the constraint already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constraint name.</param>
        /// <param name="value">Constraint value. If null, the constraint is deleted.</param>
        public virtual void SetConstraint(string name, string value) {
            if (_constraints == null)
            {
                _constraints = new Dictionary<string, string>();
            }

            if (value == null)
            {
                _constraints.Remove(name);
            }
            else
            {
                _constraints[name] = value;
            }
        }

        /// <summary>
        /// Set a label. If the label already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Label name.</param>
        /// <param name="value">Label value. If null, the label is deleted.</param>
        public virtual void SetLabel(string name, string value) {
            if (value == null)
            {
                Labels.Remove(name);
            }
            else
            {
                Labels[name] = value;
            }
        }

        /// <summary>
        /// Run this task.
        /// </summary>
        /// <param name="taskTimeoutSeconds">Optional number of second before abort is called.</param>
        /// <param name="outputDirectory">local directory for the retrieved files</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns></returns>
        public virtual async Task RunAsync(int taskTimeoutSeconds=-1, string outputDirectory=default, CancellationToken ct=default) {
            await SubmitAsync(null, 0, ct);
            await WaitAsync(taskTimeoutSeconds, ct);

            if (taskTimeoutSeconds > 0)
            {
                await AbortAsync(ct);
            }

            if (outputDirectory != default)
            {
                await DownloadResultAsync(outputDirectory, ct);
            }
        }

        /// <summary>
        /// Wait this task completion.
        /// </summary>
        /// <param name="taskTimeoutSeconds">Optional maximum number of second to wait for completion.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns>true if the task is completed</returns>
        public virtual async Task<bool> WaitAsync(int taskTimeoutSeconds=-1, CancellationToken ct =default(CancellationToken)) {
            double period = TimeSpan.FromSeconds(10).TotalMilliseconds;
            double sleepingTimeMs;
            var start = DateTime.Now;
            while (!Completed) {
                await UpdateStatusAsync(ct);
                var elasped = (DateTime.Now - start).Seconds;

                // loop timeout exit condition
                if(taskTimeoutSeconds > 0 && elasped > taskTimeoutSeconds) return false;

                // loop delay
                if(taskTimeoutSeconds > 0)
                    sleepingTimeMs = Math.Min(period, TimeSpan.FromSeconds(taskTimeoutSeconds - elasped).TotalMilliseconds);
                else
                    sleepingTimeMs = period;
                await Task.Delay(TimeSpan.FromMilliseconds(sleepingTimeMs), ct);
            }
            return true;
        }

        /// <summary>
        /// Download result in the given directory
        /// warning: Will override *output_dir* content.
        /// </summary>
        /// <param name="outputDirectory">local directory for the retrieved files</param>
        /// <param name="cancellationToken">Optional token to cancel the request</param>
        public virtual async Task DownloadResultAsync(string outputDirectory, CancellationToken cancellationToken=default) {
            if (!Directory.Exists(outputDirectory)) {
                Directory.CreateDirectory (outputDirectory);
            }

            await ResultsBucket?.DownloadFolderAsync(".", outputDirectory, cancellationToken);
        }

        /// <summary>
        /// Commit the local task changes.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public virtual async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            // build the constants
            _taskApi.Constants = new List<KeyValHelper>();
            foreach(var c in _constants) { _taskApi.Constants.Add(new KeyValHelper(c.Key, c.Value)); }

            // build the constraints
            _taskApi.Constraints = new List<KeyValHelper>();
            foreach(var c in _constraints) { _taskApi.Constraints.Add(new KeyValHelper(c.Key, c.Value)); }


            using (var response = await _api._client.PutAsJsonAsync<TaskApi>(_uri, _taskApi, cancellationToken))
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
        }


        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="instanceCount">How many times the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to 0.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public virtual async Task SubmitAsync(string profile=null, uint instanceCount=0, CancellationToken cancellationToken=default(CancellationToken)) {
            if (profile != null) _taskApi.Profile = profile;
            if (instanceCount > 0) _taskApi.InstanceCount = instanceCount;
            await SubmitAsync(cancellationToken);
        }


        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="range">Which instance ids of the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to null.</param>
        /// <returns></returns>
        public virtual async Task SubmitAsync(string profile, AdvancedRanges range, CancellationToken cancellationToken=default(CancellationToken)) {
            if (profile != null) _taskApi.Profile = profile;
            if (range != null) _advancedRange = range;
            _taskApi.AdvancedRanges = _advancedRange.ToString();
            await SubmitAsync(cancellationToken);
        }

        internal async Task PreSubmitAsync(CancellationToken cancellationToken) {
            if (_taskApi.InstanceCount > 0 && !String.IsNullOrEmpty(_taskApi.AdvancedRanges)) {
                throw new Exception("Can't use at the same time an instance count and a range.");
            }
            if (_taskApi.InstanceCount == 0 && String.IsNullOrEmpty(_taskApi.AdvancedRanges)) {
                throw new Exception("An instance count or a range must be set to submit a task.");
            }
            if (_taskApi.JobUuid == default(string) && _taskApi.Dependencies != null && !_taskApi.Dependencies.DependsOn.IsNullOrEmpty()) {
                throw new Exception("A task not attached to a job can not use dependency.");
            }
            if (_useStandaloneJob  && _taskApi.Profile == default(string)) {
                throw new Exception("A task attached to a job without a pool should have a profile.");
            }
            if (_taskApi.JobUuid != default(string) && !_useStandaloneJob  && _taskApi.Profile != default(string)) {
                throw new Exception("A task attached to a job with a pool can not have a profile.");
            }
            if (_taskApi.TargetedReservedMachineKey != default(string) && _taskApi.SchedulingType != QarnotSDK.SchedulingType.Reserved) {
                throw new Exception("Cannot target a reserved machine without using a 'Reserved' scheduling type.");
            }

            // Is a result bucket defined?
            if (_results != null) {
                if (_results != null) {
                    _taskApi.ResultBucket = _results.Shortname;
                    _taskApi.ResultsCacheTTLSec = _results?.CacheTTLSec;
                } else {
                    throw new Exception("Unknown IQStorage implementation");
                }
            }

            // build the constants
            _taskApi.Constants = new List<KeyValHelper>();
            foreach(var c in _constants) { _taskApi.Constants.Add(new KeyValHelper(c.Key, c.Value)); }

            // build the constraints
            _taskApi.Constraints = new List<KeyValHelper>();
            foreach(var c in _constraints) { _taskApi.Constraints.Add(new KeyValHelper(c.Key, c.Value)); }

            // Build the resource bucket list
            _taskApi.ResourceBuckets = new List<string>();
            bool useAdvancedResources = ResourcesBuckets.Any(res => res?.Filtering != null || res?.ResourcesTransformation != null || res?.CacheTTLSec != null);
            foreach (var item in _resources) {
                var resQBucket = item as QBucket;
                if (resQBucket != null) {
                    if (useAdvancedResources) {
                        _taskApi.AdvancedResourceBuckets.Add(new ApiAdvancedResourceBucket {
                            BucketName = resQBucket.Shortname,
                            Filtering = resQBucket.Filtering is BucketFilteringPrefix prefixFiltering ?
                                new ApiBucketFiltering {
                                    PrefixFiltering = new ApiBucketFilteringPrefix {
                                        Prefix = _api._shouldSanitizeBucketPaths ? Utils.GetSanitizedBucketPath(prefixFiltering.Prefix, _api._showBucketWarnings) : prefixFiltering.Prefix
                                    }
                                }
                                : null,
                            ResourcesTransformation = resQBucket.ResourcesTransformation is ResourcesTransformationStripPrefix stripPrefixTransformation ?
                                new ApiResourcesTransformation {
                                    StripPrefix = new ApiResourcesTransformationStripPrefix {
                                        Prefix = _api._shouldSanitizeBucketPaths ? Utils.GetSanitizedBucketPath(stripPrefixTransformation.Prefix, _api._showBucketWarnings) : stripPrefixTransformation.Prefix
                                    }
                                }
                                : null,
                            CacheTTLSec = resQBucket.CacheTTLSec
                        });
                    } else {
                        _taskApi.ResourceBuckets.Add(resQBucket.Shortname);
                    }
                } else {
                    throw new Exception("Unknown IQStorage implementation");
                }
            }

            if (_api.IsReadOnly) throw new Exception("Can't submit tasks, this connection is configured in read-only mode");
            await Task.FromResult(0);
        }

        private async Task SubmitAsync(CancellationToken cancellationToken=default(CancellationToken)) {
            await PreSubmitAsync(cancellationToken);
            using (var response = await _api._client.PostAsJsonAsync<TaskApi>("tasks", _taskApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
                var result = await response.Content.ReadAsAsync<TaskApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                await PostSubmitAsync(result, cancellationToken);
            }
        }

        internal async Task PostSubmitAsync(TaskApi result, CancellationToken cancellationToken) {
            // Update the task Uuid
            _taskApi.Uuid = result.Uuid;
            _uri = "tasks/" + _taskApi.Uuid.ToString();

            // Retrieve the task status once to update the other fields (result bucket uuid etc..)
            await UpdateStatusAsync(cancellationToken);
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="updateQBucketsInfo">If set to true, the resources and results bucket objects are also updated.</param>
        /// <returns></returns>
        public virtual async Task UpdateStatusAsync(bool updateQBucketsInfo = true) {
            await UpdateStatusAsync(default(CancellationToken), updateQBucketsInfo);
        }

        private async Task SyncFromApiObjectAsync(TaskApi result) {
            _taskApi = result;

            // update task range
            if (_taskApi.AdvancedRanges != null) _advancedRange = new AdvancedRanges(_taskApi.AdvancedRanges);
            else _advancedRange = null;

            // update constants
            _constants = result.Constants?.GroupBy(e => e.Key).ToDictionary(kv => kv.First().Key, kv => kv.First().Value) ?? new Dictionary<string, string>();

            // update constraints
            _constraints = result.Constraints?.GroupBy(e => e.Key).ToDictionary(kv => kv.First().Key, kv => kv.First().Value) ?? new Dictionary<string, string>();

            // update the task resources
            var newResourcesCount = 0;
            if (_taskApi.ResourceBuckets != null) newResourcesCount += _taskApi.ResourceBuckets.Count;
            if (_taskApi.AdvancedResourceBuckets != null) newResourcesCount += _taskApi.AdvancedResourceBuckets.Count;

            if (_resources.Count != newResourcesCount) {
                _resources.Clear();

                if (_taskApi.ResourceBuckets != null) {
                    foreach (var r in _taskApi.ResourceBuckets) {
                        _resources.Add(await QBucket.CreateAsync(_api, r, create: false));
                    }
                }
                if (_taskApi.AdvancedResourceBuckets != null) {
                    foreach (var r in _taskApi.AdvancedResourceBuckets) {
                        _resources.Add(await QBucket.CreateAsync(_api, r, create: false));
                    }
                }
            }
            // update the task result
            if (_results == null && _taskApi.ResultBucket != null) {
                _results = await QBucket.CreateAsync(_api, _taskApi.ResultBucket, create: false);
            }
            // update the task result
            if (_snapshotBucket == null && _taskApi.SnapshotBucket != null) {
                _snapshotBucket = await QBucket.CreateAsync(_api, _taskApi.SnapshotBucket, create: false);
            }
        }

        /// <summary>
        /// Call a periodic snapshot every interval seconds.
        /// You can select some additional options, like a bucket or a whitelist.
        /// </summary>
        /// <param name="interval">The interval in second between 2 snapshots.</param>
        /// <param name="whitelist">A globing string representing the whitelist filenames.</param>
        /// <param name="blacklist">A globing string representing the blacklist filenames.</param>
        /// <param name="bucket">A specific bucket to retrieve the snapshot files.</param>
        /// <param name="bucketPrefix">A bucket prefix folder to the snapshot bucket,
        /// bucketPrefix prevent key collisions when using snapshots with many tasks.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public override async Task TriggerPeriodicSnapshotAsync(uint interval, string whitelist = null, string blacklist = null, QBucket bucket = null, string bucketPrefix = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.TriggerPeriodicSnapshotAsync(interval, whitelist, blacklist, bucket, bucketPrefix, cancellationToken);

            // Retrieve the task status once to update the other fields (result bucket uuid etc..)
            await UpdateStatusAsync(cancellationToken);
        }

    /// <summary>
    /// Update this task state and status.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the request.</param>
    /// <param name="updateQBucketsInfo">If set to true, the resources and results bucket objects are also updated.</param>
    /// <returns></returns>
    public virtual async Task UpdateStatusAsync(CancellationToken cancellationToken, bool updateQBucketsInfo = true) {
            using (var response = await _api._client.GetAsync(_uri, cancellationToken)) // get task status
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response);

                var result = await response.Content.ReadAsAsync<TaskApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                await SyncFromApiObjectAsync(result);

                if (updateQBucketsInfo) {
                    foreach (var r in _resources) {
                        await r.UpdateAsync(cancellationToken);
                    }
                    if (_results != null) {
                        await _results.UpdateAsync(cancellationToken);
                    }
                    if (_snapshotBucket != null) {
                        await _snapshotBucket.UpdateAsync(cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the task doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <param name="purgeResults">Boolean to trigger result storage deletion. Default is false.</param>
        /// <returns></returns>
        public override async Task DeleteAsync(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false, bool purgeResults=false) {
            try {
                if (_api.IsReadOnly) throw new Exception("Can't delete tasks, this connection is configured in read-only mode");

                var resourcesToDelete = new List<QAbstractStorage>();
                QAbstractStorage resultToDelete = null;

                if(purgeResources)
                    resourcesToDelete = this._resources;

                if (purgeResults)
                    resultToDelete = this.Results;

                using (var response = await _api._client.DeleteAsync(_uri, cancellationToken))
                    await Utils.LookForErrorAndThrowAsync(_api._client, response);

                var deleteTasks = resourcesToDelete.Select(r => r.DeleteAsync(cancellationToken)).ToList();
                if (resultToDelete != null) deleteTasks.Add(resultToDelete.DeleteAsync(cancellationToken));

                await Task.WhenAll(deleteTasks);
            } catch (QarnotApiResourceNotFoundException ex) {
                if (failIfDoesntExist) throw ex;
            }
        }

        /// <summary>
        /// Get the task carbon facts
        /// </summary>
        /// <returns>The carbon facts of the task.</returns>
        public virtual async Task<CarbonFacts> GetCarbonFactsAsync(string referenceDatacenter = null, CancellationToken ct = default) {
            return await _api.CarbonClient.GetTaskCarbonFactsAsync(Uuid, referenceDatacenter, ct);
        }

        #endregion

        #region helpers
        /// <summary>
        /// Return the public host and port to establish an inbound connection to the master compute node (instance 0) running your task.
        /// Note: your profile have to define one or more inbound connection to support that feature. For example, the profile "docker-network-ssh"
        ///  defines a redirection to the ssh port 22. If you need inbound connections on a specific port, you can make a request to the support team.
        /// </summary>
        /// <param name="port">The port you want to access on the master compute node (instance 0).</param>
        /// <returns>The host and port formated in a string "host:port".</returns>
        public virtual string GetPublicHostForApplicationPort(UInt16 port) {
            if (Status != null && Status.RunningInstancesInfo != null) {
                var instances = Status.RunningInstancesInfo.PerRunningInstanceInfo;
                if (instances != null && instances.Count > 0) {
                    foreach (var i in instances) {
                        if (i.ActiveForwards == null) continue;
                        foreach (var af in i.ActiveForwards) {
                            if (af.ApplicationPort == port) {
                                return String.Format("{0}:{1}", af.ForwarderHost, af.ForwarderPort);
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Enumeration on the task instance ids.
        /// Useful if an advanced range is used.
        /// </summary>
        public virtual IEnumerable<UInt32> Instances {
            get {
                if (_advancedRange != null) {
                    foreach (var i in _advancedRange)
                        yield return i;
                } else {
                    for (UInt32 i = 0; i < _taskApi.InstanceCount; i++)
                        yield return i;
                }
            }
        }

        /// <summary>
        /// Get the status of an instance given its instance id.
        /// Note: the status of an instance could also be retrieved in the Status.CompletedInstances or
        ///  Status.RunningInstancesInfo.PerRunningInstanceInfo structures. This method provides an
        ///  unified way to retrieve those information.
        /// </summary>
        /// <param name="instanceId">The id of the instance.</param>
        /// <returns>The status of the instance or null if not available.</returns>
        public virtual QTaskInstanceStatus GetInstanceStatus(UInt32 instanceId) {
            if (_taskApi.CompletedInstances != null) {
                foreach (var j in _taskApi.CompletedInstances) {
                    if (j.InstanceId == instanceId) {
                        return new QTaskInstanceStatus(j);
                    }
                }
            }
            if (_taskApi.Status != null && _taskApi.Status.RunningInstancesInfo != null && _taskApi.Status.RunningInstancesInfo.PerRunningInstanceInfo != null) {
                foreach (var j in _taskApi.Status.RunningInstancesInfo.PerRunningInstanceInfo) {
                    if (j.InstanceId == instanceId) {
                        return new QTaskInstanceStatus(j);
                    }
                }
            }
            return null;
        }
        #endregion
    }

    /// <summary>
    /// Represents an unified an simplified version of a task instance status.
    /// </summary>
    public class QTaskInstanceStatus {
        /// <summary>
        /// Retrieve the instance state.
        /// </summary>
        public virtual string State { get; private set; }

        /// <summary>
        /// Retrieve the instance error
        /// </summary>
        public virtual QTaskError Error { get; private set; }

        /// <summary>
        /// Retrieve the instance progress indicator
        /// </summary>
        public virtual float Progress { get; private set; }

        /// <summary>
        /// Instance execution time(in seconds).
        /// </summary>
        public virtual float ExecutionTimeSec { get; private set; }

        /// <summary>
        /// Instance execution time frequency(in seconds.ghz).
        /// </summary>
        public virtual float ExecutionTimeGHz { get; private set; }

        /// <summary>
        /// Retrieve the instance wall time(in seconds).
        /// </summary>
        public virtual float WallTimeSec { get; private set; }

        /// <summary>
        /// Informations about the running instances.(see QTaskStatusPerRunningInstanceInfo)
        /// </summary>
        public virtual QTaskStatusPerRunningInstanceInfo RunningInstanceInfo {get; private set; }

        /// <summary>
        /// Informations about completed instances (see QTaskCompletedInstance)
        /// </summary>
        public virtual QTaskCompletedInstance CompletedInstanceInfo { get; private set; }

        internal QTaskInstanceStatus(QTaskStatusPerRunningInstanceInfo i) {
            // Phase is lowercase, convert the first letter to uppercase
            State = String.IsNullOrEmpty(i.Phase) ? "Unknown" : (i.Phase[0].ToString().ToUpper() + i.Phase.Substring(1));
            Error = null;
            Progress = i.Progress;
            ExecutionTimeGHz = i.ExecutionTimeGHz;
            ExecutionTimeSec = i.ExecutionTimeSec;
            WallTimeSec = 0;
            RunningInstanceInfo = i;
            CompletedInstanceInfo = null;
        }
        internal QTaskInstanceStatus(QTaskCompletedInstance i) {
            State = i.State;
            Error = i.Error;
            Progress = 100;
            ExecutionTimeGHz = i.ExecTimeSecGhz;
            ExecutionTimeSec = i.ExecTimeSec;
            WallTimeSec = i.WallTimeSec;
            RunningInstanceInfo = null;
            CompletedInstanceInfo = i;
        }
    }
}

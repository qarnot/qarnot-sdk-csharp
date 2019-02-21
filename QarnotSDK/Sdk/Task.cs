using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using System;

// TODO: Commit changes made to a task

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
        /// The results are being uploaded from the compute nodes to the result disk.
        /// </summary>
        [Obsolete("will be replaced by UploadingResults")]
        public static readonly string DownloadingResults    = "DownloadingResults";
        /// <summary>
        /// The results are being uploaded from the compute nodes to the result disk.
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

        private AdvancedRanges _advancedRange = null;
        /// <summary>
        /// The task shortname identifier. The shortname is provided by the user. It has to be unique.
        /// </summary>
        public string Shortname { get { return _taskApi.Shortname == null ? _taskApi.Uuid.ToString() : _taskApi.Shortname; } }
        /// <summary>
        /// The task name.
        /// </summary>
        public string Name { get { return _taskApi.Name; } }
        /// <summary>
        /// The task profile.
        /// </summary>
        public string Profile { get { return _taskApi.Profile; } }

        /// <summary>
        /// Qarnot resources disks or buckets bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        public List<QAbstractStorage> Resources {
            get {
                return _resources;
            }
            set {
                _resources = value;
            }
        }
        private List<QAbstractStorage> _resources { get; set; }

        /// <summary>
        /// Qarnot resources disks bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        public IEnumerable<QDisk> ResourcesDisks {
            get {
                return GetResources<QDisk>();
            }
        }
        /// <summary>
        /// Qarnot resources buckets bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        public IEnumerable<QBucket> ResourcesBuckets {
            get {
                return GetResources<QBucket>();
            }
        }

        /// <summary>
        /// Qarnot result disk or bucket bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        public QAbstractStorage Results {
            get {
                return _results;
            }
            set {
                _results = value;
            }
        }

        private QAbstractStorage _results { get; set; }

        /// <summary>
        /// Qarnot result disk bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        public QDisk ResultsDisk {
            get {
                return GetResults<QDisk>();
            }
        }

        /// <summary>
        /// Qarnot result bucket bound to this task.
        /// Can be set only before the task submission.
        /// </summary>
        public QBucket ResultsBucket {
            get {
                return GetResults<QBucket>();
            }
        }

        /// <summary>
        /// Retrieve the task state (see QTaskStates).
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public string State { get { return _taskApi != null ? _taskApi.State : null; } }

        /// <summary>
        /// Retrieve the task errors.
        /// </summary>
        public List<QTaskError> Errors {
            get {
                return _taskApi != null ? _taskApi.Errors : new List<QTaskError>();
            }
        }

        /// <summary>
        /// Retrieve the task detailed status.
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public QTaskStatus Status {
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
        public DateTime CreationDate { get { return _taskApi.CreationDate; } }

        /// <summary>
        /// Increased each time a new set of results is available, when snapshot or final results are ready.
        /// Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public uint ResultsCount {
            get {
                return _taskApi.ResultsCount;
            }
        }

        /// <summary>
        /// The custom task tag list.
        /// </summary>
        public List<String> Tags {
            get {
                return _taskApi.Tags;
            }
        }

        /// <summary>
        /// The task constants.
        /// </summary>
        /// <returns>return all Constants</returns>
        public Dictionary<string, string> Constants {
            get {
                var constants = _taskApi.Constants;
                if (constants == null)
                    return new Dictionary<string, string>();

                return constants.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        /// <summary>
        /// The task constraints.
        /// </summary>
        /// <returns>return all Constraints</returns>
        public Dictionary<string, string> Constraints {
            get {
                var constraints = _taskApi.Constraints;
                if (constraints == null)
                    return new Dictionary<string, string>();

                return constraints.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        /// <summary>
        /// The delay in seconds between two periodic snapshots.
        /// Once the task is running, use the SnapshotPeriodic method to update.
        /// </summary>
        public int SnapshotInterval {
            get {
                return _taskApi.SnapshotInterval;
            }
        }

        /// <summary>
        /// The pool where the task is running or null if the task doesn't belong to a pool.
        /// </summary>
        public QPool Pool { get { return (_taskApi.PoolUuid == null || _taskApi.PoolUuid == Guid.Empty.ToString()) ? null : new QPool(_api, new Guid(_taskApi.PoolUuid)); } }

        /// <summary>
        /// True if the task is completed or false if the task is still running or deploying.
        /// Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public bool Completed {
            get {
                return State == QTaskStates.Success || State == QTaskStates.Failure || State == QTaskStates.Cancelled;
            }
        }

        /// <summary>
        /// True if the task is executing (PartiallyExecuting or FullyExecuting) or false if the task is in another state.
        /// Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public bool Executing {
            get {
                return State == QTaskStates.PartiallyExecuting || State == QTaskStates.FullyExecuting;
            }
        }

        /// <summary>
        /// How many times this task have to run.
        /// </summary>
        public uint InstanceCount {
            get {
                if (_advancedRange == null) return _taskApi.InstanceCount;
                else return _advancedRange.Count;
            }
        }

        /// <summary>
        /// The results include only the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        public string ResultsWhitelist {
            get {
                return _taskApi.ResultsWhitelist;
            }
            set { _taskApi.ResultsWhitelist = value; }
        }

        /// <summary>
        /// The results exclude all the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        public string ResultsBlacklist {
            get {
                return _taskApi.ResultsBlacklist;
            }
            set { _taskApi.ResultsBlacklist = value; }
        }


        /// <summary>
        /// The snapshots include only the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        public string SnapshotWhitelist {
            get {
                return _taskApi.SnapshotWhitelist;
            }
            set { _taskApi.SnapshotWhitelist = value; }
        }

        /// <summary>
        /// The snapshots exclude all the files matching that regular expression.
        /// Must be set before the submission.
        /// </summary>
        public string SnapshotBlacklist {
            get {
                return _taskApi.SnapshotBlacklist;
            }
            set { _taskApi.SnapshotBlacklist = value; }
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
            _taskApi.Shortname = shortname;
            _uri = "tasks/" + shortname;
        }

        /// <summary>
        /// Create a new task outside of a pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="profile">The task profile. If not specified, it must be given when the task is submitted.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        public QTask(Connection connection, string name, string profile, uint instanceCount = 0, string shortname = default(string)) : this(connection, name, profile, shortname) {
            _taskApi.InstanceCount = instanceCount;
        }

        /// <summary>
        /// Create a new task outside of a pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="profile">The task profile. If not specified, it must be given when the task is submitted.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        public QTask(Connection connection, string name, string profile, AdvancedRanges range, string shortname = default(string)) : this(connection, name, profile, shortname) {
            _advancedRange = range ?? new AdvancedRanges(null);
            _taskApi.AdvancedRanges = _advancedRange.ToString();
        }

        /// <summary>
        /// Create a new task inside an existing pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task name.</param>
        /// <param name="pool">The pool where this task will run.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        public QTask(Connection connection, string name, QPool pool, uint instanceCount = 0, string shortname = default(string)) : this(connection, name, (string)null, instanceCount, shortname) {
            _taskApi.PoolUuid = pool.Uuid.ToString();
        }

        /// <summary>
        /// Create a new task outside of a pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The task shortname.</param>
        /// <param name="pool">The pool where this task will run.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        public QTask(Connection connection, string name, QPool pool, AdvancedRanges range, string shortname = default(string)) : this(connection, name, pool, 0, shortname) {
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
        }

        internal async new Task<QTask> InitializeAsync(Connection qapi, TaskApi taskApi) {
            await base.InitializeAsync(qapi, taskApi);
             _uri = "tasks/" + taskApi.Uuid.ToString();
            if (_resources == null) _resources = new List<QAbstractStorage>();
            await SyncFromApiObjectAsync(taskApi);
            return this;
        }

        internal async static Task<QTask> CreateAsync(Connection qapi, TaskApi taskApi) {
            return await new QTask().InitializeAsync(qapi, taskApi);
        }

        #region public methods
        /// <summary>
        /// Set the a list of tags for the task.
        /// </summary>
        /// <param name="tags">Task tags.</param>
        public void SetTags(params String [] tags) {
            _taskApi.Tags = tags.Distinct().ToList();
        }

        /// <summary>
        /// Deprecated, use SetConstant.
        /// </summary>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value.</param>
        [Obsolete("use SetConstant")]
        public void AddConstant(string name, string value) {
            _taskApi.Constants.Add(new KeyValHelper(name, value));
        }

        /// <summary>
        /// Set a constant. If the constant already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value. If null, the constant is not added or deleted.</param>
        public void SetConstant(string name, string value) {
            // First, check if the constant already exists
            var c = _taskApi.Constants.Find(x => x.Key == name);
            if (c != null) {
                // Exists, just replace or delete
                if (value == null) _taskApi.Constants.Remove(c);
                else c.Value = value;
                return;
            }
            // Doesn't exist, just add
            if (value != null) _taskApi.Constants.Add(new KeyValHelper(name, value));
        }

        /// <summary>
        /// Set a constraint. If the constraint already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constraint name.</param>
        /// <param name="value">Constraint value. If null, the constraint is not added or deleted.</param>
        public void SetConstraint(string name, string value) {
            // First, check if the constraints already exists
            var c = _taskApi.Constraints.Find(x => x.Key == name);
            if (c != null) {
                // Exists, just replace or delete
                if (value == null) _taskApi.Constraints.Remove(c);
                else c.Value = value;
                return;
            }
            // Doesn't exist, just add
            if (value != null) _taskApi.Constraints.Add(new KeyValHelper(name, value));
        }

        /// <summary>
        /// Run this task.
        /// </summary>
        /// <param name="taskTimeoutSeconds">Optional number of second before abort is called.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task RunAsync(int taskTimeoutSeconds=-1, CancellationToken ct =default(CancellationToken)) {
            await SubmitAsync(ct, null, 0);
            await WaitAsync(taskTimeoutSeconds, ct);
            if (taskTimeoutSeconds > 0)
                await AbortAsync(ct);
        }

        /// <summary>
        /// Wait this task completion.
        /// </summary>
        /// <param name="taskTimeoutSeconds">Optional maximum number of second to wait for completion.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task WaitAsync(int taskTimeoutSeconds=-1, CancellationToken ct =default(CancellationToken)) {
           var start = DateTime.Now;
            while(!Completed) {
                await UpdateStatusAsync();
                var elasped = (DateTime.Now - start).Seconds;

                // loop timeout exit condition
                if(taskTimeoutSeconds > 0 || elasped > taskTimeoutSeconds) return;

                // loop delay
                await Task.Delay(10);
            }
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <returns></returns>
        public async Task SubmitAsync(string profile = null, bool autoCreateResultDisk = true) {
            await SubmitAsync(default(CancellationToken), profile, 0, autoCreateResultDisk);
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="instanceCount">How many times the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to 0.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <returns></returns>
        public async Task SubmitAsync(string profile, uint instanceCount = 0, bool autoCreateResultDisk = true) {
            await SubmitAsync(default(CancellationToken), profile, instanceCount, autoCreateResultDisk);
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="range">Which instance ids of the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <returns></returns>
        public async Task SubmitAsync(string profile, AdvancedRanges range, bool autoCreateResultDisk = true) {
            await SubmitAsync(default(CancellationToken), profile, range, autoCreateResultDisk);
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <returns></returns>
        public async Task SubmitAsync(CancellationToken cancellationToken, string profile = null, bool autoCreateResultDisk = true) {
            await SubmitAsync(cancellationToken, autoCreateResultDisk);
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="instanceCount">How many times the task will run. Optional if the instance count has already been defined in the constructor, it can be set to 0.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <returns></returns>
        public async Task SubmitAsync(CancellationToken cancellationToken, string profile, uint instanceCount = 0, bool autoCreateResultDisk = true) {
            if (profile != null) _taskApi.Profile = profile;
            if (instanceCount > 0) _taskApi.InstanceCount = instanceCount;
            await SubmitAsync(cancellationToken, autoCreateResultDisk);
        }

        /// <summary>
        /// Submit this task.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The task profile, if not running inside a pool. Optional if the profile has already been defined in the constructor or if the task is bound to a pool, profile must be null.</param>
        /// <param name="range">Which instance ids of the task have to run. Optional if the instance count has already been defined in the constructor, it can be set to null.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <returns></returns>
        public async Task SubmitAsync(CancellationToken cancellationToken, string profile, AdvancedRanges range, bool autoCreateResultDisk = true) {
            if (profile != null) _taskApi.Profile = profile;
            if (range != null) _advancedRange = range;
            _taskApi.AdvancedRanges = _advancedRange.ToString();
            await SubmitAsync(cancellationToken, autoCreateResultDisk);
        }

        internal async Task PreSubmitAsync(CancellationToken cancellationToken, bool autoCreateResultDisk = true) {
            if (_taskApi.InstanceCount > 0 && !String.IsNullOrEmpty(_taskApi.AdvancedRanges)) {
                throw new Exception("Can't use at the same time an instance count and a range.");
            }
            if (_taskApi.InstanceCount == 0 && String.IsNullOrEmpty(_taskApi.AdvancedRanges)) {
                throw new Exception("An instance count or a range must be set to submit a task.");
            }

            // Is a result disk defined?
            if (_results != null) {
                var resultsQDisk = _results as QDisk;
                if (resultsQDisk != null) {
                    _taskApi.ResultDisk = resultsQDisk.Shortname;
                } else {
                    var resultsQBucket = _results as QBucket;
                    if (resultsQBucket != null) {
                        _taskApi.ResultBucket = resultsQBucket.Shortname;
                        _taskApi.ResultDisk = null;
                    } else {
                        throw new Exception("Unknown IQStorage implementation");
                    }
                }
            }

            // Build the resource disk list
            _taskApi.ResourceDisks = new List<string>();
            foreach (var item in _resources) {
                var resQDisk = item as QDisk;
                if (resQDisk != null) {
                    _taskApi.ResourceDisks.Add(item.Shortname);
                    _taskApi.ResourceBuckets.Clear();
                } else {
                    var resQBucket = item as QBucket;
                    if (resQBucket != null) {
                        _taskApi.ResourceBuckets.Add(resQBucket.Shortname);
                        _taskApi.ResourceDisks.Clear();
                    } else {
                        throw new Exception("Unknown IQStorage implementation");
                    }
                }
            }

            if (_api.IsReadOnly) throw new Exception("Can't submit tasks, this connection is configured in read-only mode");
        }


        private async Task SubmitAsync(CancellationToken cancellationToken, bool autoCreateResultDisk = true) {
            await PreSubmitAsync(cancellationToken, autoCreateResultDisk);
            var response = await _api._client.PostAsJsonAsync<TaskApi>("tasks", _taskApi, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response);
            var result = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
            await PostSubmitAsync(result, cancellationToken);
        }

        internal async Task PostSubmitAsync(TaskApi result, CancellationToken cancellationToken) {
             // Update the task Uuid
            _taskApi.Uuid = result.Uuid;
            _uri = "tasks/" + _taskApi.Uuid.ToString();

            // Retrieve the task status once to update the other fields (result disk uuid etc..)
            await UpdateStatusAsync(cancellationToken);
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="updateDisksInfo">If set to true, the resources and results disk objects are also updated.</param>
        /// <returns></returns>
        public async Task UpdateStatusAsync(bool updateDisksInfo = true) {
            await UpdateStatusAsync(default(CancellationToken), updateDisksInfo);
        }

        private async Task SyncFromApiObjectAsync(TaskApi result) {
            _taskApi = result;
            if (_taskApi.AdvancedRanges != null) _advancedRange = new AdvancedRanges(_taskApi.AdvancedRanges);
            else _advancedRange = null;

            if (_resources.Count != _taskApi.ResourceDisks.Count) {
                _resources.Clear();
                foreach (var r in _taskApi.ResourceDisks) {
                    _resources.Add(await QDisk.CreateAsync(_api, r, create: false));
                }
            }

            if (_results == null && _taskApi.ResultDisk != null) {
                _results = await QDisk.CreateAsync(_api, _taskApi.ResultDisk, create: false);
            }
        }

        /// <summary>
        /// Update this task state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateDisksInfo">If set to true, the resources and results disk objects are also updated.</param>
        /// <returns></returns>
        public async Task UpdateStatusAsync(CancellationToken cancellationToken, bool updateDisksInfo = true) {
            var response = await _api._client.GetAsync(_uri, cancellationToken); // get task status
            await Utils.LookForErrorAndThrowAsync(_api._client, response);

            var result = await response.Content.ReadAsAsync<TaskApi>();
            await SyncFromApiObjectAsync(result);

            if (updateDisksInfo) {
                foreach (var r in _resources) {
                    await r.UpdateAsync(cancellationToken);
                }
                if (_results != null) {
                    await _results.UpdateAsync(cancellationToken);
                }
            }
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
        public string GetPublicHostForApplicationPort(UInt16 port) {
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
        public IEnumerable<UInt32> Instances {
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
        public QTaskInstanceStatus GetInstanceStatus(UInt32 instanceId) {
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

        private IEnumerable<T> GetResources<T>() where T : QAbstractStorage {
            foreach (var d in _resources) {
                if (d is T) yield return ((T)d);
            }
        }
        private T GetResults<T>() where T : QAbstractStorage {
            if (_results is T) return (T)_results;
            return default(T);
        }
        #endregion
    }

    /// <summary>
    /// Represents an unified an simplified version of a task instance status.
    /// </summary>
    public class QTaskInstanceStatus {
        public string State { get; private set; }
        public QTaskError Error { get; private set; }
        public float Progress { get; private set; }
        public float ExecutionTimeSec { get; private set; }
        public float ExecutionTimeGHz { get; private set; }
        public float WallTimeSec { get; private set; }
        public QTaskStatusPerRunningInstanceInfo RunningInstanceInfo {get; private set; }
        public QTaskCompletedInstance CompletedInstanceInfo { get; private set; }

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

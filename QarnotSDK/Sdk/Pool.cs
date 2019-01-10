using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK
{
    /// <summary>
    /// Pool states.
    /// </summary>
    public sealed class QPoolStates {
        /// <summary>
        /// The Pool has been submitted and is waiting for dispatch.
        /// </summary>
        public static readonly string Submitted = "Submitted";
        /// <summary>
        /// Some of the nodes have been dispatched.
        /// </summary>
        public static readonly string PartiallyDispatched = "PartiallyDispatched";
        /// <summary>
        /// All the nodes have been dispatched.
        /// </summary>
        public static readonly string FullyDispatched = "FullyDispatched";
        /// <summary>
        /// Some of the nodes are up an running.
        /// </summary>
        public static readonly string PartiallyExecuting = "PartiallyExecuting";
        /// <summary>
        /// All the nodes are up an running.
        /// </summary>
        public static readonly string FullyExecuting = "FullyExecuting";
        /// <summary>
        /// The pool has ended.
        /// </summary>
        public static readonly string Closed = "Closed";
    }

    /// <summary>
    /// This class manages pools life cycle: start, monitor, stop.
    /// </summary>
    public partial class QPool : AQPool
    {
        /// <summary>
        /// The pool shortname identifier. The shortname is provided by the user. It has to be unique.
        /// </summary>
        public string Shortname { get { return _poolApi.Shortname == null ? _poolApi.Uuid.ToString() : _poolApi.Shortname; } }
        /// <summary>
        /// The pool name.
        /// </summary>
        public string Name { get { return _poolApi.Name; } }
        /// <summary>
        /// The pool profile.
        /// </summary>
        public string Profile { get { return _poolApi.Profile; } }
        /// <summary>
        /// Qarnot resources disks or buckets bound to this pool.
        /// Can be set only before the pool start.
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
        /// Qarnot resources disks bound to this pool.
        /// Can be set only before the pool start.
        /// </summary>
        public IEnumerable<QDisk> ResourcesDisks {
            get {
                return GetResources<QDisk>();
            }
        }

        /// <summary>
        /// Qarnot resources buckets bound to this pool.
        /// Can be set only before the pool start.
        /// </summary>
        public IEnumerable<QBucket> ResourcesBuckets {
            get {
                return GetResources<QBucket>();
            }
        }

        /// <summary>
        /// Retrieve the pool state (see QPoolStates).
        /// Available only after the pool is started. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public string State { get { return _poolApi != null ? _poolApi.State : null; } }

        /// <summary>
        /// Retrieve the pool errors.
        /// </summary>
        public List<QPoolError> Errors {
            get {
                return _poolApi != null ? _poolApi.Errors : new List<QPoolError>();
            }
        }

        /// <summary>
        /// Retrieve the pool detailed status.
        /// Available only after the pool is started. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public QPoolStatus Status {
            get {
                return _poolApi != null ? _poolApi.Status : null;
            }
        }

        /// <summary>
        /// The pool creation date.
        /// Available only after the pool is started.
        /// </summary>
        public DateTime CreationDate { get { return _poolApi.CreationDate; } }
        /// <summary>
        /// How many nodes this pool has.
        /// </summary>
        public uint NodeCount { get { return _poolApi.InstanceCount; } }
        /// <summary>
        /// The custom pool tag list.
        /// </summary>
        public List<String> Tags {
            get {
                return _poolApi.Tags;
            }
        }

        /// <summary>
        /// The Pool constants.
        /// </summary>
        public Dictionary<string, string> Constants {
            get {
                var constants = _poolApi.Constants;
                if (constants == null)
                    return new Dictionary<string, string>();

                return constants.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        /// <summary>
        /// The pool constraints.
        /// </summary>
        public Dictionary<string, string> Constraints {
            get {
                var constraints = _poolApi.Constraints;
                if (constraints == null)
                    return new Dictionary<string, string>();

                return constraints.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        /// <summary>
        /// Create a new pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The pool name.</param>
        /// <param name="profile">The pool profile. If not specified, it must be given when the pool is started.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. If not specified, it must be given when the pool is started.</param>
        /// <param name="shortname">optional unique friendly shortname of the pool.</param>
        public QPool(Connection connection, string name, string profile = null, uint initialNodeCount = 0, string shortname = default(string))
            : base(connection, new PoolApi()) {
            _poolApi.Name = name;
            _poolApi.Profile = profile;
            _poolApi.InstanceCount = initialNodeCount;
            _resources = new List<QAbstractStorage>();

            if (shortname != default(string)) {
                _poolApi.Shortname = shortname;
                _uri = "pools/" + shortname;
            }
        }

        /// <summary>
        /// Create a pool object given an existing Uuid.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="uuid">The Uuid of an already existing pool.</param>
        public QPool(Connection connection, Guid uuid) : this(connection, uuid.ToString()) {
            _uri = "pools/" + uuid.ToString();
            _poolApi.Uuid = uuid;
        }

        internal QPool() {}

        internal QPool(Connection qapi, PoolApi poolApi) : base(qapi, poolApi) {
            _uri = "pools/" + poolApi.Uuid.ToString();
            if (_resources == null) _resources = new List<QAbstractStorage>();
            SyncFromApiObjectAsync(poolApi).Wait();
        }

        internal async new Task<QPool> InitializeAsync(Connection qapi, PoolApi poolApi) {
            await base.InitializeAsync(qapi, poolApi);
             _uri = "pools/" + poolApi.Uuid.ToString();
            if (_resources == null) _resources = new List<QAbstractStorage>();
            await SyncFromApiObjectAsync(poolApi);
            return this;
        }

        internal async static Task<QPool> CreateAsync(Connection qapi, PoolApi poolApi) {
            return await new QPool().InitializeAsync(qapi, poolApi);
        }

        #region public methods
        /// <summary>
        /// Set the a list of tags for the pool.
        /// </summary>
        /// <param name="tags">Pool tags.</param>
        public void SetTags(params String [] tags) {
            _poolApi.Tags = tags.Distinct().ToList();
        }

        /// <summary>
        /// Deprecated, use SetConstant.
        /// </summary>
        /// <param name="key">Constant name.</param>
        /// <param name="value">Constant value.</param>
        [Obsolete("use SetConstant")]
        public void AddConstant(string key, string value) {
            _poolApi.Constants.Add(new KeyValHelper(key, value));
        }

        /// <summary>
        /// Set a constant. If the constant already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value. If null, the constant is not added or deleted.</param>
        public void SetConstant(string name, string value) {
            // First, check if the constant already exists
            var c = _poolApi.Constants.Find(x => x.Key == name);
            if (c != null) {
                // Exists, just replace or delete
                if (value == null) _poolApi.Constants.Remove(c);
                else c.Value = value;
                return;
            }
            // Doesn't exist, just add
            if (value != null) _poolApi.Constants.Add(new KeyValHelper(name, value));
        }

        /// <summary>
        /// Set a constraint. If the constraint already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constraint name.</param>
        /// <param name="value">Constraint value. If null, the constraint is not added or deleted.</param>
        public void SetConstraint(string name, string value) {
            // First, check if the constraints already exists
            var c = _poolApi.Constraints.Find(x => x.Key == name);
            if (c != null) {
                // Exists, just replace or delete
                if (value == null) _poolApi.Constraints.Remove(c);
                else c.Value = value;
                return;
            }
            // Doesn't exist, just add
            if (value != null) _poolApi.Constraints.Add(new KeyValHelper(name, value));
        }

        /// <summary>
        /// Commit the local pool changes.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _api._client.PutAsJsonAsync<PoolApi>("pools", _poolApi, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response);
        }

        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        public async Task StartAsync(string profile = null, uint initialNodeCount = 0) {
            await StartAsync(default(CancellationToken), profile, initialNodeCount);
        }

        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken, string profile = null, uint initialNodeCount = 0) {
            _poolApi.ResourceDisks = new List<string>();
            foreach (var item in _resources) {
                var resQDisk = item as QDisk;
                if (resQDisk != null) {
                    _poolApi.ResourceDisks.Add(item.Shortname);
                    _poolApi.ResourceBuckets.Clear();
                } else {
                    var resQBucket = item as QBucket;
                    if (resQBucket != null) {
                        _poolApi.ResourceBuckets.Add(resQBucket.Shortname);
                        _poolApi.ResourceDisks.Clear();
                    } else {
                        throw new Exception("Unknown IQStorage implementation");
                    }
                }
            }

            if (profile != null) {
                _poolApi.Profile = profile;
            }
            if (initialNodeCount > 0) {
                _poolApi.InstanceCount = initialNodeCount;
            }

            if (_api.IsReadOnly) throw new Exception("Can't start pools, this connection is configured in read-only mode");
            var response = await _api._client.PostAsJsonAsync<PoolApi> ("pools", _poolApi, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response);

            // Update the pool Uuid
            var result = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
            _poolApi.Uuid = result.Uuid;
            _uri = "pools/" + _poolApi.Uuid.ToString();

            // Retrieve the pool status once to update the other fields (result disk uuid etc..)
            await UpdateStatusAsync(cancellationToken);
        }


        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="updateDisksInfo">If set to true, the resources disk objects are also updated.</param>
        /// <returns></returns>
        public async Task UpdateStatusAsync(bool updateDisksInfo = false) {
            await UpdateStatusAsync(default(CancellationToken), updateDisksInfo);
        }

        private async Task SyncFromApiObjectAsync(PoolApi result) {
            _poolApi = result;

            if (_resources.Count != _poolApi.ResourceDisks.Count) {
                _resources.Clear();
                foreach (var r in _poolApi.ResourceDisks) {
                    _resources.Add(await QDisk.CreateAsync(_api, r, create: false));
                }
                foreach (var r in _poolApi.ResourceBuckets) {
                    _resources.Add(await QBucket.CreateAsync(_api, r, create: false));
                }
            }
        }

        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateDisksInfo">If set to true, the resources disk objects are also updated.</param>
        /// <returns></returns>
        public async Task UpdateStatusAsync(CancellationToken cancellationToken, bool updateDisksInfo = false) {
            var response = await _api._client.GetAsync(_uri, cancellationToken); // get pool status
            await Utils.LookForErrorAndThrowAsync(_api._client, response);

            var result = await response.Content.ReadAsAsync<PoolApi>(cancellationToken);
            await SyncFromApiObjectAsync(result);

            if (updateDisksInfo) {
                foreach(var r in _resources) {
                    await r.UpdateAsync(cancellationToken);
                }
            }
        }

        /// <summary>
        /// Delete the pool. If the pool is running, the pool is closed and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        public override async Task DeleteAsync(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false)
        {
            try {
                if (_api.IsReadOnly) throw new Exception("Can't delete pools, this connection is configured in read-only mode");

                var response = await _api._client.DeleteAsync(_uri, cancellationToken);
                await Utils.LookForErrorAndThrowAsync(_api._client, response);

                if (purgeResources) await Task.WhenAll(Resources.Select(r => r.DeleteAsync(cancellationToken)));
            } catch (QarnotApiResourceNotFoundException ex) {
                if (failIfDoesntExist) throw ex;
            }
        }
        #endregion

        #region helpers
        /// <summary>
        /// Return the public host and port to establish an inbound connection to the master compute node (node 0) running your pool.
        /// Note: your profile have to define one or more inbound connection to support that feature. For example, the profile "docker-network-ssh"
        ///  defines a redirection to the ssh port 22. If you need inbound connections on a specific port, you can make a request to the support team.
        /// </summary>
        /// <param name="port">The port you want to access on the master compute node (node 0).</param>
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
        /// Get the status of a node given its node id.
        /// Note: the status of a node could be retrieved in the Status.RunningInstancesInfo.PerRunningInstanceInfo
        ///  structure. This method provides an
        ///  easy way to retrieve those information.
        /// </summary>
        /// <param name="nodeId">The id of the node.</param>
        /// <returns>The status of the node or null if not available.</returns>
        public QPoolNodeStatus GetNodeStatus(UInt32 nodeId) {
            if (_poolApi.Status != null && _poolApi.Status.RunningInstancesInfo != null && _poolApi.Status.RunningInstancesInfo.PerRunningInstanceInfo != null) {
                foreach (var j in _poolApi.Status.RunningInstancesInfo.PerRunningInstanceInfo) {
                    if (j.InstanceId == nodeId) {
                        return new QPoolNodeStatus(j);
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
        #endregion
    }

    /// <summary>
    /// Represents an unified an simplified version of a pool node status.
    /// </summary>
    public class QPoolNodeStatus {
        public string State { get; set; }
        public QPoolError Error { get; set; }
        public float Progress { get; set; }
        public float ExecutionTimeSec { get; set; }
        public float ExecutionTimeGHz { get; set; }
        public float WallTimeSec { get; set; }
        public QPoolStatusPerRunningInstanceInfo RunningNodeInfo { get; private set; }

        internal QPoolNodeStatus(QPoolStatusPerRunningInstanceInfo i) {
            // Phase is lowercase, convert the first letter to uppercase
            State = String.IsNullOrEmpty(i.Phase) ? "Unknown" : (i.Phase[0].ToString().ToUpper() + i.Phase.Substring(1));
            Error = null;
            Progress = i.Progress;
            ExecutionTimeGHz = i.ExecutionTimeGHz;
            ExecutionTimeSec = i.ExecutionTimeSec;
            WallTimeSec = 0;
            RunningNodeInfo = i;
        }
    }
}
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
        [InternalDataApiName(Name="Shortname")]
        public virtual string Shortname { get { return _poolApi.Shortname == null ? _poolApi.Uuid.ToString() : _poolApi.Shortname; } }
        /// <summary>
        /// The pool name.
        /// </summary>
        [InternalDataApiName(Name="Name")]
        public virtual string Name { get { return _poolApi.Name; } }
        /// <summary>
        /// The pool profile.
        /// </summary>
        [InternalDataApiName(Name="Profile")]
        public virtual string Profile { get { return _poolApi.Profile; } }
        /// <summary>
        /// Qarnot resources buckets bound to this pool.
        /// Can be set only before the pool start.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual List<QAbstractStorage> Resources {
            get {
                return _resources.Select(bucket => (QAbstractStorage) bucket).ToList();
            }
            set {
                _resources = QBucket.GetBucketsFromResources(value);
            }
        }

        private List<QBucket> _resources { get; set; }

        /// <summary>
        /// Qarnot resources buckets bound to this pool.
        /// Can be set only before the pool start.
        /// </summary>
        [InternalDataApiName(Name="ResourceBuckets", IsFilterable=false)]
        public virtual IEnumerable<QBucket> ResourcesBuckets {
            get {
                return _resources;
            }
        }

        /// <summary>
        /// Retrieve the pool state (see QPoolStates).
        /// Available only after the pool is started. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name="State")]
        public virtual string State { get { return _poolApi != null ? _poolApi.State : null; } }

        /// <summary>
        /// Retrieve the pool errors.
        /// </summary>
        [InternalDataApiName(Name="Errors", IsFilterable=false)]
        public virtual List<QPoolError> Errors {
            get {
                return _poolApi != null ? _poolApi.Errors : new List<QPoolError>();
            }
        }

        /// <summary>
        /// Retrieve the pool detailed status.
        /// Available only after the pool is started. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        [InternalDataApiName(Name="Status", IsFilterable=false)]
        public virtual QPoolStatus Status {
            get {
                return _poolApi != null ? _poolApi.Status : null;
            }
        }

        /// <summary>
        /// The pool creation date.
        /// Available only after the pool is started.
        /// </summary>
        [InternalDataApiName(Name="CreationDate")]
        public virtual DateTime CreationDate { get { return _poolApi.CreationDate; } }
        /// <summary>
        /// How many nodes this pool has.
        /// </summary>
        [InternalDataApiName(Name="InstanceCount")]
        public virtual uint NodeCount { get { return _poolApi.InstanceCount; } }
        /// <summary>
        /// The custom pool tag list.
        /// </summary>
        [InternalDataApiName(Name="Tags")]
        public virtual List<String> Tags {
            get {
                return _poolApi.Tags;
            }
        }

        private Dictionary<string, string> _constants { get; set; }

        /// <summary>
        /// The Pool constants.
        /// </summary>
        [InternalDataApiName(Name="Constants", IsFilterable=false)]
        public virtual Dictionary<string, string> Constants {
            get {
                if (_constants == null)
                    _constants = new Dictionary<string, string>();
                return _constants;
            }
        }

        private Dictionary<string, string> _constraints { get; set; }

        /// <summary>
        /// The pool constraints.
        /// </summary>
        [InternalDataApiName(Name="Constraints", IsFilterable=false)]
        public virtual Dictionary<string, string> Constraints {
            get {
                if (_constraints == null)
                    _constraints = new Dictionary<string, string>();
                return _constraints;
            }
        }

        #region Elastic properties
        /// <summary>
        /// Allow the automatic resize of the pool
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.IsElastic")]
        public virtual bool IsElastic {
            get { return _poolApi?.ElasticProperty?.IsElastic ?? false; }
            set {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.IsElastic = value;
            }
        }

        /// <summary>
        /// Minimum node number for the pool in elastic mode
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.MinTotalSlots")]
        public virtual uint ElasticMinimumTotalNodes {
            get { return _poolApi?.ElasticProperty?.MinTotalSlots ?? 0; }
            set
            {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.MinTotalSlots = value;
            }
        }

        /// <summary>
        /// Maximum node number for the pool in elastic mode
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.MaxTotalSlots")]
        public virtual uint ElasticMaximumTotalNodes {
            get { return _poolApi?.ElasticProperty?.MaxTotalSlots ?? 0; }
            set
            {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.MaxTotalSlots = value;
            }
        }

        /// <summary>
        /// Minimum idling node number.
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.MinIdleSlots")]
        public virtual uint ElasticMinimumIdlingNodes {
            get { return _poolApi?.ElasticProperty?.MinIdleSlots ?? 0; }
            set
            {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.MinIdleSlots = value;
            }
        }

        /// <summary>
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.ResizePeriod")]
        public virtual uint ElasticResizePeriod {
            get { return _poolApi?.ElasticProperty?.ResizePeriod ?? 0; }
            set
            {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.ResizePeriod = value;
            }
        }

        /// <summary>
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.RampResizeFactor")]
        public virtual float ElasticResizeFactor {
            get { return _poolApi?.ElasticProperty?.RampResizeFactor ?? 0; }
            set
            {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.RampResizeFactor = value;
            }
        }

        /// <summary>
        /// </summary>
        [InternalDataApiName(Name="ElasticProperty.MinIdleTimeSeconds")]
        public virtual uint ElasticMinimumIdlingTime {
            get { return _poolApi?.ElasticProperty?.MinIdleTimeSeconds ?? 0; }
            set
            {
                if (_poolApi.ElasticProperty == null)
                {
                    _poolApi.ElasticProperty = new QPoolElasticProperty();
                }

                _poolApi.ElasticProperty.MinIdleTimeSeconds = value;
            }
        }

        #endregion

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
            _resources = new List<QBucket>();
            _constants = new Dictionary<string, string>();
            _constraints = new Dictionary<string, string>();

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
            if (_resources == null) _resources = new List<QBucket>();
            if (_constants == null) _constants = new Dictionary<string, string>();
            if (_constraints == null) _constraints = new Dictionary<string, string>();
            SyncFromApiObjectAsync(poolApi).Wait();
        }

        internal async new Task<QPool> InitializeAsync(Connection qapi, PoolApi poolApi) {
            await base.InitializeAsync(qapi, poolApi);
            _uri = "pools/" + poolApi.Uuid.ToString();
            if (_resources == null) _resources = new List<QBucket>();
            if (_constants == null) _constants = new Dictionary<string, string>();
            if (_constraints == null) _constraints = new Dictionary<string, string>();
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
        public virtual void SetTags(params String [] tags) {
            _poolApi.Tags = tags.Distinct().ToList();
        }

        /// <summary>
        /// Deprecated, use SetConstant.
        /// </summary>
        /// <param name="key">Constant name.</param>
        /// <param name="value">Constant value.</param>
        [Obsolete("use SetConstant")]
        public virtual void AddConstant(string key, string value) {
	    this.SetConstant(key, value);
        }

        /// <summary>
        /// Set a constant. If the constant already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constant name.</param>
        /// <param name="value">Constant value. If null, the constant is deleted.</param>
        public virtual void SetConstant(string name, string value) {
            // First, check if the constant already exists
	    if (_constants.ContainsKey(name) && value == null) {
                // Just delete the constant
                _constants.Remove(name);
                return;
            }
            // Add or update the constant
            if (value != null) _constants[name] = value;
        }

        /// <summary>
        /// Set a constraint. If the constraint already exists, it is replaced (or removed if value is null).
        /// </summary>
        /// <param name="name">Constraint name.</param>
        /// <param name="value">Constraint value. If null, the constraint is deleted.</param>
        public virtual void SetConstraint(string name, string value) {
            // First, check if the constraints already exists
	    if (_constraints.ContainsKey(name) && value == null) {
                // Delete a constraint
                _constraints.Remove(name);
                return;
            }
            // Add or update the constraint
            if (value != null) _constraints[name] = value;
        }

        /// <summary>
        /// Commit the local pool changes.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public virtual async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            // build the constants
            _poolApi.Constants = new List<KeyValHelper>();
            foreach(var c in _constants) { _poolApi.Constants.Add(new KeyValHelper(c.Key, c.Value)); }

            // build the constraints
            _poolApi.Constraints = new List<KeyValHelper>();
            foreach(var c in _constraints) { _poolApi.Constraints.Add(new KeyValHelper(c.Key, c.Value)); }

            using (var response = await _api._client.PutAsJsonAsync<PoolApi>(_uri, _poolApi, cancellationToken))
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
        }

        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        public virtual async Task StartAsync(string profile = null, uint initialNodeCount = 0) {
            await StartAsync(default(CancellationToken), profile, initialNodeCount);
        }

        /// <summary>
        /// Start the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="profile">The pool profile. Optional if it has already been defined in the constructor.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. Optional if it has already been defined in the constructor.</param>
        /// <returns></returns>
        public virtual async Task StartAsync(CancellationToken cancellationToken, string profile = null, uint initialNodeCount = 0) {
            // build the constants
            _poolApi.Constants = new List<KeyValHelper>();
            foreach(var c in _constants) { _poolApi.Constants.Add(new KeyValHelper(c.Key, c.Value)); }

            // build the constraints
            _poolApi.Constraints = new List<KeyValHelper>();
            foreach(var c in _constraints) { _poolApi.Constraints.Add(new KeyValHelper(c.Key, c.Value)); }

            _poolApi.ResourceBuckets = new List<string>();
            foreach (var item in _resources) {
                if (item != null) {
                    _poolApi.ResourceBuckets.Add(item.Shortname);
                } else {
                    throw new Exception("Unknown null bucket");
                }
            }

            if (profile != null) {
                _poolApi.Profile = profile;
            }
            if (initialNodeCount > 0) {
                _poolApi.InstanceCount = initialNodeCount;
            }

            if (_api.IsReadOnly) throw new Exception("Can't start pools, this connection is configured in read-only mode");
            using (var response = await _api._client.PostAsJsonAsync<PoolApi> ("pools", _poolApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response);

                // Update the pool Uuid
                var result = await response.Content.ReadAsAsync<PoolApi>(cancellationToken);
                _poolApi.Uuid = result.Uuid;
                _uri = "pools/" + _poolApi.Uuid.ToString();
            }

            // Retrieve the pool status once to update the other fields (result bucket uuid etc..)
            await UpdateStatusAsync(cancellationToken);
        }


        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="updateQBucketsInfo">If set to true, the resources qbucket objects are also updated.</param>
        /// <returns></returns>
        public virtual async Task UpdateStatusAsync(bool updateQBucketsInfo = false) {
            await UpdateStatusAsync(default(CancellationToken), updateQBucketsInfo);
        }

        private async Task SyncFromApiObjectAsync(PoolApi result) {
            _poolApi = result;

            // update constants
            _constants = result.Constants?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>(); 

            // update constraints
            _constraints = result.Constraints?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new Dictionary<string, string>(); 

            var newResourcesCount = 0;
            if (_poolApi.ResourceBuckets != null) newResourcesCount += _poolApi.ResourceBuckets.Count;

            if (_resources.Count != newResourcesCount) {
                _resources.Clear();

                if (_poolApi.ResourceBuckets != null) {
                    foreach (var r in _poolApi.ResourceBuckets) {
                        _resources.Add(await QBucket.CreateAsync(_api, r, create: false));
                    }
                }
            }
        }

        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="updateQBucketsInfo">If set to true, the resources bucket objects are also updated.</param>
        /// <returns></returns>
        public virtual async Task UpdateStatusAsync(CancellationToken cancellationToken, bool updateQBucketsInfo = false) {
            using (var response = await _api._client.GetAsync(_uri, cancellationToken)) // get pool status
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
                var result = await response.Content.ReadAsAsync<PoolApi>(cancellationToken);
                await SyncFromApiObjectAsync(result);
            }

            if (updateQBucketsInfo) {
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

                using (var response = await _api._client.DeleteAsync(_uri, cancellationToken))
                    await Utils.LookForErrorAndThrowAsync(_api._client, response);

                if (purgeResources) await Task.WhenAll(_resources.Select(r => r.DeleteAsync(cancellationToken)));
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
        /// Get the status of a node given its node id.
        /// Note: the status of a node could be retrieved in the Status.RunningInstancesInfo.PerRunningInstanceInfo
        ///  structure. This method provides an
        ///  easy way to retrieve those information.
        /// </summary>
        /// <param name="nodeId">The id of the node.</param>
        /// <returns>The status of the node or null if not available.</returns>
        public virtual QPoolNodeStatus GetNodeStatus(UInt32 nodeId) {
            if (_poolApi.Status != null && _poolApi.Status.RunningInstancesInfo != null && _poolApi.Status.RunningInstancesInfo.PerRunningInstanceInfo != null) {
                foreach (var j in _poolApi.Status.RunningInstancesInfo.PerRunningInstanceInfo) {
                    if (j.InstanceId == nodeId) {
                        return new QPoolNodeStatus(j);
                    }
                }
            }
            return null;
        }
        #endregion
    }

    /// <summary>
    /// Represents an unified an simplified version of a pool node status.
    /// </summary>
    public class QPoolNodeStatus {
        /// <summary>
        /// Retrieve the instance state.
        /// </summary>
        public virtual string State { get; set; }

        /// <summary>
        /// Retrieve the instance error
        /// </summary>
        public virtual QPoolError Error { get; set; }

        /// <summary>
        /// Retrieve the instance progress indicator
        /// </summary>
        public virtual float Progress { get; set; }

        /// <summary>
        /// Instance execution time(in seconds).
        /// </summary>
        public virtual float ExecutionTimeSec { get; set; }

        /// <summary>
        /// Instance execution time frequency(in seconds.ghz).
        /// </summary>
        public virtual float ExecutionTimeGHz { get; set; }

        /// <summary>
        /// Retrieve the instance wall time(in seconds).
        /// </summary>
        public virtual float WallTimeSec { get; set; }

        /// <summary>
        /// Informations about running instances (see QPoolStatusPerRunningInstanceInfo)
        /// </summary>
        public virtual QPoolStatusPerRunningInstanceInfo RunningNodeInfo { get; private set; }

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

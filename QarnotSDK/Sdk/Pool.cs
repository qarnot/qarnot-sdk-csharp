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
    public partial class QPool
    {
        private readonly Connection _api;
        private PoolApi _poolApi;
        private string _uri;

        /// <summary>
        /// The inner Connection object.
        /// </summary>
        public Connection Connection { get { return _api; } }
        /// <summary>
        /// The pool unique identifier. The Uuid is generated by the Api when the pool is started.
        /// </summary>
        public Guid Uuid { get { return _poolApi.Uuid; } }
        /// <summary>
        /// The pool shortname identifier. The shortname is provided by the user. It has to be unique.
        /// </summary>
        public string Shortname { get { return _api.HasShortnameFeature ? (_poolApi.Shortname == null ? _poolApi.Uuid.ToString() : _poolApi.Shortname) : _poolApi.Name; } }
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
        public List<QAbstractStorage> Resources { get; set; }
        /// <summary>
        /// Qarnot resources disks bound to this pool.
        /// Can be set only before the pool start.
        /// </summary>
        public IEnumerable<QDisk> ResourcesDisks { get { return GetResources<QDisk>(); } }
        /// <summary>
        /// Qarnot resources buckets bound to this pool.
        /// Can be set only before the pool start.
        /// </summary>
        public IEnumerable<QBucket> ResourcesBuckets { get { return GetResources<QBucket>(); } }
        /// <summary>
        /// Retrieve the pool state (see QPoolStates).
        /// Available only after the pool is started. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public string State { get { return _poolApi != null ? _poolApi.State : null; } }
        /// <summary>
        /// Retrieve the pool detailed status.
        /// Available only after the pool is started. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        public QPoolStatus Status { get { return _poolApi != null ? _poolApi.Status : null; } }
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
        public List<String> Tags { get { return _poolApi.Tags; } }

        /// <summary>
        /// Create a new pool.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="shortname">The pool shortname.</param>
        /// <param name="profile">The pool profile. If not specified, it must be given when the pool is started.</param>
        /// <param name="initialNodeCount">The number of compute nodes this pool will have. If not specified, it must be given when the pool is started.</param>
        public QPool(Connection connection, string shortname, string profile = null, uint initialNodeCount = 0) {
            _api = connection;
            _poolApi = new PoolApi();
            _poolApi.Name = shortname;
            _poolApi.Profile = profile;
            _poolApi.InstanceCount = initialNodeCount;
            Resources = new List<QAbstractStorage>();

            if (_api.HasShortnameFeature) {
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

        internal QPool(Connection qapi, PoolApi poolApi) {
            _api = qapi;
            _uri = "pools/" + poolApi.Uuid.ToString();
            if (Resources == null) Resources = new List<QAbstractStorage>();
            SyncFromApiObject(poolApi);
        }

        #region workaround
        // Will be removed once the 'shortname' is implemented on the api side
        internal async Task ApiWorkaround_EnsureUriAsync(bool mustExist, CancellationToken cancellationToken) {
            if (_api.HasShortnameFeature) {
                // No workaround needed
                return;
            }

            if (mustExist) {
                // The pool Uri must exist, so if Uri is null, fetch the pool by name
                if (_uri != null) {
                    return;
                }

                var result = await _api.RetrievePoolByNameAsync(_poolApi.Name, cancellationToken);
                if (result == null) {
                    throw new QarnotApiResourceNotFoundException("pool " + _poolApi.Name + " doesn't exist", null);
                }
                _poolApi.Uuid = result.Uuid;
                _uri = "pools/" + _poolApi.Uuid.ToString();
            } else {
                if (_uri != null) {
                    // We have an Uri, check if it's still valid
                    try {
                        var response = await _api._client.GetAsync(_uri, cancellationToken); // get pool status
                        await Utils.LookForErrorAndThrowAsync(_api._client, response);
                        // no error, the pool still exists
                        throw new QarnotApiResourceAlreadyExistsException("pool " + _poolApi.Name + " already exists", null);
                    } catch(QarnotApiResourceNotFoundException) {
                        // OK, not running
                    }
                } else {
                    // We don't have any Uri, check if the pool name exists
                    var result = await _api.RetrievePoolByNameAsync(_poolApi.Name, cancellationToken);
                    if (result != null) {
                        throw new QarnotApiResourceAlreadyExistsException("pool " + _poolApi.Name + " already exists", null);
                    }
                }
                _poolApi.Uuid = new Guid();
                _uri = null;
            }
        }
        #endregion

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
        /// <param name="name">Constant name.</param>
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
            await ApiWorkaround_EnsureUriAsync(false, cancellationToken);

            _poolApi.ResourceDisks = new List<string>();
            foreach (var item in Resources) {
                var resQDisk = item as QDisk;
                if (resQDisk != null) {
                    if (_api.HasDiskShortnameFeature) {
                        _poolApi.ResourceDisks.Add(item.Shortname);
                        _poolApi.ResourceBuckets.Clear();
                    } else {
                        if (resQDisk.Uuid == Guid.Empty) await resQDisk.UpdateAsync(cancellationToken);
                        _poolApi.ResourceDisks.Add(resQDisk.Uuid.ToString());
                        _poolApi.ResourceBuckets.Clear();
                    }
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

            // Update the task Uuid
            var result = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
            _poolApi.Uuid = result.Uuid;
            _uri = "pools/" + _poolApi.Uuid.ToString();

            // Retrieve the task status once to update the other fields (result disk uuid etc..)
            await UpdateStatusAsync(cancellationToken);
        }

        /// <summary>
        /// Stop the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await ApiWorkaround_EnsureUriAsync(true, cancellationToken);

            if (_api.IsReadOnly) throw new Exception("Can't stop pools, this connection is configured in read-only mode");
            var response = await _api._client.DeleteAsync(_uri, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response);
        }

        /// <summary>
        /// Update this pool state and status.
        /// </summary>
        /// <param name="updateDisksInfo">If set to true, the resources disk objects are also updated.</param>
        /// <returns></returns>
        public async Task UpdateStatusAsync(bool updateDisksInfo = false) {
            await UpdateStatusAsync(default(CancellationToken), updateDisksInfo);
        }

        private void SyncFromApiObject(PoolApi result) {
            _poolApi = result;

            if (Resources.Count != _poolApi.ResourceDisks.Count) {
                Resources.Clear();
                foreach (var r in _poolApi.ResourceDisks) {
                    Resources.Add(new QDisk(_api, new Guid(r)));
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
            await ApiWorkaround_EnsureUriAsync(true, cancellationToken);

            var response = await _api._client.GetAsync(_uri, cancellationToken); // get pool status
            await Utils.LookForErrorAndThrowAsync(_api._client, response);

            var result = await response.Content.ReadAsAsync<PoolApi>(cancellationToken);
            SyncFromApiObject(result);

            if (updateDisksInfo) {
                foreach(var r in Resources) {
                    await r.UpdateAsync(cancellationToken);
                }
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
            foreach (var d in Resources) {
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
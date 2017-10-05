using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace QarnotSDK
{
    public class QPool
    {
        private readonly Connection _api;
        private PoolApi _poolApi;
        private string _uri;

        public List<QDisk> Resources { get; set; }

        public QDisk Results { get; set; }

        public QPoolStatus Status { get { return _poolApi != null ? _poolApi.Status : null; } }

        public string State { get { return _poolApi != null ? _poolApi.State : null; } }

        public Guid Uuid { get { return _poolApi.Uuid; } }

        public string Shortname { get { return _api.HasShortnameFeature ? _poolApi.Shortname:_poolApi.Name; } }

        public string Name { get { return _poolApi.Name; } }

        public QPool(Connection api, string name, string profile = null, uint initialNodeCount = 0) {
            _api = api;
            _poolApi = new PoolApi();
            _poolApi.Name = name;
            _poolApi.Profile = profile;
            _poolApi.InstanceCount = initialNodeCount;
            Resources = new List<QDisk>();

            if (_api.HasShortnameFeature) {
                _poolApi.Shortname = name;
                _uri = "pools/" + name;
            }
        }

        public QPool(Connection api, Guid guid) : this(api, guid.ToString()) {
            _uri = "pools/" + guid.ToString();
        }

        internal QPool(Connection qapi, PoolApi poolApi) {
            _api = qapi;
            _poolApi = poolApi;
            _uri = "pools/" + _poolApi.Uuid.ToString();
            Resources = new List<QDisk>();
        }

        #region workaround
        // Will be removed once the 'shortname' is implemented on the api side
        internal async Task ApiWorkaround_EnsureUriAsync(bool mustExist) {
            if (_api.HasShortnameFeature) {
                // No workaround needed
                return;
            }

            if (mustExist) {
                // The pool uri must exist, so if uri is null, fetch the pool by name
                if (_uri != null) {
                    return;
                }

                var result = await _api.RetrievePoolByNameAsync(_poolApi.Name);
                if (result == null) {
                    throw new QarnotApiResourceNotFoundException("pool " + _poolApi.Name + " doesn't exist", null);
                }
                _poolApi.Uuid = result.Uuid;
                _uri = "pools/" + _poolApi.Uuid.ToString();
            } else {
                if (_uri != null) {
                    // We have an uri, check if it's still valid
                    try {
                        var response = await _api._client.GetAsync(_uri); // get pool status
                        await Utils.LookForErrorAndThrow(_api._client, response);
                        // no error, the pool still exists
                        throw new QarnotApiResourceAlreadyExistsException("pool " + _poolApi.Name + " already exists", null);
                    } catch(QarnotApiResourceNotFoundException) {
                        // OK, not running
                    }                     
                } else {
                    // We don't have any uri, check if the pool name exists
                    var result = await _api.RetrievePoolByNameAsync(_poolApi.Name);
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
        public void AddConstant(string key, string value)
        {
            _poolApi.Constants.Add (new KeyValHelper (key, value));
        }

        public async Task StartAsync(string profile = null, uint initialNodeCount = 0, bool autoCreateResultDisk = true)
        {
            await ApiWorkaround_EnsureUriAsync(false);

            if (Results != null) {
                if (_api.HasDiskShortnameFeature) {
                    _poolApi.ResultDisk = Results.Shortname;
                } else {
                    if (Results.Uuid == Guid.Empty) {
                        if (!autoCreateResultDisk) await Results.UpdateAsync();
                        else await Results.CreateAsync(true);
                    }
                    _poolApi.ResultDisk = Results.Uuid.ToString();
                }
            }
            _poolApi.ResourceDisks = new List<string>();
            foreach (var item in Resources) {
                if (_api.HasDiskShortnameFeature) {
                    _poolApi.ResourceDisks.Add(item.Shortname);
                } else {
                    if (item.Uuid == Guid.Empty) await item.UpdateAsync();
                    _poolApi.ResourceDisks.Add(item.Uuid.ToString());
                }
            }

            if (profile != null) {
                _poolApi.Profile = profile;
            }
            if (initialNodeCount > 0) {
                _poolApi.InstanceCount = initialNodeCount;
            }

            var response = await _api._client.PostAsJsonAsync<PoolApi> ("pools", _poolApi);
            await Utils.LookForErrorAndThrow(_api._client, response);

            // Update the task Uuid
            var result = await response.Content.ReadAsAsync<TaskApi>();
            _poolApi.Uuid = result.Uuid;
            _uri = "pools/" + _poolApi.Uuid.ToString();

            // Retrieve the task status once to update the other fields (result disk uuid etc..)
            await UpdateStatusAsync();
        }
                       
        public async Task UpdateStatusAsync(bool updateDisksInfo = true) {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.GetAsync(_uri); // get pool status
            await Utils.LookForErrorAndThrow(_api._client, response);

            var result = await response.Content.ReadAsAsync<PoolApi>();
            _poolApi = result;

            if (Resources.Count != _poolApi.ResourceDisks.Count) {
                Resources.Clear();
                foreach(var r in _poolApi.ResourceDisks) {
                    Resources.Add(new QDisk(_api, new Guid(r)));
                }
            }

            if (Results == null && _poolApi.ResultDisk != null) {
                Results = new QDisk(_api, new Guid(_poolApi.ResultDisk));
            }

            if (updateDisksInfo) {
                foreach(var r in Resources) {
                    await r.UpdateAsync();
                }
                if (Results != null) {
                    await Results.UpdateAsync();
                }
            }
        }

        public async Task StopAsync()
        {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.DeleteAsync(_uri);
            await Utils.LookForErrorAndThrow(_api._client, response);
        }

        #endregion

        public string GetPublicHostForApplicationPort(UInt16 port) {
            if (Status != null && Status.RunningInstancesInfo != null) {
                var instances = Status.RunningInstancesInfo.PerRunningInstanceInfo;
                if (instances != null && instances.Count > 0) {
                    foreach (var af in instances[0].ActiveForwards) {
                        if (af.ApplicationPort == 3389) {
                            return String.Format("{0}:{1}", af.ForwarderHost, af.ForwarderPort);
                        }
                    }
                }
            }
            return null;
        }
    }

    public class QPoolStatusActiveForwards {
        public UInt16 ApplicationPort { get; set; }
        public UInt16 ForwarderPort { get; set; }
        public string ForwarderHost { get; set; }
        public QPoolStatusActiveForwards() {
        }
    }

    public class QPoolStatusPerRunningInstanceInfo {
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
        public List<QPoolStatusActiveForwards> ActiveForwards { get; set; }

        public QPoolStatusPerRunningInstanceInfo() {
            ActiveForwards = new List<QPoolStatusActiveForwards>();
        }
    }

    public class QPoolStatusRunningInstancesInfo {
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
        public List<QPoolStatusPerRunningInstanceInfo> PerRunningInstanceInfo { get; set; }

        public QPoolStatusRunningInstancesInfo() {
            PerRunningInstanceInfo = new List<QPoolStatusPerRunningInstanceInfo>();
        }
    }

    public class QPoolStatus {
        public float DownloadProgress { get; set; }
        public float ExecutionProgress { get; set; }
        public float UploadProgress { get; set; }
        public uint FrameCount { get; set; }
        public long DownloadTimeSec { get; set; }
        public long ExecutionTimeSec { get; set; }
        public long UploadTimeSec { get; set; }
        public string SucceededRange { get; set; }
        public string ExecutedRange { get; set; }
        public string FailedRange { get; set; }
        public QPoolStatusRunningInstancesInfo RunningInstancesInfo { get; set; }

        public QPoolStatus() {
        }
    }

    internal class PoolApi {
        public override string ToString() {
            return string.Format("[PoolApi: Name={0}, Profile={1}, InstanceCount={2}, ResultDisk={3}, State={4}, SnapshotInterval={5}, ResultsCount={6}, CreationDate={7}, Uuid={8}]", Name, Profile, InstanceCount, ResultDisk, State, SnapshotInterval, ResultsCount, CreationDate, Uuid);
        }

        public string Name { get; set; }
        public string Profile { get; set; }
        public uint InstanceCount { get; set; }
        public List<string> ResourceDisks { get; set; }
        public string ResultDisk { get; set; }
        public string State { get; set; }
        public int SnapshotInterval { get; set; }
        public uint ResultsCount { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public Guid Uuid { get; set; }
        public string Shortname { get; set; }
        public QPoolStatus Status { get; set; }

        public PoolApi() {
            Constants = new List<KeyValHelper>();
            ResourceDisks = new List<String>();
        }
    }
}
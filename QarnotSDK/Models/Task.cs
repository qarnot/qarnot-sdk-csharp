using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System;

namespace QarnotSDK {
    public class QTask {
        private readonly Connection _api;
        private TaskApi _taskApi;
        private string _outDir = null;
        private string _uri = null;
        private CancellationTokenSource _submitSource = null;
        private CancellationTokenSource _snapshotSource = null;
        private CancellationTokenSource _resumeSource = null;

        public List<QDisk> Resources { get; set; }

        public QDisk Results { get; set; }

        public QTaskStatus Status { get { return _taskApi != null ? _taskApi.Status : null; } }

        public string State { get { return _taskApi != null ? _taskApi.State : null; } }

        public Guid Uuid { get { return _taskApi.Uuid; } }

        public string Shortname { get { return _api.HasShortnameFeature ? _taskApi.Shortname : _taskApi.Name; } }

        public string Name { get { return _taskApi.Name; } }

        public delegate void SnapshotResultsAvailableEventHandler(QTask sender);

        public event SnapshotResultsAvailableEventHandler SnapshotResultsAvailable;

        internal QTask() {
            Resources = new List<QDisk>();
        }

        public QTask(Connection api, string name, string profile = null, uint instanceCount = 0) {
            _api = api;
            _taskApi = new TaskApi();
            _taskApi.Name = name;
            _taskApi.Profile = profile;
            _taskApi.InstanceCount = instanceCount;
            Resources = new List<QDisk>();

            if (_api.HasShortnameFeature) {
                _taskApi.Shortname = name;
                _uri = "tasks/" + name;
            }
        }

        public QTask(Connection api, string name, QPool pool, uint instanceCount = 0) {
            _api = api;
            _taskApi = new TaskApi();
            _taskApi.Name = name;
            _taskApi.Profile = null;
            _taskApi.PoolUuid = pool.Uuid.ToString();
            _taskApi.InstanceCount = instanceCount;
            Resources = new List<QDisk>();

            if (_api.HasShortnameFeature) {
                _taskApi.Shortname = name;
                _uri = "tasks/" + name;
            }
        }

        public QTask(Connection api, Guid guid) : this(api, guid.ToString()) {
            _uri = "tasks/" + guid.ToString();
        }

        internal QTask(Connection qapi, TaskApi taskApi) {
            _api = qapi;
            _taskApi = taskApi;
            _uri = "tasks/" + _taskApi.Uuid.ToString();
        }

        #region workaround
        // Will be removed once the uniq custom id is implemented on the api side
        internal async Task ApiWorkaround_EnsureUriAsync(bool mustExist) {
            if (_api.HasShortnameFeature) {
                // No workaround needed
                return;
            }

            if (mustExist) {
                // The task uri must exist, so if uri is null, fetch the task by name
                if (_uri != null) {
                    return; // ok
                }

                var result = await _api.RetrieveTaskByNameAsync(_taskApi.Name);
                if (result == null) {
                    throw new QarnotApiResourceNotFoundException("task " + _taskApi.Name + " doesn't exist", null);
                }
                _taskApi.Uuid = result.Uuid;
                _uri = "tasks/" + _taskApi.Uuid.ToString();
            } else {
                // The task must NOT exist
                if (_uri != null) {
                    // We have an uri, check if it's still valid
                    try {
                        var response = await _api._client.GetAsync(_uri); // get task status
                        await Utils.LookForErrorAndThrow(_api._client, response);
                        // no error, the task still exists
                        throw new QarnotApiResourceAlreadyExistsException("task " + _taskApi.Name + " already exists", null);
                    } catch (QarnotApiResourceNotFoundException) {
                        // OK, not running
                    }
                } else {
                    // We don't have any uri, check if the task name exists
                    var result = await _api.RetrieveTaskByNameAsync(_taskApi.Name);
                    if (result != null) {
                        throw new QarnotApiResourceAlreadyExistsException("task " + _taskApi.Name + " already exists", null);
                    }
                }
                _taskApi.Uuid = new Guid();
                _uri = null;
            }
        }
        #endregion

        #region public methods
        public void AddConstant(string key, string value) {
            _taskApi.Constants.Add(new KeyValHelper(key, value));
        }

        public async Task SubmitAsync(string profile = null, uint instanceCount = 0, bool autoCreateResultDisk = true) {
            await ApiWorkaround_EnsureUriAsync(false);

            if (Results != null) {
                if (_api.HasDiskShortnameFeature) {
                    _taskApi.ResultDisk = Results.Shortname;
                } else {
                    if (Results.Uuid == Guid.Empty) {
                        if (!autoCreateResultDisk) await Results.UpdateAsync();
                        else await Results.CreateAsync(true);
                    }
                    _taskApi.ResultDisk = Results.Uuid.ToString();
                }
            }
            _taskApi.ResourceDisks = new List<string>();
            foreach (var item in Resources) {
                if (_api.HasDiskShortnameFeature) {
                    _taskApi.ResourceDisks.Add(item.Shortname);
                } else {
                    if (item.Uuid == Guid.Empty) await item.UpdateAsync();
                    _taskApi.ResourceDisks.Add(item.Uuid.ToString());
                }
            }

            if (profile != null) {
                _taskApi.Profile = profile;
            }
            if (instanceCount > 0) {
                _taskApi.InstanceCount = instanceCount;
            }

            var response = await _api._client.PostAsJsonAsync<TaskApi>("tasks", _taskApi);
            await Utils.LookForErrorAndThrow(_api._client, response);

            // Update the task Uuid
            var result = await response.Content.ReadAsAsync<TaskApi>();
            _taskApi.Uuid = result.Uuid;
            _uri = "tasks/" + _taskApi.Uuid.ToString();

            // Retrieve the task status once to update the other fields (result disk uuid etc..)
            await UpdateStatusAsync();
        }

        public async Task<string> FreshStdoutAsync() {
            using (MemoryStream ms = new MemoryStream()) {
                await CopyFreshStdoutToAsync(ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task CopyFreshStdoutToAsync(Stream s) {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.PostAsync(_uri + "/stdout", null);
            await Utils.LookForErrorAndThrow(_api._client, response);

            await response.Content.CopyToAsync(s);
        }

        public async Task<string> StdoutAsync() {
            using (MemoryStream ms = new MemoryStream()) {
                await CopyStdoutToAsync(ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task CopyStdoutToAsync(Stream s) {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.GetAsync(_uri + "/stdout");
            await Utils.LookForErrorAndThrow(_api._client, response);

            await response.Content.CopyToAsync(s);
        }

        public async Task<string> FreshStderrAsync() {
            using (MemoryStream ms = new MemoryStream()) {
                await CopyFreshStderrToAsync(ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task CopyFreshStderrToAsync(Stream s) {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.PostAsync(_uri + "/stderr", null);
            await Utils.LookForErrorAndThrow(_api._client, response);

            await response.Content.CopyToAsync(s);
        }

        public async Task<string> StderrAsync() {
            using (MemoryStream ms = new MemoryStream()) {
                await CopyStdoutToAsync(ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task CopyStderrToAsync(Stream s) {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.GetAsync(_uri + "/stderr");
            await Utils.LookForErrorAndThrow(_api._client, response);

            await response.Content.CopyToAsync(s);
        }

        public async Task UpdateStatusAsync(bool updateDisksInfo = true) {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.GetAsync(_uri); // get task status
            await Utils.LookForErrorAndThrow(_api._client, response);

            var result = await response.Content.ReadAsAsync<TaskApi>();
            _taskApi = result;

            if (Resources.Count != _taskApi.ResourceDisks.Count) {
                Resources.Clear();
                foreach (var r in _taskApi.ResourceDisks) {
                    Resources.Add(new QDisk(_api, new Guid(r)));
                }
            }

            if (Results == null && _taskApi.ResultDisk != null) {
                Results = new QDisk(_api, new Guid(_taskApi.ResultDisk));
            }

            if (updateDisksInfo) {
                foreach (var r in Resources) {
                    await r.UpdateAsync();
                }
                if (Results != null) {
                    await Results.UpdateAsync();
                }
            }
        }

        public async Task DeleteAsync() {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.DeleteAsync(_uri);
            await Utils.LookForErrorAndThrow(_api._client, response);
        }

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

        /////////////////////////////////////////////////////////////////////////

        public void Resume(string outDir) {
            _resumeSource = new CancellationTokenSource();
            ResumeAsync(outDir).Wait();
        }

        public async Task ResumeAsync(string outDir) {
            _resumeSource = new CancellationTokenSource();
            await ResumeAsync(outDir, _resumeSource.Token);
        }

        public async Task ResumeAsync(string outDir, CancellationToken cancellationToken) {
            await ManageTaskAsync(cancellationToken, outDir);
        }

        public void Cancel() {
            if (_snapshotSource != null) {
                _snapshotSource.Cancel();
            }
            if (_submitSource != null) {
                _submitSource.Cancel();
            }
            if (_resumeSource != null) {
                _resumeSource.Cancel();
            }
        }

        public void Run(string outDir) {
            _submitSource = new CancellationTokenSource();
            RunAsync(outDir, _submitSource.Token).Wait();
        }

        public async Task RunAsync(string outDir) {
            _submitSource = new CancellationTokenSource();
            await RunAsync(outDir, _submitSource.Token);
        }

        public async Task RunAsync(string outDir, CancellationToken cancellationToken) {
            /*
            if (_existingTask) {
                return;
            }*/

            _outDir = outDir;
            _taskApi.ResourceDisks = new List<string>();
            foreach (var item in Resources) {
                _taskApi.ResourceDisks.Add(item.Uuid.ToString());
            }

            var response = await _api._client.PostAsJsonAsync<TaskApi>("tasks", _taskApi, cancellationToken);

            await Utils.LookForErrorAndThrow(_api._client, response);

            _uri = response.Headers.Location.OriginalString.Substring(1);
            await ManageTaskAsync(cancellationToken, outDir);
        }

        private async Task ManageTaskAsync(CancellationToken cancellationToken, string outDir) {
            var taskSuccess = await AsyncCompletion(_uri, cancellationToken);

            var response = await _api._client.GetAsync(_uri); // get task

            await Utils.LookForErrorAndThrow(_api._client, response);

            TaskApi ta = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
            _taskApi = ta;
            string resDiskUri = "disks/tree/" + ta.ResultDisk;
            if (taskSuccess) {
                response = await _api._client.GetAsync(resDiskUri);
                if (response.IsSuccessStatusCode) {
                    List<QFile> resFiles = await response.Content.ReadAsAsync<List<QFile>>(cancellationToken);
                    Task[] tasks = new Task[resFiles.Count];
                    uint index = 0;
                    resDiskUri = "disks/" + ta.ResultDisk;
                    try {
                        foreach (var item in resFiles) {
                            tasks[index++] = Utils.Download(_api._client, resDiskUri, item.Name, outDir, cancellationToken);
                        }
                        await Task.WhenAll(tasks);
                    } catch (Exception ex) {
                        //Console.WriteLine (ex.Message);
                        throw ex;
                    }
                } else {
                    // result disk files list retrieval failed
                    await Utils.LookForErrorAndThrow(_api._client, response);
                }
            } else {
                //task failed
                throw new TaskFailedException();
            }
        }

        public void Snapshot() {
            var response = _api._client.PostAsync(_uri + "/snapshot", null);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
                SnapshotHandler();
        }

        public void Snapshot(uint interval) {
            Snapshot s = new QarnotSDK.Snapshot();
            s.Interval = Convert.ToInt32(interval);
            var response = _api._client.PostAsJsonAsync<Snapshot>(_uri + "/snapshot/periodic", s);
            response.Wait();
            if (response.Result.IsSuccessStatusCode)
                SnapshotHandler();
        }

        private void SnapshotHandler() {
            if (_snapshotSource != null)
                _snapshotSource.Cancel();
            _snapshotSource = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(new WaitCallback(SnapshotCallback), _snapshotSource.Token);
        }

        private void SnapshotCallback(object o) {
            CancellationToken ct = (CancellationToken)o;

            while (true) {
                try {
                    SnapshotAsync(ct).Wait();
                } catch (Exception) {
                    break;
                }
                if (SnapshotResultsAvailable != null)
                    SnapshotResultsAvailable(this);
                if (ct.IsCancellationRequested)
                    break;
            }
        }

        private async Task SnapshotAsync(CancellationToken cancellationToken) {
            var response = await _api._client.GetAsync(_uri);
            if (response.IsSuccessStatusCode) {
                TaskApi ta = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
                bool readyToDownload = await WaitSnapshotAsync(_uri, ta.ResultsCount, cancellationToken);
                if (readyToDownload) {
                    response = await _api._client.GetAsync(_uri);
                    if (response.IsSuccessStatusCode) {
                        ta = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
                        string resDiskUri = "disks/tree/" + ta.ResultDisk;
                        response = await _api._client.GetAsync(resDiskUri);
                        if (response.IsSuccessStatusCode) {
                            List<QFile> resFiles = await response.Content.ReadAsAsync<List<QFile>>(cancellationToken);
                            var tasks = new Task[resFiles.Count];
                            uint index = 0;
                            resDiskUri = "disks/" + ta.ResultDisk;
                            foreach (var item in resFiles) {
                                tasks[index++] = Utils.Download(_api._client, resDiskUri, item.Name, _outDir, cancellationToken);
                            }
                            await Task.WhenAll(tasks);
                        }
                    }
                }
            }
        }

        #endregion

        #region background methods
        private async Task<bool> AsyncCompletion(string taskUri, CancellationToken cancellationToken) {
            uint cachedResultsCount = 0;
            try {
                while (true) {
                    var response = await _api._client.GetAsync(taskUri);
                    if (response.IsSuccessStatusCode) {
                        TaskApi ta = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
                        _taskApi = ta;
                        if (cancellationToken.IsCancellationRequested) {
                            throw new TaskCanceledException();
                        }

                        switch (ta.State) {
                            case "Cancelled":
                                return false;
                            case "Failure":
                                return false;
                            case "Success":
                                //Console.WriteLine("Task success !!");
                                if (cachedResultsCount == ta.ResultsCount) {
                                    await Task.Delay(1000, cancellationToken);
                                    break;
                                }
                                return true;
                            default:
                                await Task.Delay(1000, cancellationToken);
                                break;
                        }
                        cachedResultsCount = ta.ResultsCount;
                    } else
                        return false;
                }
            } catch (TaskCanceledException ex) {
                var response = await _api._client.DeleteAsync(taskUri); //stop
                response = await _api._client.DeleteAsync(taskUri); //delete
                throw ex;
            }
        }

        private async Task<bool> WaitSnapshotAsync(string taskuri, uint baseCachedResultCount, CancellationToken cancellationToken) {
            uint cachedResultsCount = baseCachedResultCount;
            try {
                while (true) {
                    var response = await _api._client.GetAsync(taskuri);
                    if (response.IsSuccessStatusCode) {
                        TaskApi ta = await response.Content.ReadAsAsync<TaskApi>(cancellationToken);
                        _taskApi = ta;
                        if (cancellationToken.IsCancellationRequested) {
                            throw new TaskCanceledException();
                        }
                        if (cachedResultsCount != ta.ResultsCount) {
                            //Console.WriteLine ("Task state : " + ta.State);
                            return ta.State == "Submitted";
                        }
                        cachedResultsCount = ta.ResultsCount;
                    } else {
                        return false;
                    }
                    await Task.Delay(1000, cancellationToken);
                }
            } catch (TaskCanceledException ex) {
                throw ex;
            }
            #endregion
        }

        public string TaskApiDebug() {
            return _taskApi.ToString();
        }
    }

    public class QTaskStatusActiveForwards {
        public UInt16 ApplicationPort { get; set; }
        public UInt16 ForwarderPort { get; set; }
        public string ForwarderHost { get; set; }
        public QTaskStatusActiveForwards() {
        }
    }

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

    public class QTaskStatus {
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
        public QTaskStatusRunningInstancesInfo RunningInstancesInfo { get; set; }

        public QTaskStatus() {
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
        public int SnapshotInterval { get; set; }
        public uint ResultsCount { get; set; }
        public DateTime CreationDate { get; set; }
        public List<KeyValHelper> Constants { get; set; }
        public Guid Uuid { get; set; }
        public string Shortname { get; set; }
        public QTaskStatus Status { get; set; }

        public TaskApi() {
            Constants = new List<KeyValHelper>();
            ResourceDisks = new List<String>();
        }
    }
}
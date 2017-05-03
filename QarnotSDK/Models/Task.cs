using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading;

namespace qarnotsdk
{
    public class QTask
    {
        private readonly Connection _api;
        private TaskApi _taskApi;
        private CancellationTokenSource _submitSource = null;
        private CancellationTokenSource _snapshotSource = null;
        private CancellationTokenSource _resumeSource = null;
        private string _outDir = null;
        private string _taskUri = null;
        private bool _existingTask = false;

        private Dictionary<string, string> _files;

        public List<QDisk> Resources { get; set; }

        public QDisk Results { get; set; }

        public QTaskStatus Status { get { return _taskApi != null ? _taskApi.Status : null; } set{ ; }  }

        public string State { get { return _taskApi != null ? _taskApi.State : null; } set { ; } }

        public Guid Uuid { get { return _taskApi.Uuid; } }

        public string Name { get { return _taskApi.Name; } }

        public delegate void SnapshotResultsAvailableEventHandler(QTask sender);

        public event SnapshotResultsAvailableEventHandler SnapshotResultsAvailable;

        internal QTask ()
        {
            Resources = new List<QDisk> ();
        }

        internal QTask (Connection api, string name, string profile, uint instanceCount)
        {
            _api = api;
            _taskApi = new TaskApi ();
            _taskApi.Name = name;
            _taskApi.Profile = profile;
            _taskApi.FrameCount = instanceCount;
            _files = new Dictionary<string, string> ();
            Resources = new List<QDisk> ();
            _existingTask = false;
        }

        internal QTask (Connection qapi, TaskApi taskApi)
        {
            _api = qapi;
            _taskApi = taskApi;
            _existingTask = true;
            _taskUri = "tasks/" + _taskApi.Uuid.ToString ();
        }

        #region public methods

        public void AddConstant(string key, string value)
        {
            _taskApi.Constants.Add (new TaskApi.KeyValXml (key, value));
        }

        public async Task SubmitAsync()
        {
            if (_existingTask) {
                return;
            }

            _taskApi.ResourceDisks = new List<string> ();
            foreach (var item in Resources) {
                _taskApi.ResourceDisks.Add (item.Uuid.ToString ());
            }
            if (Results != null) {
                _taskApi.ResultDisk = Results.Uuid.ToString();
            }

            var response = await _api._client.PostAsJsonAsync<TaskApi> ("tasks", _taskApi);
            Utils.LookForErrorAndThrow(_api._client, response);

            _taskUri = response.Headers.Location.OriginalString.Substring (1);

            // Update the task Uuid
            var result = await response.Content.ReadAsAsync<TaskApi>();
            _taskApi.Uuid = result.Uuid;

            // Retrieve the task status once to update the other fields (result disk uuid etc..)
            await UpdateStatusAsync();
        }

        public async Task UpdateStatusAsync() {
            var response = await _api._client.GetAsync(_taskUri); // get task status
            Utils.LookForErrorAndThrow(_api._client, response);

            var result = await response.Content.ReadAsAsync<TaskApi>();
            _taskApi = result;

            if (Results == null) {
                Results = await _api.RetrieveDiskAsync(new Guid(_taskApi.ResultDisk));
            }
        }

        public async Task DeleteAsync()
        {
            var response = await _api._client.DeleteAsync(_taskUri);
            Utils.LookForErrorAndThrow(_api._client, response);
        }

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
            if (_existingTask) {
                return;
            }

            _outDir = outDir;
            _taskApi.ResourceDisks = new List<string>();
            foreach (var item in Resources) {
                _taskApi.ResourceDisks.Add(item.Uuid.ToString());
            }

            var response = await _api._client.PostAsJsonAsync<TaskApi>("tasks", _taskApi, cancellationToken);

            Utils.LookForErrorAndThrow(_api._client, response);
            //Console.WriteLine ("The Task creation response is : " + response.IsSuccessStatusCode + " : " + response.StatusCode);
            _taskUri = response.Headers.Location.OriginalString.Substring(1);
            await ManageTaskAsync(cancellationToken, outDir);
        }

        private async Task ManageTaskAsync(CancellationToken cancellationToken, string outDir) {
            //Console.WriteLine (_taskUri);

            var taskSuccess = await AsyncCompletion (_taskUri, cancellationToken);
            //Console.WriteLine ("Task is successful ? : " + taskSuccess);
            var response = await _api._client.GetAsync (_taskUri); // get task

            Utils.LookForErrorAndThrow (_api._client, response);

            TaskApi ta = await response.Content.ReadAsAsync<TaskApi> (cancellationToken);
            _taskApi = ta;
            string resDiskUri = "disks/tree/" + ta.ResultDisk;
            if (taskSuccess) {
                response = await _api._client.GetAsync (resDiskUri);
                if (response.IsSuccessStatusCode) {
                    List<MyFile> resFiles = await response.Content.ReadAsAsync<List<MyFile>> (cancellationToken);
                    Task[] tasks = new Task[resFiles.Count];
                    uint index = 0;
                    resDiskUri = "disks/" + ta.ResultDisk;
                    try {
                        foreach (var item in resFiles) {
                            tasks [index++] = Utils.Download (_api._client, resDiskUri, item.Name, outDir, cancellationToken);
                        }
                        await Task.WhenAll (tasks);
                    } catch (Exception ex) {
                            //Console.WriteLine (ex.Message);
                            throw ex;
                    }
                } else {
                    // result disk files list retrieval failed
                    Utils.LookForErrorAndThrow (_api._client, response);
                }
            } else {
                //task failed
                throw new TaskFailedException ();
            }
        }

        public void Snapshot()
        {
            var response = _api._client.PostAsync (_taskUri + "/snapshot", null);
            response.Wait ();
            if (response.Result.IsSuccessStatusCode)
                SnapshotHandler ();
        }

        public void Snapshot(uint interval)
        {
            Snapshot s = new qarnotsdk.Snapshot ();
            s.Interval = Convert.ToInt32 (interval);
            var response = _api._client.PostAsJsonAsync<Snapshot> (_taskUri + "/snapshot/periodic", s);
            response.Wait ();
            if (response.Result.IsSuccessStatusCode)
                SnapshotHandler ();
        }

        private void SnapshotHandler()
        {
            if (_snapshotSource != null)
                _snapshotSource.Cancel ();
            _snapshotSource = new CancellationTokenSource ();
            ThreadPool.QueueUserWorkItem (new WaitCallback (SnapshotCallback), _snapshotSource.Token);
        }

        private void SnapshotCallback(object o)
        {
            CancellationToken ct = (CancellationToken)o;

            while (true) {
                try {
                    SnapshotAsync (ct).Wait ();
                } catch (Exception) {
                    break;
                }
                if (SnapshotResultsAvailable != null)
                    SnapshotResultsAvailable (this);
                if (ct.IsCancellationRequested)
                    break;
            }
        }

        private async Task SnapshotAsync(CancellationToken cancellationToken)
        {
            var response = await _api._client.GetAsync (_taskUri);
            if (response.IsSuccessStatusCode) {
                TaskApi ta = await response.Content.ReadAsAsync<TaskApi> (cancellationToken);
                bool readyToDownload = await WaitSnapshotAsync(_taskUri, ta.ResultsCount, cancellationToken);
                if (readyToDownload) {
                    response = await _api._client.GetAsync (_taskUri);
                    if (response.IsSuccessStatusCode) {
                        ta = await response.Content.ReadAsAsync<TaskApi> (cancellationToken);
                        string resDiskUri = "disks/tree/" + ta.ResultDisk;
                        response = await _api._client.GetAsync (resDiskUri);
                        if (response.IsSuccessStatusCode) {
                            List<MyFile> resFiles = await response.Content.ReadAsAsync<List<MyFile>> (cancellationToken);
                            var tasks = new Task[resFiles.Count];
                            uint index = 0;
                            resDiskUri = "disks/" + ta.ResultDisk;
                            foreach (var item in resFiles) {
                                tasks [index++] = Utils.Download (_api._client, resDiskUri, item.Name, _outDir, cancellationToken);
                            }
                            await Task.WhenAll (tasks);
                        }
                    }
                }
            }
        }

        #endregion

        #region background methods



        private async Task<bool> AsyncCompletion(string taskUri, CancellationToken cancellationToken)
        {
            uint cachedResultsCount = 0;
            try {
                while (true) {
                    var response = await _api._client.GetAsync (taskUri);
                    if (response.IsSuccessStatusCode) {
                        TaskApi ta = await response.Content.ReadAsAsync<TaskApi> (cancellationToken);
                        _taskApi = ta;
                        if (cancellationToken.IsCancellationRequested) {
                            throw new TaskCanceledException ();
                        }

                        switch (ta.State) {
                        case "Cancelled":
                            return false;
                        case "Failure":
                            return false;
                        case "Success":
                            Console.WriteLine ("Task success !!");
                            if (cachedResultsCount == ta.ResultsCount) {
                                await Task.Delay (1000, cancellationToken);
                                break;
                            }
                            return true;
                        default:
                            await Task.Delay (1000, cancellationToken);
                            break;
                        }
                        cachedResultsCount = ta.ResultsCount;
                    } else
                        return false;
                }
            } catch (TaskCanceledException ex) {
                var response = await _api._client.DeleteAsync (taskUri); //stop
                response = await _api._client.DeleteAsync (taskUri); //delete
                throw ex;
            }
        }

        private async Task<bool> WaitSnapshotAsync(string taskuri, uint baseCachedResultCount, CancellationToken cancellationToken)
        {
            uint cachedResultsCount = baseCachedResultCount;
            try {
                while (true) {
                    var response = await _api._client.GetAsync (taskuri);
                    if (response.IsSuccessStatusCode) {
                        TaskApi ta = await response.Content.ReadAsAsync<TaskApi> (cancellationToken);
                        _taskApi = ta;
                        if (cancellationToken.IsCancellationRequested) {
                            throw new TaskCanceledException ();
                        }
                        if (cachedResultsCount != ta.ResultsCount) {
                            //Console.WriteLine ("Task state : " + ta.State);
                            return ta.State == "Submitted";
                        }
                        cachedResultsCount = ta.ResultsCount;
                    } else {
                        return false;
                    }
                    await Task.Delay (1000, cancellationToken);
                }
            } catch (TaskCanceledException ex) {
                throw ex;
            }
            #endregion
        }

        public string TaskApiDebug()
        {
            return _taskApi.ToString ();
        }
    }

    [Serializable]
    public class TaskFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        public TaskFailedException ()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public TaskFailedException (string message) : base (message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public TaskFailedException (string message, Exception inner) : base (message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected TaskFailedException (System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base (info, context)
        {
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
        public QTaskStatus() {
        }
    }

    internal class TaskApi
    {public override string ToString ()
        {
            return string.Format ("[TaskApi: Name={0}, Profile={1}, FrameCount={2}, ResultDisk={3}, State={4}, SnapshotInterval={5}, ResultsCount={6}, CreationDate={7}, Uuid={8}]", Name, Profile, FrameCount, ResultDisk, State, SnapshotInterval, ResultsCount, CreationDate, Uuid);
        }
        

        public string Name { get; set; }

        public string Profile { get; set; }

        public uint FrameCount { get; set; }

        public List<string> ResourceDisks { get; set; }

        public string ResultDisk { get; set; }

        public string State { get; set; }

        public int SnapshotInterval { get; set; }

        public uint ResultsCount { get; set; }

        public DateTime CreationDate { get ; set; }

        public List<KeyValXml> Constants { get; set; }

        public Guid Uuid { get; set; }

        public QTaskStatus Status { get; set; }

        public TaskApi ()
        {
            Constants = new List<KeyValXml> ();
            ResourceDisks = new List<String> ();
        }

 

        internal class KeyValXml
        {
            public string Key { get; set; }

            public string Value { get; set; }

            public KeyValXml (string key, string value)
            {
                Key = key;
                Value = value;
            }

            public KeyValXml ()
            {

            }
        }
    }
}
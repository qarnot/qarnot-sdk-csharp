using System;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace QarnotSDK {
    /// <summary>
    /// This class allows you to access the Qarnot compute API and construct other objects: QTask, QPool, QDisk.
    /// </summary>
    public partial class Connection {
        internal HttpClient _client;
        internal HttpClientHandler _httpClientHandler;
        internal RetryHandler _retryHandler;

        /// <summary>
        /// Specify if the Api has the shortname feature to retrieve the tasks/pools by name.
        /// If set to false, the task/pool shortname feature will be emulated by the SDK,
        /// however more requests are needed to achieve the same result.
        /// The default value is true.
        /// </summary>
        public bool HasShortnameFeature { get; set; }
        /// <summary>
        /// Specify if the Api has the shortname feature to retrieve the disks by name.
        /// If set to false, the disk shortname feature will be emulated by the SDK,
        /// however more requests are needed to achieve the same result.
        /// The default value is false.
        /// </summary>
        public bool HasDiskShortnameFeature { get; set; }
        /// <summary>
        /// Set the Connection object in read-only mode, useful for monitoring purpose.
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// The token used for authentication against the Api.
        /// This token is available at https://account.qarnot.com
        /// </summary>
        public string Token { get; }
        /// <summary>
        /// Api Uri.
        /// The default ClusterUri is https://api.qarnot.com
        /// </summary>
        public Uri Uri { get; }
        /// <summary>
        /// The Uri where the buckets are stored.
        /// Note: if null or empty, this value is auto filled when calling
        ///  any Bucket method for the first time.
        /// </summary>
        public Uri StorageUri { get; set; }
        /// <summary>
        /// The access key to your buckets.
        /// By default, this is your account email.
        /// Note: if null or empty, this value is auto filled when calling
        ///  any Bucket method for the first time.
        /// </summary>
        public string StorageAccessKey { get; set; }
        /// <summary>
        /// The secret key to your buckets.
        /// By default, this is your token.
        /// </summary>
        public string StorageSecretKey { get; set; }
        /// <summary>
        /// The upload part size for your bucket's object.
        /// If set to 0, the object are not uploaded by part.
        /// Must be greater than "5 * 1024 * 1024" (5MB).
        /// </summary>
        public long StorageUploadPartSize { get; set; } = 8 * 1024 * 1024;
        /// <summary>
        /// List of part sizes to try to compute the bucket object eTag.
        /// The default element is "0" which means "try to guess the part sizes"
        /// If you know the part size or have a part size not multiple of 1MB, you can
        /// override this list to improve performances.
        /// </summary>
        public List<long> StorageAvailablePartSizes {
            get; set;
        } = new List<long>() { 0 };
        /// <summary>
        /// Maximum number of retries in case of transient error.
        /// Default is 3 times.
        /// </summary>
        public int MaxRetry { get { return _retryHandler.MaxRetries; } set { _retryHandler.MaxRetries = value; } }

        /// <summary>
        /// Construct a new Connection object using your token.
        /// </summary>
        /// <param name="token">The Api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        public Connection(string token, HttpClientHandler httpClientHandler = null) : this("https://api.qarnot.com", token, httpClientHandler) {
        }

        /// <summary>
        /// Construct a new Connection object to a different Api endpoints (private, UAT, Dev...) using your token.
        /// </summary>
        /// <param name="uri">Api Uri, should be https://api.qarnot.com </param>
        /// <param name="token">The Api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        public Connection(string uri, string token, HttpClientHandler httpClientHandler = null) : this(uri, null, token, httpClientHandler) {
        }

        /// <summary>
        /// Construct a new Connection object to a different Api endpoints (private, UAT, Dev...) and
        /// different S3 storage endpoints using your token.
        /// </summary>
        /// <param name="uri">Api Uri, should be https://api.qarnot.com </param>
        /// <param name="storageUri">Storage Uri, should be null or https://storage.qarnot.com </param>
        /// <param name="token">The api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        public Connection(string uri, string storageUri, string token, HttpClientHandler httpClientHandler = null) {
            Uri = new Uri(uri);
            if (storageUri != null) StorageUri = new Uri(storageUri);
            Token = token;
            StorageSecretKey = token;
            HasShortnameFeature = true;
            HasDiskShortnameFeature = false;
            _httpClientHandler = httpClientHandler == null ? new HttpClientHandler():httpClientHandler;
            _retryHandler = new RetryHandler(_httpClientHandler, 3);
            _client = new HttpClient(_retryHandler);
            _client.BaseAddress = Uri;
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token);
        }

        #region CreateX
        /// <summary>
        /// Create a new Pool.
        /// A pool is a running set of nodes where you can execute tasks.
        /// The newly created pool has to be started.
        /// </summary>
        /// <param name="name">The name of the pool</param>
        /// <param name="profile">The profile of the pool. If not specified, it must be given when the pool is started.</param>
        /// <param name="initialNodeCount">The number of nodes you want in the pool. If not specified, it must be given when the pool is started.</param>
        /// <returns>A new pool.</returns>
        public QPool CreatePool(string name, string profile = null, uint initialNodeCount = 0) {
            var pool = new QPool(this, name, profile, initialNodeCount);
            return pool;
        }

        /// <summary>
        /// Create a new Task that will run outside of any pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="profile">The profile of the task. If not specified, it must be given when the task is submitted.</param>
        /// <returns>A new task.</returns>
        public QTask CreateTask(string name, string profile = null) {
            var task = new QTask(this, name, profile, 0);
            return task;
        }

        /// <summary>
        /// Create a new Task that will run outside of any pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="profile">The profile of the task. If not specified, it must be given when the task is submitted.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <returns>A new task.</returns>
        public QTask CreateTask(string name, string profile, uint instanceCount = 0) {
            var task = new QTask(this, name, profile, instanceCount);
            return task;
        }

        /// <summary>
        /// Create a new Task that will run outside of any pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="profile">The profile of the task. If not specified, it must be given when the task is submitted.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <returns>A new task.</returns>
        public QTask CreateTask(string name, string profile, AdvancedRanges range) {
            var task = new QTask(this, name, profile, range);
            return task;
        }

        /// <summary>
        /// Create a new Task inside an existing pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="pool">The pool where the task will run.</param>
        /// <param name="instanceCount">How many times the task have to run.</param>
        /// <returns>A new task.</returns>
        public QTask CreateTask(string name, QPool pool, uint instanceCount) {
            var task = new QTask(this, name, pool, instanceCount);
            return task;
        }

        /// <summary>
        /// Create a new Task inside an existing pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="pool">The pool where the task will run.</param>
        /// <param name="range">Which instance ids of the task have to run.</param>
        /// <returns>A new task.</returns>
        public QTask CreateTask(string name, QPool pool, AdvancedRanges range) {
            var task = new QTask(this, name, pool, range);
            return task;
        }

        /// <summary>
        /// Create a new disk.
        /// </summary>
        /// <param name="name">The name of the disk.</param>
        /// <returns>A new disk.</returns>
        public QDisk CreateDisk(string name) {
            var disk = new QDisk(this, name);
            return disk;
        }
        #endregion

        #region RetrieveXAsync
        /// <summary>
        /// Retrieve the tasks list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public async Task<List<QTask>> RetrieveTasksAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("tasks", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            var qapiTaskList = await response.Content.ReadAsAsync<List<TaskApi>>(cancellationToken);
            var ret = new List<QTask>();
            foreach (var item in qapiTaskList) {
                ret.Add(new QTask(this, item));
            }
            return ret;
        }

        /// <summary>
        /// Retrieve the tasks list filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for task filtering.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public async Task<List<QTask>> RetrieveTasksByTagsAsync(List<string> tags, CancellationToken cancellationToken = default(CancellationToken)) {
            if(tags == null || tags.Count == 0)
                return RetrieveTasksAsync(cancellationToken).Result;

            var uri = "tasks/?tag=" + string.Join(",", tags.Select(tag => HttpUtility.UrlEncode(tag)));
            var response = await _client.GetAsync(uri, cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            var qapiTaskList = await response.Content.ReadAsAsync<List<TaskApi>>(cancellationToken);
            var ret = new List<QTask>();
            foreach (var item in qapiTaskList) {
                ret.Add(new QTask(this, item));
            }
            return ret;
        }

        /// <summary>
        /// Retrieve the pools list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public async Task<List<QPool>> RetrievePoolsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("pools", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            var list = await response.Content.ReadAsAsync<List<PoolApi>>(cancellationToken);
            var ret = new List<QPool>();
            foreach (var item in list) {
                ret.Add(new QPool(this, item));
            }
            return ret;
        }

        /// <summary>
        /// Retrieve the disks list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of disks.</returns>
        public async Task<List<QDisk>> RetrieveDisksAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("disks", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            var list = await response.Content.ReadAsAsync<List<DiskApi>>(cancellationToken);
            var ret = new List<QDisk>();
            foreach (var item in list) {
                ret.Add(new QDisk(this, item));
            }
            return ret;
        }

        internal async Task<Amazon.S3.AmazonS3Client> GetS3ClientAsync(CancellationToken cancellationToken) {
            // Check if we need to retrieve the StorageAccessKey (account email)
            if (String.IsNullOrEmpty(StorageAccessKey)) {
                // RetrieveUserInformationAsync will set the StorageAccessKey
                await RetrieveUserInformationAsync(false, cancellationToken);
            }
            if (StorageUri == null) {
                var s = await RetrieveApiSettingsAsync(cancellationToken);
                if (s.Storage == null) throw new Exception($"The API at {_client.BaseAddress} doesn't have a Ceph storage configured.");
                StorageUri = new Uri(s.Storage);
            }

            // S3Config
            var s3Config = new Amazon.S3.AmazonS3Config();
            s3Config.ServiceURL = StorageUri.ToString();

            // Setup the proxy from the HttpClientHandler
            if (_httpClientHandler != null) {
                var proxy = _httpClientHandler.Proxy as System.Net.WebProxy;
                if (proxy != null) s3Config.SetWebProxy(proxy);
            }

            // Create the client with the proper credentials & configuration
            var s3Client = new Amazon.S3.AmazonS3Client(
                StorageAccessKey,
                StorageSecretKey,
                s3Config);

            return s3Client;
        }

        /// <summary>
        /// Retrieve the buckets list with each bucket file count and used space.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of buckets.</returns>
        public async Task<List<QBucket>> RetrieveBucketsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return await RetrieveBucketsAsync(true, cancellationToken);
        }

        /// <summary>
        /// Retrieve the buckets list.
        /// </summary>
        /// <param name="retrieveBucketStats">If set to true, the file count and used space of each bucket is also retrieved. If set to false, it is faster but only the bucket names are returned.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of buckets.</returns>
        public async Task<List<QBucket>> RetrieveBucketsAsync(bool retrieveBucketStats, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var s3Client = await GetS3ClientAsync(cancellationToken)) {
                var s3Response = await s3Client.ListBucketsAsync(cancellationToken);

                var ret = new List<QBucket>();

                // Update bucket statistics (file count / used space)
                var tasks = new List<Task>();
                foreach (var item in s3Response.Buckets) {
                    var b = new QBucket(this, item);
                    if (retrieveBucketStats) tasks.Add(b.UpdateAsync(cancellationToken));
                    ret.Add(b);
                }
                try {
                    if (retrieveBucketStats) await Task.WhenAll(tasks);
                } catch {
                    // Discard silently UpdateAsync exceptions
                }

                return ret;
            }
        }

        /// <summary>
        /// Retrieve the storages list (buckets and disks).
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of disks and buckets.</returns>
        public async Task<List<QAbstractStorage>> RetrieveStoragesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var t1 = RetrieveDisksAsync(cancellationToken);
            var t2 = RetrieveBucketsAsync(cancellationToken);
            var ret = new List<QAbstractStorage>(await t1);
            ret.AddRange(await t2);
            return ret;
        }

        /// <summary>
        /// Retrieve the rest Api settings.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The rest Api settings.</returns>
        public async Task<ApiSettings> RetrieveApiSettingsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("settings", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            return await response.Content.ReadAsAsync<ApiSettings>(cancellationToken);
        }

        /// <summary>
        /// Retrieve the user quotas and disks information for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and disks information.</returns>
        public async Task<UserInformation> RetrieveUserInformationAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return await RetrieveUserInformationAsync(true, cancellationToken);
        }

        /// <summary>
        /// Retrieve the user quotas and disks information for your account.
        /// Note: BucketCount field is retrieved with a second request to the bucket Api.
        /// </summary>
        /// <param name="retrieveBucketCount">If set to false, the BucketCount field is not filled but the request is faster.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and disks information without BucketCount.</returns>
        public async Task<UserInformation> RetrieveUserInformationAsync(bool retrieveBucketCount, CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("info", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);
            var u = await response.Content.ReadAsAsync<UserInformation>(cancellationToken);

            // Cache the user access key, for further calls to the bucket Api.
            if (String.IsNullOrEmpty(StorageAccessKey)) {
                StorageAccessKey = u.Email;
            }

            if (retrieveBucketCount) {
                u.BucketCount = (await RetrieveBucketsAsync(false, cancellationToken)).Count;
            }

            return u;
        }

        /// <summary>
        /// Retrieve the list of profiles available for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of profile names.</returns>
        public async Task<List<string>> RetrieveProfilesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("profiles", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            return await response.Content.ReadAsAsync<List<string>>(cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of the constants you can override for a specific profile.
        /// </summary>
        /// <param name="profile">Name of the profile.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of constants.</returns>
        public async Task<List<Constant>> RetrieveConstantsAsync(string profile, CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _client.GetAsync("profiles/" + profile, cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            var list = await response.Content.ReadAsAsync<ProfilesConstantApi>(cancellationToken);
            return list.Constants;
        }
        #endregion

        #region RetrieveXByNameAsync
        /// <summary>
        /// Retrieve a task by its name.
        /// </summary>
        /// <param name="name">Name of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that name or null if it hasn't been found.</returns>
        public async Task<QTask> RetrieveTaskByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrieveTasksAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a pool by its name.
        /// </summary>
        /// <param name="name">Name of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that name or null if it hasn't been found.</returns>
        public async Task<QPool> RetrievePoolByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrievePoolsAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a disk by its name.
        /// </summary>
        /// <param name="name">Name of the disk to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The disk object for that name or null if it hasn't been found.</returns>
        public async Task<QDisk> RetrieveDiskByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrieveDisksAsync(cancellationToken);
            return ret.Find(x => x.Description == name);
        }
        #endregion

        #region CreateXAsync
        /// <summary>
        /// Submit a list of task as a bulk.
        /// </summary>
        /// <param name="tasks">The task list to submit as a bulk.</param>
        /// <param name="autoCreateResultDisk">Set to true to ensure that the result disk specified exists. If set to false and the result disk doesn't exist, this will result in an exception.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>void.</returns>
        public async Task SubmitTasksAsync(List<QTask> tasks, bool autoCreateResultDisk = true, CancellationToken cancellationToken = default(CancellationToken)) {
            await Task.WhenAll(tasks.Select(task => task.PreSubmitAsync(cancellationToken, autoCreateResultDisk)));
            var response = await _client.PostAsJsonAsync<List<TaskApi>>("tasks", tasks.Select(t => t._taskApi).ToList(), cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_client, response);
            var results = await response.Content.ReadAsAsync<List<QBulkTaskResponse>>(cancellationToken);

            // The "contract" with the api is that response should come in the same order as submission
            var errorMessage = String.Empty;
            var postTasks = new List<Task>();
            for (int i = 0; i < tasks.Count; i++) {
                if (!results[i].IsSuccesResponse) {
                    errorMessage += $"[{tasks[i].Name}] : {results[i].StatusCode}, {results[i].Message}\n";
                }
                postTasks.Add(tasks[i].PostSubmitAsync(new TaskApi() { Uuid = results[i].Uuid }, cancellationToken));
            }
            await Task.WhenAll(postTasks);

            // Notify user that something went partially wrong.
            if (!String.IsNullOrEmpty(errorMessage)) {
                throw new Exception(errorMessage);
            }
        }
        #endregion
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace QarnotSDK
{
    /// <summary>
    /// This class allows you to access the Qarnot compute API and construct other objects: QTask, QPool, QDisk.
    /// </summary>
    public partial class Connection {
        internal HttpClient _client;

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
        /// The token used for authentication against the api.
        /// This token is available at https://account.qarnot.com
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Api Uri.
        /// The default Uri is https://api.qarnot.com
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// Construct a new Connection object using your token.
        /// </summary>
        /// <param name="token">The api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        public Connection(string token, HttpClientHandler httpClientHandler = null) : this("https://api.qarnot.com", token, httpClientHandler) {
        }

        /// <summary>
        /// Construct a new Connection object to a different Api endpoints (private, UAT, Dev...) using your token.
        /// </summary>
        /// <param name="uri">Api Uri, should be https://api.qarnot.com </param>
        /// <param name="token">The api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        public Connection(string uri, string token, HttpClientHandler httpClientHandler = null) {
            Uri = new Uri(uri);
            Token = token;
            HasShortnameFeature = true;
            HasDiskShortnameFeature = false;
            _client = httpClientHandler == null ? new HttpClient() : new HttpClient(httpClientHandler);
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
        /// Retrieve the list of tasks.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public async Task<List<QTask>> RetrieveTasksAsync(CancellationToken cancellationToken = new CancellationToken()) {
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
        /// Retrieve the list of pools.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public async Task<List<QPool>> RetrievePoolsAsync(CancellationToken cancellationToken = new CancellationToken()) {
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
        /// Retrieve the list of disks.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of disks.</returns>
        public async Task<List<QDisk>> RetrieveDisksAsync(CancellationToken cancellationToken = new CancellationToken()) {
            var response = await _client.GetAsync("disks", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            var list = await response.Content.ReadAsAsync<List<DiskApi>>(cancellationToken);
            var ret = new List<QDisk>();
            foreach (var item in list) {
                ret.Add(new QDisk(this, item));
            }
            return ret;
        }

        /// <summary>
        /// Retrieve the user quotas and disks information for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and disks information.</returns>
        public async Task<UserInformation> RetrieveUserInformationAsync(CancellationToken cancellationToken = new CancellationToken()) {
            var response = await _client.GetAsync("info", cancellationToken);

            await Utils.LookForErrorAndThrowAsync(_client, response);

            return await response.Content.ReadAsAsync<UserInformation>(cancellationToken);
        }

        /// <summary>
        /// Retrieve the list of profiles available for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of profile names.</returns>
        public async Task<List<string>> RetrieveProfilesAsync(CancellationToken cancellationToken = new CancellationToken()) {
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
        public async Task<List<Constant>> RetrieveConstantsAsync(string profile, CancellationToken cancellationToken = new CancellationToken()) {
            var response = await _client.GetAsync("profiles/"+profile, cancellationToken);

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
        public async Task<QTask> RetrieveTaskByNameAsync(string name, CancellationToken cancellationToken = new CancellationToken()) {
            var ret = await RetrieveTasksAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a pool by its name.
        /// </summary>
        /// <param name="name">Name of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that name or null if it hasn't been found.</returns>
        public async Task<QPool> RetrievePoolByNameAsync(string name, CancellationToken cancellationToken = new CancellationToken()) {
            var ret = await RetrievePoolsAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a disk by its name.
        /// </summary>
        /// <param name="name">Name of the disk to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The disk object for that name or null if it hasn't been found.</returns>
        public async Task<QDisk> RetrieveDiskByNameAsync(string name, CancellationToken cancellationToken = new CancellationToken()) {
            var ret = await RetrieveDisksAsync(cancellationToken);
            return ret.Find(x => x.Description == name);
        }
        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace QarnotSDK {
    /// <summary>
    /// This class allows you to access the Qarnot compute API and construct other objects: QTask, QPool
    /// </summary>
    public partial class Connection {
        /// <summary>
        /// Feature flag to enable the sanitization of the bucket paths (removing leading and duplicated slashes)
        /// </summary>
        internal bool _shouldSanitizeBucketPaths = true;
        /// <summary>
        /// Flag to show the warnings when bucket path is sanitized (if sanitization is enabled with _shouldSanitizeBucketPaths=true)
        /// </summary>
        internal bool _showBucketWarnings = true;
        internal HttpClient _client;
        internal HttpClientHandler _httpClientHandler;
        internal IRetryHandler _retryHandler;

        /// <summary>
        /// Overload class of the AmazonS3Config to be able to overload
        /// some getters (user-agent, etc...)
        /// </summary>
        private class OverloadedS3config: Amazon.S3.AmazonS3Config {

            /// <summary>
            /// overload the AmazonS3Config user agent to append a qarnot version
            /// </summary>
            /// <value>the qarnot sdk and s3 sdk user agent</value>
            public override string UserAgent { get => $"{Connection.SdkUserAgent} {base.UserAgent}"; }
        }

        /// <summary>
        /// Specify if the Api has the shortname feature to retrieve the tasks/pools by name.
        /// If set to false, the task/pool shortname feature will be emulated by the SDK,
        /// however more requests are needed to achieve the same result.
        /// The default value is true.
        /// </summary>
        [ObsoleteAttribute("It will always be True as the SDK don't need to emulate shortname anymore", false)]
        public bool HasShortnameFeature { get { return true; } set { ; } }
        /// <summary>
        /// Set the Connection object in read-only mode, useful for monitoring purpose.
        /// </summary>
        public  virtual bool IsReadOnly { get; set; }
        /// <summary>
        /// The token used for authentication against the Api.
        /// This token is available at https://account.qarnot.com
        /// </summary>
        public  virtual string Token { get; }
        /// <summary>
        /// Api Uri.
        /// The default ClusterUri is https://api.qarnot.com
        /// </summary>
        public  virtual Uri Uri { get; }
        /// <summary>
        /// The Uri where the buckets are stored.
        /// Note: if null or empty, this value is auto filled when calling
        ///  any Bucket method for the first time.
        /// </summary>
        public virtual Uri StorageUri { get; set; }
        /// <summary>
        /// Boolean to force all requests to the storage service to use a path style
        /// instead of a virtual host.
        /// </summary>
        public virtual bool ForceStoragePathStyle { get; set; }
        /// <summary>
        /// The access key to your buckets.
        /// By default, this is your account email.
        /// Note: if null or empty, this value is auto filled when calling
        ///  any Bucket method for the first time.
        /// </summary>
        public virtual string StorageAccessKey { get; set; }
        /// <summary>
        /// The secret key to your buckets.
        /// By default, this is your token.
        /// </summary>
        public virtual string StorageSecretKey { get; set; }
        /// <summary>
        /// The upload part size for your bucket's object.
        /// If set to 0, the object are not uploaded by part.
        /// Must be greater than "5 * 1024 * 1024" (5MB).
        /// </summary>
        public virtual long StorageUploadPartSize { get; set; } = 8 * 1024 * 1024;
        /// <summary>
        /// List of part sizes to try to compute the bucket object eTag.
        /// The default element is "0" which means "try to guess the part sizes"
        /// If you know the part size or have a part size not multiple of 1MB, you can
        /// override this list to improve performances.
        /// </summary>
        public virtual List<long> StorageAvailablePartSizes {
            get; set;
        } = new List<long>() { 0 };

        /// <summary>
        /// Maximum number of retries in case of transient error.
        /// Default is 3 times.
        /// </summary>
        public virtual int MaxRetry { get { return _retryHandler.MaxRetries; } set { _retryHandler.MaxRetries = value; } }

        /// <summary>
        /// Maximum number of retries in case of storage error.
        /// Default is 3 times.
        /// </summary>
        public virtual int MaxStorageRetry { get; set; } = 3;

        /// <summary>
        /// Interval between retries (in milliseconds).
        /// Default is 500 ms.
        /// </summary>
        public virtual int RetryInterval { get { return _retryHandler.RetryInterval; } set { _retryHandler.RetryInterval = value; } }

        /// <summary>
        /// Sdk user agent: adding references to the current version to trace bugs and usages
        /// </summary>
        private static string SdkUserAgent { get {
                return $"qarnot-sdk-csharp/{Assembly.GetExecutingAssembly().GetName().Version}";
            }
        }

#if (!NET45)
        /// <summary>
        /// An optional HttpClient factory for the S3 storage, if you need to setup a custom certificate or not check the certificate.
        /// </summary>
        public Amazon.Runtime.HttpClientFactory S3HttpClientFactory { get; set; } = null;
#endif

        /// <summary>
        /// Construct a new Connection object using your token.
        /// </summary>
        /// <param name="token">The Api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        /// <param name="retryHandler">An optional IRetryHandler if you need to setup retry for transient error (default to exponential).</param>
        /// <param name="forceStoragePathStyle">An optional forceStoragePathStyle to force path style for request to storage.</param>
        /// <param name="sanitizeBucketPaths">A flag to enable the sanitization of bucket paths (removing leading and duplicated slashes). Enabled by default</param>
        public Connection(string token, HttpClientHandler httpClientHandler = null, IRetryHandler retryHandler = null, bool forceStoragePathStyle = false, bool sanitizeBucketPaths = true)
            : this("https://api.qarnot.com", token, httpClientHandler, retryHandler, forceStoragePathStyle, sanitizeBucketPaths) {
        }

        /// <summary>
        /// Construct a new Connection object to a different Api endpoints (private, UAT, Dev...) using your token.
        /// </summary>
        /// <param name="uri">Api Uri, should be https://api.qarnot.com </param>
        /// <param name="token">The Api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler if you need to setup a proxy for example.</param>
        /// <param name="retryHandler">An optional IRetryHandler if you need to setup retry for transient error (default to exponential).</param>
        /// <param name="forceStoragePathStyle">An optional forceStoragePathStyle to force path style for request to storage.</param>
        /// <param name="sanitizeBucketPaths">A flag to enable the sanitization of bucket paths (removing leading and duplicated slashes). Enabled by default</param>
        public Connection(string uri, string token, HttpClientHandler httpClientHandler = null, IRetryHandler retryHandler = null, bool forceStoragePathStyle = false, bool sanitizeBucketPaths = true)
            : this(uri, null, token, httpClientHandler, retryHandler, forceStoragePathStyle, sanitizeBucketPaths: sanitizeBucketPaths) {
        }

        /// <summary>
        /// Construct a new Connection object to a different Api endpoints (private, UAT, Dev...) and
        /// different S3 storage endpoints using your token.
        /// </summary>
        /// <param name="uri">Api Uri, should be https://api.qarnot.com </param>
        /// <param name="storageUri">Storage Uri, should be null or https://storage.qarnot.com </param>
        /// <param name="token">The api token available at https://account.qarnot.com </param>
        /// <param name="httpClientHandler">An optional HttpClientHandler for the api, if you need to setup a proxy for example.</param>
        /// <param name="retryHandler">An optional IRetryHandler if you need to setup retry for transient error (default to exponential).</param>
        /// <param name="forceStoragePathStyle">An optional forceStoragePathStyle to force path style for request to storage.</param>
        /// <param name="delegatingHandlers">A list of hander used by the api connection. Default will create a list with the QarnotSrvHandler.</param>
        /// <param name="dnsSrvLoadBalancingCacheTime">the cache time in minutes before retrieve the values of the QarnotSrvHandler. Unless you have a strong reason, you should keep the default value.</param>
        /// <param name="sanitizeBucketPaths">A flag to enable the sanitization of bucket paths (removing leading and duplicated slashes). Enabled by default</param>
        /// <param name="showBucketWarnings">A flag to choose to show or remove the warnings during bucket sanitization. Enabled by default</param>
        public Connection(
            string uri,
            string storageUri,
            string token,
            HttpClientHandler httpClientHandler = null,
            IRetryHandler retryHandler = null,
            bool forceStoragePathStyle = false,
            List<DelegatingHandler> delegatingHandlers = null,
            uint? dnsSrvLoadBalancingCacheTime = 5,
            bool sanitizeBucketPaths = true,
            bool showBucketWarnings = true)
        {
            Uri = new Uri(uri);
            if (storageUri != null) StorageUri = new Uri(storageUri);
            ForceStoragePathStyle = forceStoragePathStyle;
            _shouldSanitizeBucketPaths = sanitizeBucketPaths;
            _showBucketWarnings = showBucketWarnings;
            Token = token;
            StorageSecretKey = token;
            _httpClientHandler = httpClientHandler ?? new HttpClientHandler();

            _retryHandler = retryHandler ?? new ExponentialRetryHandler();

            if (delegatingHandlers == null)
            {
                delegatingHandlers = delegatingHandlers ?? new List<DelegatingHandler>();
            }

            AddDnsLoadBalancerToTheDelegateHandlers(dnsSrvLoadBalancingCacheTime, delegatingHandlers);
            delegatingHandlers.Add(_retryHandler);
            _client = new HttpClient(Utils.LinkHandlers(delegatingHandlers, _httpClientHandler));
            _client.BaseAddress = Uri;
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Token);
            _client.DefaultRequestHeaders.Add("User-Agent", SdkUserAgent);
        }

        private void AddDnsLoadBalancerToTheDelegateHandlers(uint? dnsSrvLoadBalancingCacheTime, List<DelegatingHandler> delegatingHandlers)
        {
            var qarnotDnsLoadBalancerHandlerFactory = new QarnotDnsLoadBalancerHandlerFactory(Uri, dnsSrvLoadBalancingCacheTime);
            var qarnotDnsLoadBalancerHandler = qarnotDnsLoadBalancerHandlerFactory.DnsBalancingMessageHandler;
            if (qarnotDnsLoadBalancerHandler != null)
            {
                delegatingHandlers.Add(qarnotDnsLoadBalancerHandler);
            }
        }

        #region CreateX

        /// <summary>
        /// Create a new job.
        /// </summary>
        /// <param name="name">The job name.</param>
        /// <param name="pool">The pool we want the job to be attached to.</param>
        /// <param name="shortname">The pool unique shortname.</param>
        /// <param name="UseTaskDependencies">Bool to allow use of dependencies for tasks in this job.</param>
        /// <returns>A new job.</returns>
        public virtual QJob CreateJob(string name, QPool pool=null, string shortname=default(string), bool UseTaskDependencies=false)
        {
            return new QJob(this, name, pool, shortname, UseTaskDependencies);
        }


        /// <summary>
        /// Create a new Pool.
        /// A pool is a running set of nodes where you can execute tasks.
        /// The newly created pool has to be started.
        /// </summary>
        /// <param name="name">The name of the pool</param>
        /// <param name="profile">The profile of the pool. If not specified, it must be given when the pool is started.</param>
        /// <param name="initialNodeCount">The number of nodes you want in the pool. If not specified, it must be given when the pool is started.</param>
        /// <param name="shortname">optional unique friendly shortname of the pool.</param>
        /// <param name="taskDefaultWaitForPoolResourcesSynchronization">Default value for task's <see
        /// cref="QTask.WaitForPoolResourcesSynchronization" />, see also <see cref="QPool.TaskDefaultWaitForPoolResourcesSynchronization" /></param>
        /// <returns>A new pool.</returns>
        public virtual QPool CreatePool(string name, string profile = null, uint initialNodeCount = 0,
                                        string shortname = default(string), bool? taskDefaultWaitForPoolResourcesSynchronization=null)
        {
            var pool = new QPool(this, name, profile, initialNodeCount, shortname, taskDefaultWaitForPoolResourcesSynchronization);
            return pool;
        }


        /// <summary>
        /// Create a new Task that will run outside of any pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="profile">The profile of the task. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, string profile = null, string shortname = default(string)) {
            var task = new QTask(this, name, profile, 0, shortname);
            return task;
        }

        /// <summary>
        /// Create a new Task that will run outside of any pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="profile">The profile of the task. If not specified, it must be given when the task is submitted.</param>
        /// <param name="instanceCount">How many times the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, string profile, uint instanceCount = 0, string shortname = default(string)) {
            var task = new QTask(this, name, profile, instanceCount, shortname);
            return task;
        }

        /// <summary>
        /// Create a new Task that will run outside of any pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="profile">The profile of the task. If not specified, it must be given when the task is submitted.</param>
        /// <param name="range">Which instance ids of the task have to run. If not specified, it must be given when the task is submitted.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, string profile, AdvancedRanges range, string shortname = default(string)) {
            var task = new QTask(this, name, profile, range, shortname);
            return task;
        }

        /// <summary>
        /// Create a new Task inside an existing pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="pool">The pool where the task will run.</param>
        /// <param name="instanceCount">How many times the task have to run.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="waitForPoolResourcesSynchronization">Whether task should wait for previous pool resources
        /// update to be completed before executing. See <see cref="QTask.WaitForPoolResourcesSynchronization" /></param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, QPool pool, uint instanceCount, string shortname = default(string), bool? waitForPoolResourcesSynchronization=null) {
            var task = new QTask(this, name, pool, instanceCount, shortname, waitForPoolResourcesSynchronization);
            return task;
        }


        /// <summary>
        /// Create a new Task inside an existing pool.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="pool">The pool where the task will run.</param>
        /// <param name="range">Which instance ids of the task have to run.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="waitForPoolResourcesSynchronization">Whether task should wait for previous pool resources
        /// update to be completed before executing. See <see cref="QTask.WaitForPoolResourcesSynchronization" /></param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, QPool pool, AdvancedRanges range, string shortname = default(string), bool? waitForPoolResourcesSynchronization=null) {
            var task = new QTask(this, name, pool, range, shortname, waitForPoolResourcesSynchronization);
            return task;
        }


        /// <summary>
        /// Create a new Task attached to a job.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="job">The job, the task will be attached to.</param>
        /// <param name="instanceCount">How many times the task have to run.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="profile">optional task profile when using a job detached from a pool.</param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, QJob job, uint instanceCount, string shortname = default(string), string profile = default(string)) {
            return new QTask(this, name, job, instanceCount, shortname, profile);
        }

        /// <summary>
        /// Create a new Task attached to a job.
        /// The newly created task has to be submitted.
        /// </summary>
        /// <param name="name">The name of the task.</param>
        /// <param name="job">The job, the task will be attached to.</param>
        /// <param name="range">Which instance ids of the task have to run.</param>
        /// <param name="shortname">optional unique friendly shortname of the task.</param>
        /// <param name="profile">optional task profile when using a job detached from a pool.</param>
        /// <returns>A new task.</returns>
        public virtual QTask CreateTask(string name, QJob job, AdvancedRanges range, string shortname = default(string), string profile = default(string)) {
            return new QTask(this, name, job, range, shortname, profile);
        }

        /// <summary>
        /// Create a new bucket.
        /// </summary>
        /// <param name="name">The name of the bucket.</param>
        /// <param name="ct">Optional token to cancel the request.</param>
        /// <returns>A new Bucket.</returns>
        public virtual async Task <QBucket> CreateBucketAsync(string name, CancellationToken ct=default(CancellationToken)) {
            return await QBucket.CreateAsync(this, name, create: true, ct: ct);
        }
        #endregion

        #region RetrieveXAsync
        /// <summary>
        /// Retrieve the tasks list. (deprecated)
        /// </summary>
        /// <param name="summary">Obsolete params to get a summary version of a task.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async Task<List<QTask>> RetrieveTasksAsync(bool summary, CancellationToken cancellationToken = default(CancellationToken))
            => await RetrieveTasksAsync(cancellationToken);

        /// <summary>
        /// Retrieve the tasks list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async Task<List<QTask>> RetrieveTasksAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var returnData = new List<QTask>();
            PaginatedResponse<QTask> paginatedResponse;
            var pageDetails = new PaginatedRequest<QTask>();

            do
            {
                paginatedResponse = await RetrievePaginatedTaskAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve the tasks list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async Task<List<QTaskSummary>> RetrieveTaskSummariesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var returnData = new List<QTaskSummary>();
            PaginatedResponse<QTaskSummary> paginatedResponse;
            var pageDetails = new PaginatedRequest<QTaskSummary>();

            do
            {
                paginatedResponse = await RetrievePaginatedTaskSummariesAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve the tasks list with custom filtering.
        /// </summary>
        /// <param name="level">the qtask filter object</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async Task<List<QTask>> RetrieveTasksAsync(QDataDetail<QTask> level, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.PostAsJsonAsync<DataDetailApi<QTask>>("tasks/search", level._dataDetailApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qapiTaskList = await response.Content.ReadAsAsync<List<TaskApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                var ret = new List<QTask>();
                foreach (var item in qapiTaskList) {
                    ret.Add(await QTask.CreateAsync(this, item));
                }
                return ret;
            }
        }

        /// <summary>
        /// Retrieve the tasks list filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for task filtering.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async Task<List<QTask>> RetrieveTasksByTagsAsync(List<string> tags, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (tags == null || tags.Count == 0)
            {
                return await RetrieveTasksAsync(cancellationToken);
            }

            var returnData = new List<QTask>();
            PaginatedResponse<QTask> paginatedResponse;
            var pageDetails = new PaginatedRequest<QTask>()
            {
                Filter = QFilter<QTask>
                    .Or(tags
                        .Select(tag => QFilter<QTask>.Contains(t => t.Tags, tag))
                        .ToArray()),
            };

            do
            {
                paginatedResponse = await RetrievePaginatedTaskAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve the tasks list filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for task filtering.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of tasks.</returns>
        public virtual async Task<List<QTaskSummary>> RetrieveTaskSummariesByTagsAsync(List<string> tags, CancellationToken cancellationToken = default(CancellationToken)) {
            if(tags == null || tags.Count == 0)
                return await RetrieveTaskSummariesAsync(cancellationToken);

            var uri = "tasks/summaries?tag=" + string.Join(",", tags.Select(tag => HttpUtility.UrlEncode(tag)));
            using (var response = await _client.GetAsync(uri, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qapiTaskList = await response.Content.ReadAsAsync<List<TaskApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                var ret = new List<QTaskSummary>();
                foreach (var item in qapiTaskList) {
                    ret.Add(await QTaskSummary.CreateAsync(this, item));
                }
                return ret;
            }
        }

        /// <summary>
        /// Retrieve a page of the tasks list summaries.
        /// </summary>
        /// <param name="pageDetails">The pagination details, with the result number by page, the filters and the token of the page to reach.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A response page with list of tasks.</returns>
        public virtual async Task<PaginatedResponse<QTaskSummary>> RetrievePaginatedTaskSummariesAsync(PaginatedRequest<QTaskSummary> pageDetails, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.PostAsJsonAsync<PaginatedRequestApi<QTaskSummary>>("tasks/summaries/paginate", pageDetails._pageRequestApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qapiTaskSummariesPages = await response.Content.ReadAsAsync<PaginatedResponseAPI<TaskApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await PaginatedResponse<QTaskSummary>.CreateAsync(this, qapiTaskSummariesPages, QTaskSummary.CreateAsync);
            }
        }

        /// <summary>
        /// Retrieve a page of the tasks list.
        /// </summary>
        /// <param name="pageDetails">The pagination details, with the result number by page, the filters and the token of the page to reach.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A page with a list of tasks.</returns>
        public virtual async Task<PaginatedResponse<QTask>> RetrievePaginatedTaskAsync(PaginatedRequest<QTask> pageDetails, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.PostAsJsonAsync<PaginatedRequestApi<QTask>>("tasks/paginate", pageDetails._pageRequestApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qapiTaskPages = await response.Content.ReadAsAsync<PaginatedResponseAPI<TaskApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await PaginatedResponse<QTask>.CreateAsync(this, qapiTaskPages, QTask.CreateAsync);
            }
        }

        /// <summary>
        /// Retrieve the pools list. (deprecated)
        /// </summary>
        /// <param name="summary">Obsolete params to get a summary version of a pool.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual async Task<List<QPool>> RetrievePoolsAsync(bool summary, CancellationToken cancellationToken = default(CancellationToken))
            => await RetrievePoolsAsync(cancellationToken);

        /// <summary>
        /// Retrieve the pools list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual async Task<List<QPool>> RetrievePoolsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var returnData = new List<QPool>();
            PaginatedResponse<QPool> paginatedResponse;
            var pageDetails = new PaginatedRequest<QPool>();

            do
            {
                paginatedResponse = await RetrievePaginatedPoolAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve the pools summary list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual async Task<List<QPoolSummary>> RetrievePoolSummariesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var returnData = new List<QPoolSummary>();
            PaginatedResponse<QPoolSummary> paginatedResponse;
            var pageDetails = new PaginatedRequest<QPoolSummary>();

            do
            {
                paginatedResponse = await RetrievePaginatedPoolSummariesAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve a page of the pools list.
        /// </summary>
        /// <param name="pageDetails">The pagination details, with the result number by page, the filters and the token of the page to reach.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A page with a list of pools.</returns>
        public virtual async Task<PaginatedResponse<QPool>> RetrievePaginatedPoolAsync(PaginatedRequest<QPool> pageDetails, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.PostAsJsonAsync<PaginatedRequestApi<QPool>>("pools/paginate", pageDetails._pageRequestApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var qapiPoolPages = await response.Content.ReadAsAsync<PaginatedResponseAPI<PoolApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await PaginatedResponse<QPool>.CreateAsync(this, qapiPoolPages, QPool.CreateAsync);
            }
        }

        /// <summary>
        /// Retrieve a page of the pools list summaries.
        /// </summary>
        /// <param name="pageDetails">The pagination details, with the result number by page, the filters and the token of the page to reach.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A response page with list of pools.</returns>
        public virtual async Task<PaginatedResponse<QPoolSummary>> RetrievePaginatedPoolSummariesAsync(PaginatedRequest<QPoolSummary> pageDetails, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.PostAsJsonAsync<PaginatedRequestApi<QPoolSummary>>("pools/summaries/paginate", pageDetails._pageRequestApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var qapiPoolSummariesPages = await response.Content.ReadAsAsync<PaginatedResponseAPI<PoolApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await PaginatedResponse<QPoolSummary>.CreateAsync(this, qapiPoolSummariesPages, QPoolSummary.CreateAsync);
            }
        }

        /// <summary>
        /// Retrieve the pools list with custom filtering.
        /// </summary>
        /// <param name="level">the qpool filter object</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual async Task<List<QPool>> RetrievePoolsAsync(QDataDetail<QPool> level, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.PostAsJsonAsync<DataDetailApi<QPool>>("pools/search", level._dataDetailApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qapiPoolList = await response.Content.ReadAsAsync<List<PoolApi>>(Utils.GetCustomResponseFormatter(), cancellationToken);
                var ret = new List<QPool>();
                foreach (var item in qapiPoolList) {
                    ret.Add(new QPool(this, item));
                }
                return ret;
            }
        }

        /// <summary>
        /// Retrieve the pools list filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for pool filtering.</param>
        /// <param name="summary">Optional token to choose between full pools and pools summaries.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of pools.</returns>
        public virtual async Task<List<QPool>> RetrievePoolsByTagsAsync(List<string> tags, bool summary = true, CancellationToken cancellationToken = default(CancellationToken)) {
            if (tags == null || tags.Count == 0)
            {
                return await RetrievePoolsAsync(cancellationToken);
            }

            var returnData = new List<QPool>();
            PaginatedResponse<QPool> paginatedResponse;
            var pageDetails = new PaginatedRequest<QPool>()
            {
                Filter = QFilter<QPool>
                    .Or(tags
                        .Select(tag => QFilter<QPool>.Contains(t => t.Tags, tag))
                        .ToArray()),
            };

            do
            {
                paginatedResponse = await RetrievePaginatedPoolAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve the jobs list with custom filtering.
        /// </summary>
        /// <param name="level">the qjob filter object</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of jobs.</returns>
        public virtual async Task<List<QJob>> RetrieveJobsAsync(QDataDetail<QJob> level, CancellationToken cancellationToken = default(CancellationToken)) {
            var baseUri =  "jobs/search";
            using (var response = await _client.PostAsJsonAsync<DataDetailApi<QJob>>(baseUri, level._dataDetailApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qjoblist = await response.Content.ReadAsAsync<List<JobApi>>(cancellationToken);
                var ret = new List<QJob>();
                foreach (var item in qjoblist) {
                    ret.Add(new QJob(this, item));
                }
                return ret;
            }
        }

        /// <summary>
        /// Retrieve the job list.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of jobs.</returns>
        public virtual async Task<List<QJob>> RetrieveJobsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var returnData = new List<QJob>();
            PaginatedResponse<QJob> paginatedResponse;
            var pageDetails = new PaginatedRequest<QJob>();

            do
            {
                paginatedResponse = await RetrievePaginatedJobAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve the jobs list filtered by tags.
        /// </summary>
        /// <param name="tags">list of tags for job filtering.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>An enumeration of jobs.</returns>
        public virtual async Task<IEnumerable<QJob>> RetrieveJobsByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
        {
            if (tags == null || tags.Count == 0)
            {
                return await RetrieveJobsAsync(cancellationToken);
            }

            var returnData = new List<QJob>();
            PaginatedResponse<QJob> paginatedResponse;
            var pageDetails = new PaginatedRequest<QJob>()
            {
                Filter = QFilter<QJob>
                    .Or(tags
                        .Select(tag => QFilter<QJob>.Contains(t => t.Tags, tag))
                        .ToArray()),
            };

            do
            {
                paginatedResponse = await RetrievePaginatedJobAsync(pageDetails, cancellationToken);
                pageDetails.Token = paginatedResponse.NextToken;
                returnData.AddRange(paginatedResponse.Data);
            } while (paginatedResponse.IsTruncated);

            return returnData;
        }

        /// <summary>
        /// Retrieve a page of the jobs list.
        /// </summary>
        /// <param name="pageDetails">The pagination details, with the result number by page, the filters and the token of the page to reach.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A page with a list of jobs.</returns>
        public virtual async Task<PaginatedResponse<QJob>> RetrievePaginatedJobAsync(PaginatedRequest<QJob> pageDetails, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.PostAsJsonAsync<PaginatedRequestApi<QJob>>("jobs/paginate", pageDetails._pageRequestApi, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var qapiJobPages = await response.Content.ReadAsAsync<PaginatedResponseAPI<JobApi>>(cancellationToken);
                return await PaginatedResponse<QJob>.CreateAsync(this, qapiJobPages, (connec, jApi) => Task.FromResult(new QJob(connec, jApi)));
            }
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
            var s3Config = new OverloadedS3config()
            {
                ServiceURL = StorageUri.ToString(),
                ForcePathStyle = ForceStoragePathStyle,
                MaxErrorRetry = MaxStorageRetry
            };

#if (!NET45)
            if (S3HttpClientFactory != null)
            {
                s3Config.HttpClientFactory = S3HttpClientFactory;
            }
#endif

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
        public virtual async Task<List<QBucket>> RetrieveBucketsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return await RetrieveBucketsAsync(true, cancellationToken);
        }

        /// <summary>
        /// Retrieve the buckets list.
        /// </summary>
        /// <param name="retrieveBucketStats">If set to true, the file count and used space of each bucket is also retrieved. If set to false, it is faster but only the bucket names are returned.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of buckets.</returns>
        public virtual async Task<List<QBucket>> RetrieveBucketsAsync(bool retrieveBucketStats, CancellationToken cancellationToken = default(CancellationToken)) {
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
        /// Retrieve the bucket for the corresponding unique name.
        /// </summary>
        /// <param name="bucketName">Unique name of the bucket.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The bucket.</returns>
        public virtual async Task<QBucket> RetrieveBucketAsync(string bucketName, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var s3Client = await GetS3ClientAsync(cancellationToken)) {
                bool exist = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
                return exist ? await QBucket.CreateAsync(this, bucketName, create: false, ct: cancellationToken) : null;
            }
        }

        /// <summary>
        /// Retrieve a bucket or create one if it does not exist.
        /// </summary>
        /// <param name="bucketName">The name of the bucket.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A new Bucket.</returns>
        public virtual async Task<QBucket> RetrieveOrCreateBucketAsync(string bucketName, CancellationToken cancellationToken = default(CancellationToken)) {
            var bucket = await RetrieveBucketAsync(bucketName, cancellationToken);
            return bucket?? await CreateBucketAsync(bucketName, cancellationToken);
        }

        /// <summary>
        /// Retrieve the rest Api settings.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The rest Api settings.</returns>
        public virtual async Task<ApiSettings> RetrieveApiSettingsAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync("settings", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                return await response.Content.ReadAsAsync<ApiSettings>(cancellationToken);
            }
        }

        /// <summary>
        /// Retrieve the user quotas information for your account.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and buckets information.</returns>
        public virtual async Task<UserInformation> RetrieveUserInformationAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            return await RetrieveUserInformationAsync(true, cancellationToken);
        }

        /// <summary>
        /// Retrieve the user quotas and buckets information for your account.
        /// Note: BucketCount field is retrieved with a second request to the bucket Api.
        /// </summary>
        /// <param name="retrieveBucketCount">If set to false, the BucketCount field is not filled but the request is faster.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The quotas and buckets information without BucketCount.</returns>
        public virtual async Task<UserInformation> RetrieveUserInformationAsync(bool retrieveBucketCount, CancellationToken cancellationToken = default(CancellationToken)) {
            UserInformation u;
            using (var response = await _client.GetAsync("info", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                u = await response.Content.ReadAsAsync<UserInformation>(cancellationToken);
            }

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
        public virtual async Task<List<string>> RetrieveProfilesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync("profiles", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                return await response.Content.ReadAsAsync<List<string>>(cancellationToken);
            }
        }

        /// <summary>
        /// Retrieve the list of the constants you can override for a specific profile.
        /// </summary>
        /// <param name="profile">Name of the profile.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of constants.</returns>
        public virtual async Task<List<Constant>> RetrieveConstantsAsync(string profile, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync("profiles/" + profile, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var list = await response.Content.ReadAsAsync<ProfilesConstantApi>(cancellationToken);
                return list.Constants;
            }
        }

        /// <summary>
        /// Retrieve a page of your user available hardware constraints.
        /// </summary>
        /// <param name="pageOffset">Response page limitation details</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The complete user hardware constraints list.</returns>
        public virtual async Task<OffsetPageResponse<HardwareConstraint>> RetrieveUserHardwareConstraintsPageAsync(OffsetPageRequest pageOffset, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.GetAsync(
                String.Format("/hardware-constraints{0}", pageOffset != default ? String.Format("?{0}", pageOffset.GetAsQueryString()) : ""),
                cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);

                var hardwareConstraints = await response.Content.ReadAsAsync<OffsetPageResponse<HardwareConstraint>>(
                    Utils.GetCustomResponseFormatter(),
                    cancellationToken);
                return hardwareConstraints;
            }
        }

        /// <summary>
        /// Retrieve all available hardware constraints.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The user hardware constraints list.</returns>
        public virtual async Task<IEnumerable<HardwareConstraint>> RetrieveUserHardwareConstraintsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var hardwareConstraints = new List<HardwareConstraint>();
            bool shouldIterate;
            var nextOffset = 0;

            do
            {
                var hardwareConstraintsPage = await RetrieveUserHardwareConstraintsPageAsync(new OffsetPageRequest(Int32.MaxValue, nextOffset), cancellationToken);
                hardwareConstraints.AddRange(hardwareConstraintsPage.Data ?? new List<HardwareConstraint>());
                nextOffset = hardwareConstraintsPage.Limit + hardwareConstraintsPage.Offset;
                shouldIterate = hardwareConstraintsPage.Total > nextOffset;
            } while (shouldIterate);

            return hardwareConstraints;
        }
        #endregion

        #region RetrieveXByNameAsync
        /// <summary>
        /// Retrieve a task by its name.
        /// </summary>
        /// <param name="name">Name of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that name or null if it hasn't been found.</returns>
        [Obsolete("RetrieveTaskByNameAsync is deprecated, please use RetrieveTaskByShortnameAsync or RetrieveTasksByNameAsync instead.")]
        public virtual async Task<QTask> RetrieveTaskByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrieveTasksAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a task summary by its name.
        /// </summary>
        /// <param name="name">Name of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that name or null if it hasn't been found.</returns>
        [Obsolete("RetrieveTaskSummaryByNameAsync is deprecated, please use RetrieveTaskSummaryByShortnameAsync instead.")]
        public virtual async Task<QTaskSummary> RetrieveTaskSummaryByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrieveTaskSummariesAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a task list by there name.
        /// </summary>
        /// <param name="name">Name of the tasks to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task list for that name.</returns>
        public virtual async Task<List<QTask>> RetrieveTasksByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var nameFilter = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Eq<string>(t => t.Name, name)
            };

            var ret = await RetrieveTasksAsync(nameFilter, cancellationToken);
            return ret;
        }

        /// <summary>
        /// Retrieve a task by its shortname.
        /// </summary>
        /// <param name="shortname">shortname of the task to find, if the task shortname has not bean set, the shortname is equivalent to the uuid.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that shortname or null if it hasn't been found.</returns>
        public virtual async Task<QTask> RetrieveTaskByShortnameAsync(string shortname, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(shortname))
            {
                throw new ArgumentException("shortname can not be null or empty.");
            }

            using (var response = await _client.GetAsync($"tasks/{shortname}", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiTask = await response.Content.ReadAsAsync<TaskApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QTask.CreateAsync(this, apiTask);
            }
        }

        /// <summary>
        /// Retrieve a task summary by its shortname.
        /// </summary>
        /// <param name="shortname">Shortname of the task summary to find, if the task shortname has not bean set, the shortname is equivalent to the uuid.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task summary object for that shortname or null if it hasn't been found.</returns>
        public virtual async Task<QTaskSummary> RetrieveTaskSummaryByShortnameAsync(string shortname, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(shortname))
            {
                throw new ArgumentException("shortname can not be null or empty.");
            }

            using (var response = await _client.GetAsync($"tasks/{shortname}", cancellationToken))//TODO:Add the url path for summary when api is ready
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiTask = await response.Content.ReadAsAsync<TaskApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QTaskSummary.CreateAsync(this, apiTask);
            }
        }

        /// <summary>
        /// Retrieve a task by its uuid.
        /// </summary>
        /// <param name="uuid">uuid of the task to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task object for that uuid or null if it hasn't been found.</returns>
        public virtual async Task<QTask> RetrieveTaskByUuidAsync(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync($"tasks/{uuid}", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiTask = await response.Content.ReadAsAsync<TaskApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QTask.CreateAsync(this, apiTask);
            }
        }

        /// <summary>
        /// Retrieve a task summary by its uuid.
        /// </summary>
        /// <param name="uuid">uuid of the task summary to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The task summary object for that uuid or null if it hasn't been found.</returns>
        public virtual async Task<QTaskSummary> RetrieveTaskSummaryByUuidAsync(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync($"tasks/{uuid}", cancellationToken))//TODO:Add the url path for summary when api is ready
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiTask = await response.Content.ReadAsAsync<TaskApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QTaskSummary.CreateAsync(this, apiTask);
            }
        }

        /// <summary>
        /// Retrieve a job by its shortname.
        /// </summary>
        /// <param name="shortname">shortname of the job to find, or it's uuid if no shortname has bin set.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The job object for that shortname or null if it hasn't been found.</returns>
        public virtual async Task<QJob> RetrieveJobByShortnameAsync(string shortname, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(shortname))
            {
                throw new ArgumentException("shortname can not be null or empty.");
            }

            using (var response = await _client.GetAsync($"jobs/{shortname}", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiJob = await response.Content.ReadAsAsync<JobApi>(cancellationToken);
                return new QJob (this, apiJob);
            }
        }

        /// <summary>
        /// Retrieve a job by its uuid or shortname.
        /// </summary>
        /// <param name="uuid">uuid or shortname of the job to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The job object for that uuid or null if it hasn't been found.</returns>
        public virtual async Task<QJob> RetrieveJobByUuidAsync(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync($"jobs/{uuid}", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiJob = await response.Content.ReadAsAsync<JobApi>(cancellationToken);
                return new QJob (this, apiJob);
            }
        }

        /// <summary>
        /// Retrieve a pool by its name.
        /// </summary>
        /// <param name="name">Name of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that name or null if it hasn't been found.</returns>
        [Obsolete("RetrievePoolByNameAsync is deprecated, please use RetrievePoolByShortnameAsync or RetrievePoolsByNameAsync instead.")]
        public virtual async Task<QPool> RetrievePoolByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrievePoolsAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }


        /// <summary>
        /// Retrieve a pool summary by its name.
        /// </summary>
        /// <param name="name">Name of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that name or null if it hasn't been found.</returns>
        [Obsolete("RetrievePoolSummaryByNameAsync is deprecated, please use RetrievePoolSummaryByShortnameAsync instead.")]
        public virtual async Task<QPoolSummary> RetrievePoolSummaryByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken)) {
            var ret = await RetrievePoolSummariesAsync(cancellationToken);
            return ret.Find(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve a pool list by its name.
        /// </summary>
        /// <param name="name">Name of the pools to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool list for that name.</returns>
        public virtual async Task<List<QPool>> RetrievePoolsByNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            var nameFilter = new QDataDetail<QPool>()
            {
                Filter = QFilter<QPool>.Eq<string>(t => t.Name, name)
            };

            var ret = await RetrievePoolsAsync(nameFilter, cancellationToken);
            return ret;
        }
        /// <summary>
        /// Retrieve a pool by its shortname.
        /// </summary>
        /// <param name="shortname">shortname of the pool to find. (if shortname is not set, the shortname is equivalant to the uuid).</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that shortname or null if it hasn't been found.</returns>
        public virtual async Task<QPool> RetrievePoolByShortnameAsync(string shortname, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(shortname))
            {
                throw new ArgumentException("shortname can not be null or empty.");
            }

            using (var response = await _client.GetAsync($"pools/{shortname}", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiPool = await response.Content.ReadAsAsync<PoolApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QPool.CreateAsync(this, apiPool);
            }
        }

        /// <summary>
        /// Retrieve a pool summary by its shortname.
        /// </summary>
        /// <param name="shortname">shortname of the pool summary to find. (if shortname is not set, the shortname is equivalant to the uuid).</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool summary object for that shortname or null if it hasn't been found.</returns>
        public virtual async Task<QPoolSummary> RetrievePoolSummaryByShortnameAsync(string shortname, CancellationToken cancellationToken = default(CancellationToken)) {
            if (string.IsNullOrWhiteSpace(shortname))
            {
                throw new ArgumentException("shortname can not be null or empty.");
            }

            using (var response = await _client.GetAsync($"pools/{shortname}", cancellationToken))//TODO:Add the url path for summary when api is ready
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiPool = await response.Content.ReadAsAsync<PoolApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QPoolSummary.CreateAsync(this, apiPool);
            }
        }
        /// <summary>
        /// Retrieve a pool by its uuid or shortname.
        /// </summary>
        /// <param name="uuid">uuid or shortname of the pool to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool object for that uuid or null if it hasn't been found.</returns>
        public virtual async Task<QPool> RetrievePoolByUuidAsync(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync($"pools/{uuid}", cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiPool = await response.Content.ReadAsAsync<PoolApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QPool.CreateAsync(this, apiPool);
            }
        }

        /// <summary>
        /// Retrieve a pool summary by its uuid or shortname.
        /// </summary>
        /// <param name="uuid">uuid or shortname of the pool summary to find.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>The pool summary object for that uuid or null if it hasn't been found.</returns>
        public virtual async Task<QPoolSummary> RetrievePoolSummaryByUuidAsync(string uuid, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var response = await _client.GetAsync($"pools/{uuid}", cancellationToken))//TODO:Add the url path for summary when api is ready
            {
                await Utils.LookForErrorAndThrowAsync(_client, response, cancellationToken);
                var apiPool = await response.Content.ReadAsAsync<PoolApi>(Utils.GetCustomResponseFormatter(), cancellationToken);
                return await QPoolSummary.CreateAsync(this, apiPool);
            }
        }

        #endregion

        #region CreateXAsync
        /// <summary>
        /// Submit a list of task as a bulk.
        /// </summary>
        /// <param name="tasks">The task list to submit as a bulk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>void.</returns>
        public virtual async Task SubmitTasksAsync(List<QTask> tasks, CancellationToken cancellationToken = default(CancellationToken)) {
            List<QBulkTaskResponse> results;
            await Task.WhenAll(tasks.Select(task => task.PreSubmitAsync(cancellationToken)));
            using (var response = await _client.PostAsJsonAsync<List<TaskApi>>("tasks", tasks.Select(t => t._taskApi).ToList(), cancellationToken))
                results = await response.Content.ReadAsAsync<List<QBulkTaskResponse>>(Utils.GetCustomResponseFormatter(), cancellationToken);

            // The "contract" with the api is that response should come in the same order as submission
            var errorMessage = String.Empty;
            var postTasks = new List<Task>();

            for (int i = 0; i < tasks.Count; i++) {
                if (!results[i].IsSuccesResponse) {
                    errorMessage += $"[{tasks[i].Name}] : {results[i].StatusCode}, {results[i].Message}\n";
                } else {
                    postTasks.Add(tasks[i].PostSubmitAsync(new TaskApi() { Uuid = results[i].Uuid.GetValueOrDefault() }, cancellationToken));
                }
            }
            await Task.WhenAll(postTasks);

            // Notify user that something went partially wrong.
            if (!String.IsNullOrEmpty(errorMessage)) {
                throw new QarnotApiException(errorMessage);
            }
        }

        #endregion
    }
}

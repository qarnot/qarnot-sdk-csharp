using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using System;


namespace QarnotSDK {
    /// <summary>
    /// Job states.
    /// </summary>
    public sealed class QJobStates {
        /// <summary>
        /// The Job is active and ready to receive tasks.
        /// </summary>
        public static readonly string Active = "Active";
        /// <summary>
        /// The job is being completed
        /// </summary>
        public static readonly string Terminating = "Terminating";
        /// <summary>
        /// The Job is completed. (we can't dispatch in it anymore)
        /// </summary>
        public static readonly string Completed = "Completed";
        /// <summary>
        /// The job is currently being deleted.
        /// </summary>
        public static readonly string Deleting = "Deleting";
    }

    /// <summary>
    /// This class manages Jobs life cycle: logical group of tasks.
    /// </summary>
    public partial class QJob {

        /// <summary>
        /// Reference to the api connection.
        /// </summary>
        protected Connection _api;

        /// <summary>
        /// The job resource uri.
        /// </summary>
        protected string _uri = null;

        internal JobApi _jobApi { get; set; }

        /// <summary>
        /// The inner Connection object.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public Connection Connection { get { return _api; } }

        /// <summary>
        /// The job unique identifier. The Uuid is generated by the Api when the job is created.
        /// </summary>
        [InternalDataApiName(Name="Uuid")]
        public Guid Uuid { get { return _jobApi.Uuid; } }

        /// <summary>
        /// The Job name
        /// </summary>
        [InternalDataApiName(Name="Name")]
        public string Name 
        {
            get => _jobApi.Name;
            set => _jobApi.Name = value;
        }

        /// <summary>
        /// The related pool uuid
        /// </summary>
        [InternalDataApiName(Name="PoolUuid")]
        public Guid PoolUuid { get => _jobApi.PoolUuid.IsNullOrEmpty() ? default(Guid) : new Guid(_jobApi.PoolUuid); }

        /// <summary>
        /// Retrieve the job state (see QJobStates).
        /// Available only after the submission. Use UpdateStatus or UpdateStatusAsync to refresh.
        /// </summary>
        /// 
        [InternalDataApiName(Name="State")]
        public string State { get => _jobApi.State; }

        /// <summary>
        /// The Job creation date
        /// </summary>
        [InternalDataApiName(Name="CreationDate")]
        public DateTime CreationDate { get => _jobApi.CreationDate; }

        /// <summary>
        /// The Job last modified date
        /// </summary>
        [InternalDataApiName(Name="LastModified")]
        public DateTime LastModified { get => _jobApi.LastModified; }

        /// <summary>
        /// Boolean to indicate if the job use dependency behaviour
        /// </summary>
        [InternalDataApiName(Name="UseDependencies")]
        public bool UseDependencies
        {
            get => _jobApi.UseDependencies;
            set => _jobApi.UseDependencies = value;
        }

        /// <summary>
        /// Wall time limit for the job execution.
        /// Once this this time duration exceeded, the whole job will terminate
        /// and the tasks linked will be canceled
        /// </summary>
        [InternalDataApiName(Name="MaxWallTime", IsFilterable=false, IsSelectable=false)]
        public TimeSpan MaximumWallTime
        {
            get
            {
                if (_jobApi.MaxWallTime.HasValue)
                    return _jobApi.MaxWallTime.Value;
                return default(TimeSpan);
            }
            set
            {
                _jobApi.MaxWallTime = value;
            }
        }

        /// <summary>
        /// The pool the job is attached to. null if the job is not attached to a pool. 
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public QPool Pool
        {
            get
            {
                if (_jobApi.PoolUuid.IsNullOrEmpty())
                    return null;
                return new QPool(_api, new Guid(_jobApi.PoolUuid));
            }
        }

        internal QJob(Connection qapi, JobApi jobApi)
        {
            _api = qapi;
            _uri = "jobs/" + jobApi.Uuid.ToString();
            _jobApi = jobApi;
        }

        /// <summary>
        /// Create a new job.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="name">The job name.</param>
        /// <param name="pool">The pool we want the job to be attached to.</param>
        /// <param name="UseTaskDependencies">Bool to allow use of dependencies for tasks in this job.</param>
        public QJob(Connection connection, string name = default(string), QPool pool=null, bool UseTaskDependencies=false)
            : this (connection, new JobApi())
        {
            _jobApi.Name = name;
            if (pool != null)
                _jobApi.PoolUuid = pool.Uuid.ToString();
            _jobApi.UseDependencies = UseTaskDependencies;
        }

        /// <summary>
        /// Create a job object given an existing Uuid.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="uuid">The Uuid of an already existing job.</param>
        public QJob(Connection connection, Guid uuid) : this(connection, new JobApi())
        {
            _uri = "jobs/" + uuid.ToString();
            _jobApi.Uuid = uuid;
        }

        /// <summary>
        /// Submit this job.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task SubmitAsync(CancellationToken cancellationToken=default(CancellationToken))
        {
            if (_api.IsReadOnly) throw new Exception("Can't submit jobs, this connection is configured in read-only mode");

            var response = await _api._client.PostAsJsonAsync<JobApi>("jobs", _jobApi, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response,cancellationToken);
            var result = await response.Content.ReadAsAsync<JobApi>(cancellationToken);
            await PostSubmitAsync(result, cancellationToken);
        }

        /// <summary>
        /// Update this job.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task UpdateStatusAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            var response = await _api._client.GetAsync(_uri, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response, cancellationToken);
            var result = await response.Content.ReadAsAsync<JobApi>(cancellationToken);
            _jobApi = result;
        }

        /// <summary>
        /// Terminate an active job. (will cancel all remaining tasks)
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task TerminateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_api.IsReadOnly) throw new Exception("Can't terminate jobs, this connection is configured in read-only mode");
            var response = await _api._client.PostAsync(_uri + "/terminate", null, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response, cancellationToken);
        }

        /// <summary>
        /// Delete the job. If the job is active, the job is terminated and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_api.IsReadOnly) throw new Exception("Can't delete jobs, this connection is configured in read-only mode");
            var response = await _api._client.DeleteAsync(_uri, cancellationToken);
            await Utils.LookForErrorAndThrowAsync(_api._client, response, cancellationToken);
        }

        #region internals

        internal async Task PostSubmitAsync(JobApi result, CancellationToken cancellationToken = default(CancellationToken))
         {
            _jobApi.Uuid = result.Uuid;
            _uri = "jobs/" + _jobApi.Uuid.ToString();
            await UpdateStatusAsync(cancellationToken);
        }
        #endregion
    }
}
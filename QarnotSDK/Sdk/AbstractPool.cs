using System.Threading.Tasks;
using System.Threading;
using System;
using System.Net.Http;

namespace QarnotSDK {

    /// <summary>
    /// This class manages pools life cycle: submission, monitor, delete.
    /// </summary>
    public abstract partial class AQPool {

        /// <summary>
        /// Reference to the api connection.
        /// </summary>
        protected Connection _api;

        /// <summary>
        /// The pool resource uri.
        /// </summary>
        protected string _uri = null;

        internal PoolApi _poolApi { get; set; }

        /// <summary>
        /// The inner Connection object.
        /// </summary>
        [InternalDataApiName(IsFilterable=false, IsSelectable=false)]
        public virtual Connection Connection { get { return _api; } }

        /// <summary>
        /// The pool unique identifier. The Uuid is generated by the Api when the pool is submitted.
        /// </summary>
        [InternalDataApiName(Name="Uuid")]
        public virtual Guid Uuid { get { return _poolApi.Uuid; } }

        internal AQPool() { }

        internal AQPool(Connection qapi, PoolApi poolApi) {
            _api = qapi;
            _uri = "pools/" + poolApi.Uuid.ToString();
            _poolApi = poolApi;
        }

        internal virtual async Task<AQPool> InitializeAsync(Connection qapi, PoolApi poolApi) {
            _api = qapi;
            _uri = "pools/" + poolApi.Uuid.ToString();
            _poolApi = poolApi;
            return await Task.FromResult<AQPool>(this);
        }

        #region public methods
        /// <summary>
        /// Delete the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        [Obsolete("use CloseAsync")]
        public virtual async Task StopAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't stop pools, this connection is configured in read-only mode");
            using (var response = await _api._client.DeleteAsync(_uri, cancellationToken))
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
        }

        /// <summary>
        /// Close the pool.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public virtual async Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't close pools, this connection is configured in read-only mode");
            using (var response = await _api._client.PostAsync(_uri + "/close", null, cancellationToken))
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
        }

        /// <summary>
        /// Delete the pool. If the pool is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        public abstract Task DeleteAsync(CancellationToken cancellationToken, bool failIfDoesntExist = false, bool purgeResources=false);

        /// <summary>
        /// Delete the pool. If the pool is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(bool failIfDoesntExist = false, bool purgeResources=false)
            => await DeleteAsync(default(CancellationToken), failIfDoesntExist, purgeResources); 
        #endregion

        /// <summary>
        /// Request made on a running pool to re-sync the resource buckets to the compute nodes.
        ///  1 - Upload new files on your resource bucket,
        ///  2 - Call this method,
        ///  3 - The new files will appear on all the compute nodes in the $DOCKER_WORKDIR folder
        /// Note: There is no way to know when the files are effectively transfered. This information is available on the compute node only.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task UpdateResourcesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_api.IsReadOnly) 
            {
                throw new Exception("Can't update resources, this connection is configured in read-only mode");
            }
            var reqMsg = new HttpRequestMessage(new HttpMethod("PATCH"), _uri);
            using (var response = await _api._client.SendAsync(reqMsg, cancellationToken))
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
            }
        }
    }
}

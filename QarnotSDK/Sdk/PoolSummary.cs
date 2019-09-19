using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Linq;
using System;


namespace QarnotSDK {

    /// <summary>
    /// This class manages pools life cycle: submission, monitor, delete.
    /// </summary>
    public partial class QPoolSummary : AQPool {

        /// <summary>
        /// The pool shortname identifier. The shortname is provided by the user. It has to be unique.
        /// </summary>
        public virtual string Shortname { get { return _poolApi.Shortname == null ? _poolApi.Uuid.ToString() : _poolApi.Shortname; } }
        /// <summary>
        /// The pool name.
        /// </summary>
        public virtual string Name { get { return _poolApi.Name; } }
        /// <summary>
        /// The pool profile.
        /// </summary>
        public virtual string Profile { get { return _poolApi.Profile; } }


        /// <summary>
        /// Retrieve the pool state (see QTaskStates).
        /// Available only after the submission.
        /// </summary>
        public virtual string State { get { return _poolApi != null ? _poolApi.State : null; } }

        /// <summary>
        /// The pool creation date.
        /// Available only after the submission.
        /// </summary>
        public virtual DateTime CreationDate { get { return _poolApi.CreationDate; } }

        internal QPoolSummary() { }

        internal QPoolSummary(Connection qapi, PoolApi poolApi) : base(qapi, poolApi) { }

        internal async new Task<QPoolSummary> InitializeAsync(Connection qapi, PoolApi poolApi) {
            await base.InitializeAsync(qapi, poolApi);
             _uri = "pool/" + poolApi.Uuid.ToString();
            _poolApi = poolApi;
            return this;
        }

        internal async static Task<QPoolSummary> CreateAsync(Connection qapi, PoolApi poolApi) {
            return await new QPoolSummary().InitializeAsync(qapi, poolApi);
        }

        /// <summary>
        /// Delete the pool. If the pool is running, the pool is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the pool doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <returns></returns>
        public override async Task DeleteAsync(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false)
        {
            if (_api.IsReadOnly) throw new Exception("Can't delete pools, this connection is configured in read-only mode");

            // the summary pool hasn't the resources and the results. (switching to fullPool for purging)
            if (purgeResources) {
                var fullPool = await GetFullQPoolAsync(cancellationToken);
                await fullPool.DeleteAsync(cancellationToken, failIfDoesntExist, purgeResources);
            }
            else {
                try {
                    using (var response = await _api._client.DeleteAsync(_uri, cancellationToken))
                        await Utils.LookForErrorAndThrowAsync(_api._client, response);
                } catch (QarnotApiResourceNotFoundException ex) {
                    if (failIfDoesntExist) throw ex;
                }
            }
        }

        private void SyncFromApiObject(PoolApi result) {
            _poolApi = result;
        }

        /// <summary>
        /// Get The Full Pool from this pool summary.
        /// <param name="ct">Optional token to cancel the request.</param>
        /// </summary>
        public virtual async Task<QPool> GetFullQPoolAsync(CancellationToken ct = default(CancellationToken)) {
            using (var response = await _api._client.GetAsync(_uri, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response);
                var result = await response.Content.ReadAsAsync<PoolApi>();
                return await QPool.CreateAsync(Connection, result);
            }
        }
    }
}

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
        public string Shortname { get { return _poolApi.Shortname == null ? _poolApi.Uuid.ToString() : _poolApi.Shortname; } }
        /// <summary>
        /// The pool name.
        /// </summary>
        public string Name { get { return _poolApi.Name; } }
        /// <summary>
        /// The pool profile.
        /// </summary>
        public string Profile { get { return _poolApi.Profile; } }


        /// <summary>
        /// Retrieve the pool state (see QTaskStates).
        /// Available only after the submission.
        /// </summary>
        public string State { get { return _poolApi != null ? _poolApi.State : null; } }

        /// <summary>
        /// The pool creation date.
        /// Available only after the submission.
        /// </summary>
        public DateTime CreationDate { get { return _poolApi.CreationDate; } }

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

        private void SyncFromApiObject(PoolApi result) {
            _poolApi = result;
        }

        /// <summary>
        /// Get The Full Pool from this task summary.
        /// <param name="ct">Optional token to cancel the request.</param>
        /// </summary>
        public async Task<QPool> GetFullQPoolAsync(CancellationToken ct = default(CancellationToken)) {
            var response = await _api._client.GetAsync(_uri, ct);
            await Utils.LookForErrorAndThrowAsync(_api._client, response);

            var result = await response.Content.ReadAsAsync<PoolApi>();
            return await QPool.CreateAsync(Connection, result);
        }
    }
}

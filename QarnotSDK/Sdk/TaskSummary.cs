using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;


namespace QarnotSDK {

    /// <summary>
    /// This class manages tasks life cycle: submission, monitor, delete.
    /// </summary>
    public partial class QTaskSummary : AQTask {

        private AdvancedRanges _advancedRange = null;

        /// <summary>
        /// The task shortname identifier. The shortname is provided by the user. It has to be unique.
        /// </summary>
        public virtual string Shortname { get { return _taskApi.Shortname == null ? _taskApi.Uuid.ToString() : _taskApi.Shortname; } }
        /// <summary>
        /// The task name.
        /// </summary>
        public virtual string Name { get { return _taskApi.Name; } }
        /// <summary>
        /// The task profile.
        /// </summary>
        public virtual string Profile { get { return _taskApi.Profile; } }


        /// <summary>
        /// Retrieve the task state (see QTaskStates).
        /// Available only after the submission.
        /// </summary>
        public virtual string State { get { return _taskApi != null ? _taskApi.State : null; } }

        /// <summary>
        /// The task creation date.
        /// Available only after the submission.
        /// </summary>
        public virtual DateTime CreationDate { get { return _taskApi.CreationDate; } }

        /// <summary>
        /// The pool where the task is running or null if the task doesn't belong to a pool.
        /// </summary>
        public virtual QPool Pool { get { return (_taskApi.PoolUuid == null || _taskApi.PoolUuid == Guid.Empty.ToString()) ? null : new QPool(_api, new Guid(_taskApi.PoolUuid)); } }

        /// <summary>
        /// True if the task is completed or false if the task is still running or deploying.
        /// </summary>
        public virtual bool Completed {
            get {
                return State == QTaskStates.Success || State == QTaskStates.Failure || State == QTaskStates.Cancelled;
            }
        }

        /// <summary>
        /// True if the task is executing (PartiallyExecuting or FullyExecuting) or false if the task is in another state.
        /// </summary>
        public virtual bool Executing {
            get {
                return State == QTaskStates.PartiallyExecuting || State == QTaskStates.FullyExecuting;
            }
        }

        /// <summary>
        /// How many times this task have to run.
        /// </summary>
        public virtual uint InstanceCount {
            get {
                if (_advancedRange == null) return _taskApi.InstanceCount;
                else return _advancedRange.Count;
            }
        }


        internal QTaskSummary() { }

        internal QTaskSummary(Connection qapi, TaskApi taskApi) : base(qapi, taskApi) { }

        internal async new Task<QTaskSummary> InitializeAsync(Connection qapi, TaskApi taskApi) {
            await base.InitializeAsync(qapi, taskApi);
             _uri = "tasks/" + taskApi.Uuid.ToString();
            await SyncFromApiObjectAsync(taskApi);
            return this;
        }

        internal async static Task<QTaskSummary> CreateAsync(Connection qapi, TaskApi taskApi) {
            return await new QTaskSummary().InitializeAsync(qapi, taskApi);
        }

        /// <summary>
        /// Delete the task. If the task is running, the task is aborted and deleted.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <param name="failIfDoesntExist">If set to false and the task doesn't exist, no exception is thrown. Default is true.</param>
        /// <param name="purgeResources">Boolean to trigger all resource storages deletion. Default is false.</param>
        /// <param name="purgeResults">Boolean to trigger result storage deletion. Default is false.</param>
        /// <returns></returns>
        public override async Task DeleteAsync(CancellationToken cancellationToken, bool failIfDoesntExist = false,
            bool purgeResources=false, bool purgeResults=false) {
            if (_api.IsReadOnly) throw new Exception("Can't delete tasks, this connection is configured in read-only mode");

            // the summary task hasn't the resources and the results. (switching to fullTask for purging)
            if (purgeResources || purgeResults) {
                var fullTask = await GetFullQTaskAsync(cancellationToken);
                await fullTask.DeleteAsync(cancellationToken, failIfDoesntExist, purgeResources, purgeResults);
            }
            else {
                try {
                    using(var response = await _api._client.DeleteAsync(_uri, cancellationToken))
                        await Utils.LookForErrorAndThrowAsync(_api._client, response, cancellationToken);
                } catch (QarnotApiResourceNotFoundException ex) {
                    if (failIfDoesntExist) throw ex;
                }
            }
        }

        private async Task SyncFromApiObjectAsync(TaskApi result) {
            _taskApi = result;
            if (_taskApi.AdvancedRanges != null) _advancedRange = new AdvancedRanges(_taskApi.AdvancedRanges);
            else _advancedRange = null;
            await Task.FromResult(0);
        }

        #region helpers
        /// <summary>
        /// Enumeration on the task instance ids.
        /// Useful if an advanced range is used.
        /// </summary>
        public virtual IEnumerable<UInt32> Instances {
            get {
                if (_advancedRange != null) {
                    foreach (var i in _advancedRange)
                        yield return i;
                } else {
                    for (UInt32 i = 0; i < _taskApi.InstanceCount; i++)
                        yield return i;
                }
            }
        }
        #endregion

        /// <summary>
        /// Get The Full Task from this task summary.
        /// <param name="ct">Optional token to cancel the request.</param>
        /// </summary>
        public virtual async Task<QTask> GetFullQTaskAsync(CancellationToken ct = default(CancellationToken)) {
            using (var response = await _api._client.GetAsync(_uri, ct))
            {
                await Utils.LookForErrorAndThrowAsync(_api._client, response, ct);
                var result = await response.Content.ReadAsAsync<TaskApi>();
                return await QTask.CreateAsync(Connection, result);
            }
        }
    }
}

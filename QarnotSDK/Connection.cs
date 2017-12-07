using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace QarnotSDK
{
    public class Connection
    {
        internal HttpClient _client;

        public bool HasShortnameFeature { get; set; }
        public bool HasDiskShortnameFeature { get; set; }

        public Connection (string uri, string auth, HttpClientHandler httpClientHandler = null)
        {
            HasShortnameFeature = false;
            HasDiskShortnameFeature = false;
            _client = httpClientHandler == null ? new HttpClient ():new HttpClient(httpClientHandler);
            _client.BaseAddress = new Uri (uri);
            _client.DefaultRequestHeaders.Clear ();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue (auth);
        }

        #region CreateX
        public QPool CreatePool(string name, string profile = null, uint initialNodeCount = 0) {
            var pool = new QPool(this, name, profile, initialNodeCount);
            return pool;
        }

        public QTask CreateTask(string name, string profile, uint frameCount) {
            var task = new QTask(this, name, profile, frameCount);
            return task;
        }

        public QDisk CreateDisk(string name) {
            var disk = new QDisk(this, name);
            return disk;
        }
        #endregion

        #region RetrieveXAsync
        public async Task<List<QTask>> RetrieveTasksAsync() {
            var ret = new List<QTask>();

            var response = await _client.GetAsync("tasks");
            if (response.IsSuccessStatusCode) {
                var qapiTaskList = await response.Content.ReadAsAsync<List<TaskApi>>();

                foreach (var item in qapiTaskList) {
                    ret.Add(new QTask(this, item));
                }
            }
            return ret;
        }

        public async Task<List<QPool>> RetrievePoolsAsync() {
            var ret = new List<QPool>();

            var response = await _client.GetAsync("pools");
            if (response.IsSuccessStatusCode) {
                var list = await response.Content.ReadAsAsync<List<PoolApi>>();

                foreach (var item in list) {
                    ret.Add(new QPool(this, item));
                }
            }
            return ret;
        }

        public async Task<List<QDisk>> RetrieveDisksAsync() {
            var ret = new List<QDisk>();

            var response = await _client.GetAsync("disks");
            if (response.IsSuccessStatusCode) {
                var list = await response.Content.ReadAsAsync<List<DiskApi>>();

                foreach (var item in list) {
                    ret.Add(new QDisk(this, item));
                }
            }
            return ret;
        }
        #endregion

        #region RetrieveXByNameAsync
        public async Task<QTask> RetrieveTaskByNameAsync(string taskName) {
            var ret = await RetrieveTasksAsync();
            return ret.Find(x => x.Name == taskName);
        }

        public async Task<QPool> RetrievePoolByNameAsync(string poolName) {
            var ret = await RetrievePoolsAsync();
            return ret.Find(x => x.Name == poolName);
        }

        public async Task<QDisk> RetrieveDiskByNameAsync(string diskName) {
            var ret = await RetrieveDisksAsync();
            return ret.Find(x => x.Description == diskName);
        }
        #endregion

        #region Async wrappers
        public List<QPool> RetrievePools() {
            return RetrievePoolsAsync().Result;
        }

        public List<QTask> RetrieveTasks() {
            return RetrieveTasksAsync().Result;
        }

        public List<QDisk> RetrieveDisks() {
            return RetrieveDisksAsync().Result;
        }
        #endregion
    }
}

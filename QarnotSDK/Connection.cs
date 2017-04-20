using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace qarnotsdk
{
    public class Connection
    {
        internal HttpClient _client;
        internal JsonMediaTypeFormatter _formatter;

        public Connection (string uri, string auth, HttpClientHandler httpClientHandler = null)
        {
            _client = httpClientHandler == null ? new HttpClient ():new HttpClient(httpClientHandler);
            _client.BaseAddress = new Uri (uri);
            _client.DefaultRequestHeaders.Clear ();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue (auth);
            _formatter = new JsonMediaTypeFormatter();
            _formatter.SerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public QTask CreateTask(string name, string profile, uint frameCount)
        {
            var task = new QTask (this, name, profile, frameCount);
            return task;
        }

        public async Task<QDisk> CreateDiskAsync(string description) {
            var dapi = new DiskApi(description, false);
            var response = await _client.PostAsync<DiskApi>("disks", dapi, _formatter); //create disk
            Utils.LookForErrorAndThrow(_client, response);

            // Retrieve the guid from the response and assign it to the DiskApi
            var result = await response.Content.ReadAsAsync<DiskApi>();
            dapi.Uuid = result.Uuid;
            return new QDisk(_client, dapi);
        }

        public async Task<List<QTask>> RetrieveTasksAsync()
        {
            List<QTask> ret = new List<QTask> ();

            var response = await _client.GetAsync("tasks");
            if (response.IsSuccessStatusCode) {
                var qapiTaskList = await response.Content.ReadAsAsync<List<TaskApi>>();

                foreach (var item in qapiTaskList) {
                    ret.Add(new QTask(this, item));
                }
            }
            return ret;
        }

        public async Task<QTask> RetrieveTaskByNameAsync(string taskName) {
            var ret = await RetrieveTasksAsync();
            return ret.Find(x => x.Name == taskName);
        }

        public async Task<QDisk> RetrieveDiskByNameAsync(string diskName) {
            var ret = await RetrieveDisksAsync();
            return ret.Find(x => x.Description == diskName);
        }

        public async Task<List<QDisk>> RetrieveDisksAsync()
        {
            var ret = new List<QDisk>();

            var response = await _client.GetAsync("disks");
            if (response.IsSuccessStatusCode) {
                var qapiDiskList = await response.Content.ReadAsAsync<List<DiskApi>>();

                foreach (var item in qapiDiskList) {
                    ret.Add(new QDisk(_client, item));
                }
            }
            return ret;
        }

        #region Async wrappers
        public QDisk CreateDisk(string description) {
            return CreateDiskAsync(description).Result;
        }

        public List<QTask> RetrieveTasks() {
            return RetrieveTasksAsync().Result;
        }

        public List<QDisk> RetrieveDisks() {
            return RetrieveDisksAsync().Result;
        }

        public QDisk RetrieveDisk(Guid guid) {
            var ttask = _client.GetAsync("disks/" + guid.ToString());

            ttask.Wait();
            var response = ttask.Result;
            if (response.IsSuccessStatusCode) {
                var ttask2 = response.Content.ReadAsAsync<DiskApi>();
                ttask2.Wait();
                var r = ttask2.Result;
                return new QDisk(_client, r);
            }
            return null;
        }
        #endregion
    }
}

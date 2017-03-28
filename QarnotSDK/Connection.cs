using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace qarnotsdk
{
    public class Connection
    {
        internal HttpClient _client;
        internal JsonMediaTypeFormatter _formatter;
        public Connection (string uri, string auth)
        {
            _client = new HttpClient ();
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

        public QDisk CreateDisk(string description)
        {
            var dapi = new DiskApi (description, false);
            var response = _client.PostAsync ("disks", dapi, _formatter).Result; //create disk
            Utils.LookForErrorAndThrow (_client, response);
            return new QDisk (_client, dapi);
        }

        public List<QTask> RetrieveTasks()
        {
            List<QTask> r = new List<QTask> ();

            var ttask = _client.GetAsync ("tasks");
            ttask.Wait ();
            var response = ttask.Result;
            if (response.IsSuccessStatusCode) {
                var ttask2 = response.Content.ReadAsAsync<List<TaskApi>> ();
                ttask2.Wait ();
                var qapiTaskList = ttask2.Result;

                foreach (var item in qapiTaskList) {
                    r.Add(new QTask(this, item));
                }
            }
            return r;
        }

        public QDisk RetrieveDisk(Guid guid)
        {
            var ttask = _client.GetAsync ("disks/" + guid.ToString());

            ttask.Wait ();
            var response = ttask.Result;
            if (response.IsSuccessStatusCode) {
                var ttask2 = response.Content.ReadAsAsync<DiskApi> ();
                ttask2.Wait ();
                var r = ttask2.Result;
                return new QDisk (_client, r);
            }
            return null;
        }

        public List<QDisk> RetrieveDisks()
        {
            var ret = new List<QDisk> ();

            var ttask = _client.GetAsync ("disks");
            ttask.Wait ();
            var response = ttask.Result;
            if (response.IsSuccessStatusCode) {
                var ttask2 = response.Content.ReadAsAsync<List<DiskApi>> ();
                ttask2.Wait ();
                var qapiDiskList = ttask2.Result;

                foreach (var item in qapiDiskList) {
                    ret.Add (new QDisk (_client, item));
                }
            }

            return ret;
        }
    }
}


using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace qarnotsdk
{
    public class QDisk
    {
        private readonly HttpClient _client;
        private DiskApi _diskApi;
        private readonly string _diskUri;
        private CancellationTokenSource _downloadSource = null;

        public string Description { get { return _diskApi.Description; } }

        public Guid Uuid { get { return _diskApi.Uuid; } }

        public int FileCount { get { return _diskApi.FileCount;  } }

        public long UsedSpaceBytes { get { return _diskApi.UsedSpaceBytes; } }

        public DateTime CreationDate { get { return _diskApi.CreationDate; } }

        public bool Locked { get { return _diskApi.Locked; } }

        internal QDisk (HttpClient client, DiskApi diskApi)
        {
            _client = client;
            _diskApi = diskApi;
            if (_diskApi.Uuid.Equals (default(Guid))) {
                var r = _client.PostAsJsonAsync<DiskApi> ("disks", _diskApi).Result;

                DiskApi da = r.Content.ReadAsAsync<DiskApi> (new CancellationToken ()).Result;
                _diskApi = da;
            }
            _diskUri = "disks/" + _diskApi.Uuid.ToString () + "/";
        }

        public void Cancel()
        {
            if (_downloadSource != null)
                _downloadSource.Cancel ();
        }

        public async Task UpdateAsync() {
            string uri = "disks/" + _diskApi.Uuid.ToString();
            var response = await _client.GetAsync(uri); //create disk
            Utils.LookForErrorAndThrow(_client, response);

            // Retrieve the guid from the response and assign it to the DiskApi
            var result = await response.Content.ReadAsAsync<DiskApi>();
            _diskApi = result;
        }

        public async Task DownloadAsync(string outdir)
        {
            _downloadSource = new CancellationTokenSource ();
            await DownloadAsync (outdir, _downloadSource.Token);
        }

        public async Task DownloadAsync(string outDir, CancellationToken cancellationToken)
        {
            string resDiskUri = "disks/tree/" + _diskApi.Uuid.ToString ();
            var response = await _client.GetAsync (resDiskUri);
            Utils.LookForErrorAndThrow (_client, response);
            List<MyFile> resFiles = await response.Content.ReadAsAsync<List<MyFile>> (cancellationToken);
            var tasks = new Task[resFiles.Count];
            uint index = 0;
            try {
                foreach (var item in resFiles) {
                    tasks [index++] = Utils.Download (_client, _diskUri, item.Name, outDir, cancellationToken);
                }
                await Task.WhenAll (tasks);
            } catch (Exception ex) {
                //Console.WriteLine (ex.Message);
                throw ex;
            }
        }

        private async Task UploadAsync(string diskUri, Stream sourceStream, string remoteName, CancellationToken cancellationToken)
        {
            var requestContent = new MultipartFormDataContent ();
            var fileContent = new StreamContent(sourceStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse ("application/octet-stream");
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse ("attachment; filename=" + Path.GetFileName (remoteName));
            requestContent.Add (fileContent, Path.GetFileNameWithoutExtension (remoteName), Path.GetFileName (remoteName));

            var response = await _client.PostAsync (diskUri + Path.GetDirectoryName(remoteName), requestContent, cancellationToken);
            Utils.LookForErrorAndThrow (_client, response);
        }

        public async Task AddFileAsync(string filePath, string remote=null)
        {
            if (!File.Exists (filePath))
                throw new IOException ("No such file " + filePath);

            var remoteName = remote ?? Path.GetFileName (filePath);
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                await UploadAsync(_diskUri, fs, remoteName, new CancellationToken());
            }
        }

        public async Task AddBytesAsync(string filename, byte[] data) {
            using (var fs = new MemoryStream(data)) {
                await UploadAsync(_diskUri, fs, filename, new CancellationToken());
            }
        }

        public async Task DeleteAsync()
        {
            var response = await _client.DeleteAsync (_diskUri);
            response.EnsureSuccessStatusCode ();
        }

        #region Async wrappers
        public void Download(string outdir) {
            _downloadSource = new CancellationTokenSource();
            DownloadAsync(outdir, _downloadSource.Token).Wait();
        }

        private void Upload(string diskUri, Stream sourceStream, string remoteName, CancellationToken cancellationToken) {
            UploadAsync(diskUri, sourceStream, remoteName, new CancellationToken()).Wait();
        }

        public void AddFile(string filePath, string remote = null) {
            AddFileAsync(filePath, remote).Wait();
        }

        public void AddBytes(string filename, byte[] data) {
            AddBytesAsync(filename, data).Wait();
        }

        public void Delete() {
            DeleteAsync().Wait();
        }
        #endregion
    }

    internal class DiskApi
    {
        public string Description { get; set; }

        public Guid Uuid { get; set; }

        public int FileCount { get; set; }

        public long UsedSpaceBytes { get; set; }

        public DateTime CreationDate { get; set; }

        public bool Locked { get; set; }

        public DiskApi ()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="csharpapi.Disk"/> class.
        /// </summary>
        /// <param name="description">Description.</param>
        /// <param name="locked">If set to <c>true</c> disk is locked.</param>
        public DiskApi (string description, bool locked)
        {
            Description = description;
            Locked = locked;
        }


        public override string ToString()
        {
            return string.Format ("[Disk: Description={0}, Id={1}, FileCount={2}, UsedSpaceBytes={3}, CreationDate={4}, Locked={5}]", Description, Uuid, FileCount, UsedSpaceBytes, CreationDate, Locked);
        }
    }
}


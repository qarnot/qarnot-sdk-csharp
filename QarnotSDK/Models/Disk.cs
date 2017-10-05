using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;

namespace QarnotSDK
{
    public class QDisk {
        private readonly Connection _api;
        private DiskApi _diskApi;
        private string _uri;

        private CancellationTokenSource _downloadSource = null;

        public string Description { get { return _diskApi.Description; } }

        public string Shortname { get { return _api.HasDiskShortnameFeature ? _diskApi.Shortname : _diskApi.Description; } }

        public Guid Uuid { get { return _diskApi.Uuid; } }

        public int FileCount { get { return _diskApi.FileCount; } }

        public long UsedSpaceBytes { get { return _diskApi.UsedSpaceBytes; } }

        public DateTime CreationDate { get { return _diskApi.CreationDate; } }

        public bool Locked { get { return _diskApi.Locked; } }

        public QDisk(Connection api, string name) {
            _api = api;
            _diskApi = new DiskApi();
            _diskApi.Description = name;

            if (_api.HasDiskShortnameFeature) {
                _diskApi.Shortname = name;
                _uri = "disks/" + name;
            }
        }

        public QDisk(Connection api, Guid guid) : this(api, guid.ToString()) {
            _uri = "disks/" + guid.ToString();
        }

        internal QDisk(Connection api, DiskApi diskApi) {
            _api = api;
            _diskApi = diskApi;
            _uri = "disks/" + _diskApi.Uuid.ToString();
        }

        #region workaround
        // Will be removed once the 'shortname' is implemented on the api side
        internal async Task ApiWorkaround_EnsureUriAsync(bool mustExist) {
            if (_api.HasDiskShortnameFeature) {
                // No workaround needed
                return;
            }

            if (mustExist) {
                // The pool uri must exist, so if uri is null, fetch the pool by name
                if (_uri != null) {
                    return;
                }

                var result = await _api.RetrieveDiskByNameAsync(_diskApi.Description);
                if (result == null) {
                    throw new QarnotApiResourceNotFoundException("disk " + _diskApi.Description + " doesn't exist", null);
                }
                _diskApi.Uuid = result.Uuid;
                _uri = "disks/" + _diskApi.Uuid.ToString();
            } else {
                if (_uri != null) {
                    // We have an uri, check if it's still valid
                    try {
                        var response = await _api._client.GetAsync(_uri); // get disk status
                        await Utils.LookForErrorAndThrow(_api._client, response);
                        // no error, the disk still exists
                        throw new QarnotApiResourceAlreadyExistsException("disk " + _diskApi.Description + " already exists", null);
                    } catch (QarnotApiResourceNotFoundException) {
                        // OK, not running
                    }
                } else {
                    // We don't have any uri, check if the disk name exists
                    var result = await _api.RetrieveDiskByNameAsync(_diskApi.Description);
                    if (result != null) {
                        throw new QarnotApiResourceAlreadyExistsException("disk " + _diskApi.Description + " already exists", null);
                    }
                }
                _diskApi.Uuid = new Guid();
                _uri = null;
            }
        }
        #endregion

        public void Cancel() {
            if (_downloadSource != null)
                _downloadSource.Cancel();
        }

        public async Task UpdateAsync() {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.GetAsync(_uri);
            await Utils.LookForErrorAndThrow(_api._client, response);

            // Retrieve the guid from the response and assign it to the DiskApi
            var result = await response.Content.ReadAsAsync<DiskApi>();
            _diskApi = result;
        }

        public async Task DownloadAsync(string outdir) {
            _downloadSource = new CancellationTokenSource();
            await DownloadAsync(outdir, _downloadSource.Token);
        }

        public async Task DownloadAsync(string outDir, CancellationToken cancellationToken) {
            await ApiWorkaround_EnsureUriAsync(true);

            string resDiskUri = "disks/tree/" + _diskApi.Uuid.ToString();
            var response = await _api._client.GetAsync(resDiskUri);
            await Utils.LookForErrorAndThrow(_api._client, response);
            List<QFile> resFiles = await response.Content.ReadAsAsync<List<QFile>>(cancellationToken);
            var tasks = new Task[resFiles.Count];
            uint index = 0;
            try {
                foreach (var item in resFiles) {
                    tasks[index++] = Utils.Download(_api._client, _uri, item.Name, outDir, cancellationToken);
                }
                await Task.WhenAll(tasks);
            } catch (Exception ex) {
                //Console.WriteLine (ex.Message);
                throw ex;
            }
        }

        private async Task UploadAsync(Stream sourceStream, string remoteName, CancellationToken cancellationToken) {
            await ApiWorkaround_EnsureUriAsync(true);

            var requestContent = new MultipartFormDataContent();
            var fileContent = new StreamContent(sourceStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            fileContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse("attachment; filename=" + Path.GetFileName(remoteName));
            requestContent.Add(fileContent, Path.GetFileNameWithoutExtension(remoteName), Path.GetFileName(remoteName));

            var response = await _api._client.PostAsync(_uri + "/" + Path.GetDirectoryName(remoteName), requestContent, cancellationToken);
            await Utils.LookForErrorAndThrow(_api._client, response);
        }

        public async Task<Stream> GetStreamAsync(string remotePath) {
            await ApiWorkaround_EnsureUriAsync(true);

            string fileUri = _uri + "/" + remotePath;

            var response = await _api._client.GetAsync(
                fileUri,
                HttpCompletionOption.ResponseHeadersRead);
            await Utils.LookForErrorAndThrow(_api._client, response);

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task GetFileAsync(string remotePath, string filePath) {
            try {
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                    using (var httpStream = await GetStreamAsync(remotePath)) {
                        await httpStream.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }
                }
            } catch (Exception ex) {
                // Cleanup in case of error
                File.Delete(filePath);
                throw ex;
            }
        }

        public async Task AddFileAsync(string filePath, string remote = null) {
            if (!File.Exists(filePath))
                throw new IOException("No such file " + filePath);

            var remoteName = remote ?? Path.GetFileName(filePath);
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
                await UploadAsync(fs, remoteName, new CancellationToken());
            }
        }

        public async Task AddBytesAsync(string filename, byte[] data) {
            using (var fs = new MemoryStream(data)) {
                await UploadAsync(fs, filename, new CancellationToken());
            }
        }

        public async Task CreateAsync(bool dontFailIfExists = false) {
            if (dontFailIfExists) {
                // Check if the disk exists and return immediately
                try {
                    await UpdateAsync();
                    return;
                } catch (QarnotApiResourceNotFoundException) {

                }
            }

            await ApiWorkaround_EnsureUriAsync(false);

            var response = await _api._client.PostAsJsonAsync<DiskApi>("disks", _diskApi);
            await Utils.LookForErrorAndThrow(_api._client, response);

            // Update the task Uuid
            var result = await response.Content.ReadAsAsync<DiskApi>();
            _diskApi.Uuid = result.Uuid;
            _uri = "disks/" + _diskApi.Uuid.ToString();
        }

        public async Task LockAsync(bool lockState) {
            await ApiWorkaround_EnsureUriAsync(true);

            var lockApi = new LockApi(lockState);
            var response = await _api._client.PutAsJsonAsync<LockApi>(_uri, lockApi);
            await Utils.LookForErrorAndThrow(_api._client, response);

            _diskApi.Locked = lockState;
        }

        public async Task DeleteAsync() {
            await ApiWorkaround_EnsureUriAsync(true);

            var response = await _api._client.DeleteAsync(_uri);
            response.EnsureSuccessStatusCode();
        }

        #region Async wrappers
        public void Download(string outdir) {
            _downloadSource = new CancellationTokenSource();
            DownloadAsync(outdir, _downloadSource.Token).Wait();
        }

        private void Upload(Stream sourceStream, string remoteName, CancellationToken cancellationToken) {
            UploadAsync(sourceStream, remoteName, new CancellationToken()).Wait();
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

    internal class LockApi {
        public bool Locked { get; set; }

        public LockApi() { }
        public LockApi(bool locked) {
            Locked = locked;
        }
    }

    internal class DiskApi {
        public override string ToString() {
            return string.Format("[Disk: Description={0}, Id={1}, FileCount={2}, UsedSpaceBytes={3}, CreationDate={4}, Locked={5}]", Description, Uuid, FileCount, UsedSpaceBytes, CreationDate, Locked);
        }

        public string Description { get; set; }
        public string Shortname { get; set; }
        public Guid Uuid { get; set; }
        public int FileCount { get; set; }
        public long UsedSpaceBytes { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Locked { get; set; }

        public DiskApi() {
        }
    }
}


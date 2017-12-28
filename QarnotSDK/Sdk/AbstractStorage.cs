using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK {
    /// <summary>
    /// Represents an entry (file or folder) in a storage.
    /// This class is abstract, use one of the final implementations: QDiskEntry or QBucketEntry.
    /// </summary>
    public abstract class QAbstractStorageEntry {
        static internal int ReadBufferSize { get; set;} = 1 * 1024 * 1024;

        /// <summary>
        /// Size of this entry in bytes.
        /// </summary>
        public long Size { get; protected set; }

        /// <summary>
        /// Full name of this entry.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The flags for this entry, use it to identify folders, files and executable files.
        /// </summary>
        public FileFlags FileFlags { get; protected set; }

        /// <summary>
        /// The digest for this entry content.
        /// Use EqualsLocalFileDigest or EqualsLocalFileDigestAsync to compare the digest to a local file.
        /// </summary>
        public string Digest { get; protected set; }

        /// <summary>
        /// Computes the digest of a local file and compares it to the digest of this storage entry.
        /// </summary>
        /// <param name="localFilePath">The local file to compare to.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public abstract Task<bool> EqualsLocalFileDigestAsync(string localFilePath, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Computes the digest of a local file and compares it to the digest of this storage entry.
        /// </summary>
        /// <param name="localFilePath">The local file to compare to.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public bool EqualsLocalFileDigest(string localFilePath, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return EqualsLocalFileDigestAsync(localFilePath, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        internal string GetNormalizedName() {
            var n = Name;
            if (n.EndsWith("/")) {
                n = n.Substring(0, n.Length - 1);
            }
            if (n.StartsWith("/")) {
                n = n.Substring(1);
            }
            return n;
        }
    }

    /// <summary>
    /// Represents an abstract class to a resource or result storage class.
    /// Two classes implement this interface: QDisk and QBucket
    /// </summary>
    public abstract partial class QAbstractStorage {
        /// <summary>
        /// A storage unique identifier.
        /// Note: For a bucket, the unique id is the bucket name.
        ///  For a disk, the unique id is the disk Uuid.
        /// </summary>
        public abstract string UniqueId { get; }
        /// <summary>
        /// The storage shortname identifier. The shortname is provided by the user. It has to be unique.
        /// Note: For a bucket, shortname is the bucket name.
        /// </summary>
        public abstract string Shortname { get; }
        /// <summary>
        /// Number of files in this storage.
        /// Use Update or UpdateAsync to refresh.
        /// </summary>
        public abstract int FileCount { get; }
        /// <summary>
        /// Size of this storage in bytes.
        /// Use Update or UpdateAsync to refresh.
        /// </summary>
        public abstract long UsedSpaceBytes { get; }
        /// <summary>
        /// The storage creation date
        /// </summary>
        public abstract DateTime CreationDate { get; }

        /// <summary>
        /// Create the storage.
        /// Note: if the storage already exists, no exception is thrown.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete this storage.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Update this storage file count and storage usage.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public abstract Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// List the files and folders.
        /// </summary>
        /// <param name="remoteFolder">The folder to list.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of QFile</returns>
        public abstract Task<List<QAbstractStorageEntry>> ListEntriesAsync(string remoteFolder, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Delete a file or folder in this storage.
        /// </summary>
        /// <param name="remotePath">The entry to remove.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public abstract Task DeleteEntryAsync(string remotePath, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Write a stream to a file in this storage.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public abstract Task UploadStreamAsync(Stream sourceStream, string remoteFile, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Get a stream on a file in this storage.
        /// </summary>
        /// <param name="remoteFile">The source file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A stream with the file's data.</returns>
        public abstract Task<Stream> DownloadStreamAsync(string remoteFile, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Upload a local file to a file in this storage.
        /// </summary>
        /// <param name="localFile">The source local file name.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task UploadFileAsync(string localFile, string remoteFile = null, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!File.Exists(localFile))
                throw new IOException("No such file " + localFile);

            var remoteName = String.IsNullOrEmpty(remoteFile) ? Path.GetFileName(localFile) : remoteFile;
            using (var fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read)) {
                await UploadStreamAsync(fileStream, remoteName, cancellationToken);
            }
        }

        /// <summary>
        /// Download a file from this storage to a local file.
        /// </summary>
        /// <param name="remoteFile">The source file name in this storage.</param>
        /// <param name="localFile">The destination local file name.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task DownloadFileAsync(string remoteFile, string localFile, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                using (var fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                    using (var httpStream = await DownloadStreamAsync(remoteFile, cancellationToken)) {
                        await httpStream.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }
                }
            } catch (Exception ex) {
                // Cleanup in case of error
                File.Delete(localFile);
                throw ex;
            }
        }

        /// <summary>
        /// Write a string to a file in this storage.
        /// </summary>
        /// <param name="content">The content to write.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="encoding">The encoding to use to write the string. UTF8 by default.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task UploadStringAsync(string content, string remoteFile, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken)) {
            if (encoding == null) encoding = Encoding.UTF8;
            using (var stream = new MemoryStream(encoding.GetBytes(content))) {
                await UploadStreamAsync(stream, remoteFile, cancellationToken);
            }
        }

        /// <summary>
        /// Get the text content of a file in this storage.
        /// </summary>
        /// <param name="remoteFile">The source file name in this storage.</param>
        /// <param name="encoding">The encoding to use to read the string. UTF8 by default.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task<string> DownloadStringAsync(string remoteFile, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken)) {
            if (encoding == null) encoding = Encoding.UTF8;
            using (var stream = new StreamReader(await DownloadStreamAsync(remoteFile, cancellationToken), encoding)) {
                return await stream.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Write binary data to a file in this storage.
        /// </summary>
        /// <param name="data">The binary buffer to write.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task UploadBytesAsync(byte[] data, string remoteFile, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var stream = new MemoryStream(data)) {
                await UploadStreamAsync(stream, remoteFile, cancellationToken);
            }
        }

        /// <summary>
        /// Synchronize a remote folder to a local folder.
        /// </summary>
        /// <param name="localFolderPath">The local folder to overwrite.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <param name="dontDelete">If false, the files that have been deleted in this storage will also be removed in the local folder. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync only a sub-folder of this storage.</param>
        /// <returns></returns>
        public async Task SyncRemoteToLocalAsync(string localFolderPath, CancellationToken cancellationToken, bool dontDelete = true, string remoteFolderRelativePath = "") {
            Directory.CreateDirectory(localFolderPath);

            List<QAbstractStorageEntry> existingFiles;
            try {
                existingFiles = await ListEntriesAsync(remoteFolderRelativePath, cancellationToken);
            } catch (QarnotSDK.QarnotApiResourceNotFoundException) {
                // The remote folder doesn't exist, so we just have to create an empty file list.
                existingFiles = new List<QAbstractStorageEntry>();
            }

            // Remove Directories that are not presents remotely or that are now a file
            foreach (var file in Directory.EnumerateDirectories(localFolderPath)) {
                var remotePath = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(file));
                bool bFound = false;

                foreach (var qfile in existingFiles) {
                    if (qfile.GetNormalizedName().Equals(remotePath)) {
                        if (qfile.FileFlags == FileFlags.Directory) {
                            bFound = true;
                        }
                        break;
                    }
                }

                if (!bFound) {
                    if (!dontDelete) Directory.Delete(file, true);
                }
            }

            // Sync files
            foreach (var file in Directory.EnumerateFiles(localFolderPath)) {
                var fileName = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(file));
                var remotePath = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(file));
                bool bFound = false;

                var info = new FileInfo(file);

                foreach (var qfile in existingFiles) {
                    if (qfile.GetNormalizedName().Equals(fileName)) {
                        if (qfile.FileFlags != FileFlags.Directory) {
                            // File already exists, now check size
                            if (qfile.Size == info.Length) {
                                // Size is same, now check the Sha1
                                if (await qfile.EqualsLocalFileDigestAsync(file, cancellationToken)) {
                                    // File are identical, no need to re-download
                                    existingFiles.Remove(qfile);
                                }
                            }
                            bFound = true;
                        }

                        // The file already exists, we can break here
                        break;
                    }
                }

                if (!bFound) {
                    if (!dontDelete) File.Delete(file);
                }
            }

            // Download remaining files
            foreach (var toDownload in existingFiles) {
                var remotePath = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(toDownload.GetNormalizedName()));
                var localPath = Path.Combine(localFolderPath, Path.GetFileName(toDownload.GetNormalizedName()));

                if (toDownload.FileFlags == FileFlags.Directory) {
                    await SyncRemoteToLocalAsync(localPath, cancellationToken, dontDelete, remotePath);
                } else {
                    await DownloadFileAsync(remotePath, localPath, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Synchronize a local folder to a remote folder.
        /// </summary>
        /// <param name="localFolderPath">The source local folder to upload.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <param name="dontDelete">If false, the files that have been deleted in the local folder will also be removed from this storage. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync to a sub-folder of this storage.</param>
        /// <returns></returns>
        public async Task SyncLocalToRemoteAsync(string localFolderPath, CancellationToken cancellationToken, bool dontDelete = true, string remoteFolderRelativePath = "") {
            List<QAbstractStorageEntry> existingFiles;
            try {
                existingFiles = await ListEntriesAsync(remoteFolderRelativePath, cancellationToken);
            } catch (QarnotSDK.QarnotApiResourceNotFoundException) {
                // The remote folder doesn't exist, so we just have to create an empty file list.
                existingFiles = new List<QAbstractStorageEntry>();
            }

            // Sync files
            foreach (var file in Directory.EnumerateFiles(localFolderPath)) {
                var fileName = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(file));
                var remotePath = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(file));

                var info = new FileInfo(file);

                bool bUpload = true;
                foreach (var qfile in existingFiles) {
                    if (qfile.GetNormalizedName().Equals(fileName)) {
                        if (qfile.FileFlags == FileFlags.Directory) {
                            // Exists, but it's a directory. Remove it
                            if (!dontDelete) await DeleteEntryAsync(qfile.Name, cancellationToken);
                        } else {
                            // File already exists, now check size
                            if (qfile.Size == info.Length) {
                                // Size is same, now check the Digest
                                if (await qfile.EqualsLocalFileDigestAsync(file, cancellationToken)) {
                                    // File are identical, no need to re-upload
                                    bUpload = false;
                                }
                            }
                        }

                        // The file already exists, we can break here
                        existingFiles.Remove(qfile);
                        break;
                    }
                }

                if (bUpload) {
                    await UploadFileAsync(file, remotePath, cancellationToken);
                }
            }

            // Sync folders
            foreach (var file in Directory.EnumerateDirectories(localFolderPath)) {
                var remotePath = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(file));

                // Remove the folder from the existing files
                foreach (var qfile in existingFiles) {
                    if (qfile.GetNormalizedName().Equals(remotePath)) {
                        existingFiles.Remove(qfile);

                        if (qfile.FileFlags != FileFlags.Directory) {
                            // Exists, but it's not a directory, we have to remove it first
                            if (!dontDelete) await DeleteEntryAsync(remotePath, cancellationToken);
                        }

                        break;
                    }
                }

                await SyncLocalToRemoteAsync(file, cancellationToken, dontDelete, remotePath);
            }

            // Remove remaining files
            foreach (var toRemove in existingFiles) {
                //var remotePath = UnixPathCombine(remoteFolderRelativePath, Path.GetFileName(toRemove.GetNormalizedName()));
                if (!dontDelete) await DeleteEntryAsync(toRemove.Name, cancellationToken);
            }
        }

        /// <summary>
        /// Upload a folder to a folder in this storage.
        /// </summary>
        /// <param name="localFolderPath">The source folder path.</param>
        /// <param name="remoteFolderPath">The destination folder path in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task UploadFolderAsync(string localFolderPath, string remoteFolderPath = null, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!Directory.Exists(localFolderPath))
                throw new IOException("No such folder " + localFolderPath);

            var remoteName = String.IsNullOrEmpty(remoteFolderPath) ? Path.GetFileName(localFolderPath) : remoteFolderPath;
            await SyncLocalToRemoteAsync(localFolderPath, true, remoteName);
        }

        /// <summary>
        /// Download a folder in this storage to a local folder.
        /// </summary>
        /// <param name="remoteFolderPath">The source folder path in this storage.</param>
        /// <param name="localFolderPath">The destination folder path.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public async Task DownloadFolderAsync(string remoteFolderPath, string localFolderPath, CancellationToken cancellationToken = default(CancellationToken)) {
            await SyncRemoteToLocalAsync(localFolderPath, cancellationToken, true, remoteFolderPath);
        }

        /// <summary>
        /// Synchronize a remote folder to a local folder.
        /// </summary>
        /// <param name="localFolderPath">The local folder to overwrite.</param>
        /// <param name="dontDelete">If false, the files that have been deleted in this storage will also be removed in the local folder. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync only a sub-folder of this storage.</param>
        /// <returns></returns>
        public async Task SyncRemoteToLocalAsync(string localFolderPath, bool dontDelete = true, string remoteFolderRelativePath = "") {
            await SyncRemoteToLocalAsync(localFolderPath, default(CancellationToken), dontDelete, remoteFolderRelativePath);
        }

        /// <summary>
        /// Synchronize a local folder to a remote folder.
        /// </summary>
        /// <param name="localFolderPath">The source local folder to upload.</param>
        /// <param name="dontDelete">If false, the files that have been deleted in the local folder will also be removed from this storage. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync to a sub-folder of this storage.</param>
        /// <returns></returns>
        public async Task SyncLocalToRemoteAsync(string localFolderPath, bool dontDelete = true, string remoteFolderRelativePath = "") {
            await SyncLocalToRemoteAsync(localFolderPath, default(CancellationToken), dontDelete, remoteFolderRelativePath);
        }

        internal static string UnixPathCombine(string p1, string p2) {
            return Path.Combine(p1, p2).Replace('\\', '/');
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK {
    public abstract partial class QAbstractStorage {
        /// <summary>
        /// Create the storage.
        /// Note: if the storage already exists, no exception is thrown.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public void Create(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CreateAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this storage file count and storage usage.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Update(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UpdateAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the storage.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Delete(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                DeleteAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// List the files and folders.
        /// </summary>
        /// <param name="remoteFolder">The folder to list.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of QFile</returns>
        public List<QAbstractStorageEntry> ListEntries(string remoteFolder, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return ListEntriesAsync(remoteFolder, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete a file or folder in this storage.
        /// </summary>
        /// <param name="remotePath">The entry to remove.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void DeleteEntry(string remotePath, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                DeleteEntryAsync(remotePath, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        #region upload/download
        /// <summary>
        /// Write a stream to a file in this storage.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadStream(Stream sourceStream, string remoteFile, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UploadStreamAsync(sourceStream, remoteFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Get a stream on a file in this storage.
        /// </summary>
        /// <param name="remoteFile">The source file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A stream with the file's data.</returns>
        public Stream DownloadStream(string remoteFile, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return DownloadStreamAsync(remoteFile, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Upload a local file to a file in this storage.
        /// </summary>
        /// <param name="localFile">The source local file name.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadFile(string localFile, string remoteFile = null, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UploadFileAsync(localFile, remoteFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Download a file from this storage to a local file.
        /// </summary>
        /// <param name="remoteFile">The source file name in this storage.</param>
        /// <param name="localFile">The destination local file name.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void DownloadFile(string remoteFile, string localFile, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                DownloadFileAsync(remoteFile, localFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
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
        public void UploadString(string content, string remoteFile, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UploadStringAsync(content, remoteFile, encoding, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Get the text content of a file in this storage.
        /// </summary>
        /// <param name="remoteFile">The source file name in this storage.</param>
        /// <param name="encoding">The encoding to use to read the string. UTF8 by default.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public string DownloadString(string remoteFile, Encoding encoding = null, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return DownloadStringAsync(remoteFile, encoding, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Write binary data to a file in this storage.
        /// </summary>
        /// <param name="data">The binary buffer to write.</param>
        /// <param name="remoteFile">The destination file name in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadBytes(byte[] data, string remoteFile, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UploadBytesAsync(data, remoteFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Upload a folder to a folder in this storage.
        /// </summary>
        /// <param name="localFolderPath">The source folder path.</param>
        /// <param name="remoteFolderPath">The destination folder path in this storage.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void UploadFolder(string localFolderPath, string remoteFolderPath = null, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                UploadFolderAsync(localFolderPath, remoteFolderPath, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Download a folder in this storage to a local folder.
        /// </summary>
        /// <param name="remoteFolderPath">The source folder path in this storage.</param>
        /// <param name="localFolderPath">The destination folder path.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void DownloadFolder(string remoteFolderPath, string localFolderPath, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                DownloadFolderAsync(remoteFolderPath, localFolderPath, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Synchronize a remote folder to a local folder.
        /// </summary>
        /// <param name="localFolderPath">The local folder to overwrite.</param>
        /// <param name="dontDelete">If false, the files that have been deleted in this storage will also be removed in the local folder. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync only a sub-folder of this storage.</param>
        public void SyncRemoteToLocal(string localFolderPath, bool dontDelete = true, string remoteFolderRelativePath = "") {
            try {
                SyncRemoteToLocalAsync(localFolderPath, dontDelete, remoteFolderRelativePath).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
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
        public void SyncRemoteToLocal(string localFolderPath, CancellationToken cancellationToken, bool dontDelete = true, string remoteFolderRelativePath = "") {
            try {
                SyncRemoteToLocalAsync(localFolderPath, cancellationToken, dontDelete, remoteFolderRelativePath).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Synchronize a local folder to a remote folder.
        /// </summary>
        /// <param name="localFolderPath">The source local folder to upload.</param>
        /// <param name="dontDelete">If false, the files that have been deleted in the local folder will also be removed from this storage. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync to a sub-folder of this storage.</param>
        /// <returns></returns>
        public void SyncLocalToRemote(string localFolderPath, bool dontDelete = true, string remoteFolderRelativePath = "") {
            try {
                SyncLocalToRemoteAsync(localFolderPath, dontDelete, remoteFolderRelativePath).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
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
        public void SyncLocalToRemote(string localFolderPath, CancellationToken cancellationToken, bool dontDelete = true, string remoteFolderRelativePath = "") {
            try {
                SyncLocalToRemoteAsync(localFolderPath, cancellationToken, dontDelete, remoteFolderRelativePath).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;

namespace QarnotSDK
{
    /// <summary>
    /// This class handles the Qarnot Disks
    /// </summary>
    public partial class QDisk {
        /// <summary>
        /// Create the disk.
        /// </summary>
        /// <param name="dontFailIfExists">Don't throw an exception if the disk already exists.</param>
        /// <returns></returns>
        public void Create(bool dontFailIfExists = false) {
            try {
                CreateAsync(dontFailIfExists).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        /// <summary>
        /// Create the disk.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <param name="dontFailIfExists">Don't throw an exception if the disk already exist.</param>
        /// <returns></returns>
        public void Create(CancellationToken cancellationToken, bool dontFailIfExists = false) {
            try {
                CreateAsync(cancellationToken, dontFailIfExists).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Lock or unlock the disk.
        /// </summary>
        /// <param name="lockState">True to lock the disk, false to unlock it.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Lock(bool lockState, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                LockAsync(lockState, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete the disk.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Delete(CancellationToken cancellationToken = new CancellationToken()) {
            try {
                DeleteAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Update this disk file count and disk usage.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void Update(CancellationToken cancellationToken = new CancellationToken()) {
            try {
                UpdateAsync(cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        #region upload/download
        /// <summary>
        /// Write a stream to a file in this disk.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="remoteFile">The destination file name in this disk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadStream(Stream sourceStream, string remoteFile, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                UploadStreamAsync(sourceStream, remoteFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Get a stream on a file in this disk.
        /// </summary>
        /// <param name="remoteFile">The source file name in this disk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A stream with the file's data.</returns>
        public Stream DownloadStream(string remoteFile, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                return DownloadStreamAsync(remoteFile, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Upload a local file to a file in this disk.
        /// </summary>
        /// <param name="localFile">The source local file name.</param>
        /// <param name="remoteFile">The destination file name in this disk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadFile(string localFile, string remoteFile = null, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                UploadFileAsync(localFile, remoteFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Download a file from this disk to a local file.
        /// </summary>
        /// <param name="remoteFile">The source file name in this disk.</param>
        /// <param name="localFile">The destination local file name.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void DownloadFile(string remoteFile, string localFile, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                DownloadFileAsync(remoteFile, localFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Write a string to a file in this disk.
        /// </summary>
        /// <param name="content">The content to write.</param>
        /// <param name="remoteFile">The destination file name in this disk.</param>
        /// <param name="encoding">The encoding to use to write the string. UTF8 by default.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadString(string content, string remoteFile, Encoding encoding = null, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                UploadStringAsync(content, remoteFile, encoding, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Get the text content of a file in this disk.
        /// </summary>
        /// <param name="remoteFile">The source file name in this disk.</param>
        /// <param name="encoding">The encoding to use to read the string. UTF8 by default.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public string DownloadString(string remoteFile, Encoding encoding = null, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                return DownloadStringAsync(remoteFile, encoding, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Write binary data to a file in this disk.
        /// </summary>
        /// <param name="data">The binary buffer to write.</param>
        /// <param name="remoteFile">The destination file name in this disk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void UploadBytes(byte[] data, string remoteFile, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                UploadBytesAsync(data, remoteFile, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Upload a folder to a folder in this disk.
        /// </summary>
        /// <param name="localFolderPath">The source folder path.</param>
        /// <param name="remoteFolderPath">The destination folder path in this disk.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void UploadFolder(string localFolderPath, string remoteFolderPath = null, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                UploadFolderAsync(localFolderPath, remoteFolderPath, cancellationToken).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Download a folder in this disk to a local folder.
        /// </summary>
        /// <param name="remoteFolderPath">The source folder path in this disk.</param>
        /// <param name="localFolderPath">The destination folder path.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        public void DownloadFolder(string remoteFolderPath, string localFolderPath, CancellationToken cancellationToken = new CancellationToken()) {
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
        /// <param name="dontDelete">If false, the files that have been deleted in this disk will also be removed in the local folder. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync only a sub-folder of this disks.</param>
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
        /// <param name="dontDelete">If false, the files that have been deleted in this disk will also be removed in the local folder. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync only a sub-folder of this disks.</param>
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
        /// <param name="dontDelete">If false, the files that have been deleted in the local folder will also be removed from this disk. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync to a sub-folder of this disks.</param>
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
        /// <param name="dontDelete">If false, the files that have been deleted in the local folder will also be removed from this disk. To avoid mistakes, this parameter is set to true by default.</param>
        /// <param name="remoteFolderRelativePath">Optional, allows to sync to a sub-folder of this disks.</param>
        /// <returns></returns>
        public void SyncLocalToRemote(string localFolderPath, CancellationToken cancellationToken, bool dontDelete = true, string remoteFolderRelativePath = "") {
            try {
                SyncLocalToRemoteAsync(localFolderPath, cancellationToken, dontDelete, remoteFolderRelativePath).Wait();
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Delete a file or folder in this disk.
        /// </summary>
        /// <param name="remotePath">The entry to remove.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public void DeleteEntry(string remotePath, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                DeleteEntryAsync(remotePath, cancellationToken).Wait();
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
        public List<QFile> ListEntries(string remoteFolder, CancellationToken cancellationToken = new CancellationToken()) {
            try {
                return ListEntriesAsync(remoteFolder, cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Return all the entries this disk contains.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of QFile</returns>
        [System.Obsolete("use ListEntries() instead")]
        public List<QFile> Tree(CancellationToken cancellationToken = new CancellationToken()) {
            try {
                return TreeAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}


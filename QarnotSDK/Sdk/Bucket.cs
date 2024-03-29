﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK {

    /// <summary>
    /// Abstract base class for bucket filtering
    /// </summary>
    public abstract class ABucketFiltering {
        internal abstract ABucketFiltering SanitizePaths(bool showWarnings);
    }


    /// <summary>
    /// Filter a bucket by prefix
    /// </summary>
    public sealed class BucketFilteringPrefix : ABucketFiltering {
        /// <summary> Prefix by which to filter </summary>
        public string Prefix { get; private set; }

        /// <summary> Create a prefix filtering </summary>
        /// <param name="prefix">The prefix to filter by</param>
        public BucketFilteringPrefix(string prefix) {
            Prefix = prefix;
        }

        internal override ABucketFiltering SanitizePaths(bool showWarnings = true)
        {
            Prefix = Utils.GetSanitizedBucketPath(Prefix, showWarnings);
            return this;
        }
    }


    /// <summary>
    /// Abstract base class for resources transformation
    /// </summary>
    public abstract class AResourcesTransformation {
        internal abstract AResourcesTransformation SanitizePaths(bool showWarnings = true);
    }


    /// <summary>
    /// Transform resources by stripping a prefix from the object name in the bucket
    /// </summary>
    public sealed class ResourcesTransformationStripPrefix : AResourcesTransformation {
        /// <summary> Prefix to strip </summary>
        public string Prefix { get; private set; }

        /// <summary> Build a resources transformation stripping prefixes</summary>
        /// <param name="prefix">The prefix to strip</param>
        public ResourcesTransformationStripPrefix(string prefix) {
            Prefix = prefix;
        }

        internal override AResourcesTransformation SanitizePaths(bool showWarnings = true)
        {
            Prefix = Utils.GetSanitizedBucketPath(Prefix, showWarnings);
            return this;
        }
    }


    /// <summary>
    /// Represents an entry (file or folder) in a bucket.
    /// </summary>
    public class QBucketEntry : QAbstractStorageEntry {
        private long DefaultUploadPartSize;
        private List<long> AvailableMultipartSizes;
        private const int Md5HashSize = 16;

        /// <summary>
        /// Helper to compute multiple MD5 on one stream.
        /// </summary>
        private class eTagHelper {
            public long NextPartOffset;
            public int TotalParts;
            public long PartSize;
            public MemoryStream Hashes;
            public MD5 Md5;

            public eTagHelper(long partSize, int parts) {
                TotalParts = parts;
                Hashes = new MemoryStream(parts * Md5HashSize);
                NextPartOffset = partSize;
                PartSize = partSize;
                Md5 = MD5.Create();
            }
        }

        /// <summary>
        /// Last modification date of this entry.
        /// </summary>
        public  virtual DateTime LastModified { get; protected set; }

        /// <summary>
        /// Size of one part if the file was uploaded in multi-part.
        /// Note: this value is filled only after a successful call to EqualsLocalFileDigest or EqualsLocalFileDigestAsync.
        /// </summary>
        public  virtual long PartSize { get; private set; }

        /// <summary>
        /// Computes the digest of a local file and compares it to the digest of this bucket object.
        /// Note: if this entry was uploaded by parts with a part size different from the default (8MB),
        ///  you may need to adjust the StorageAvailablePartSizes list in Connection.
        ///  By default, this method will try to "guess" the correct part size.
        /// </summary>
        /// <param name="localFilePath">The local file to compare to.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public override async Task<bool> EqualsLocalFileDigestAsync(string localFilePath, CancellationToken cancellationToken = default(CancellationToken)) {
            // No need to check, we don't have the eTag :(
            if (String.IsNullOrEmpty(Digest) || Digest.Length < Md5HashSize * 2) return false;

            int digestPartCount = 0;
            if (Digest.Length > Md5HashSize * 2) {
                // Multipart eTag: should be "md5hash-partnumber"
                if (Digest.IndexOf('-') != Md5HashSize * 2) return false;
                if (!int.TryParse(Digest.Substring(Md5HashSize * 2 + 1), out digestPartCount)) return false;
            }

            using (var fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read)) {
                if (digestPartCount == 0 || digestPartCount == 1) {
                    var md5 = MD5.Create();
                    var digest = md5.ComputeHash(fs);
                    var hexDigest = BitConverter.ToString(digest).Replace("-", "").ToLower() + (digestPartCount == 1 ? "-1":"");
                    return hexDigest.Equals(Digest);
                } else {
                    long fileSize = fs.Length;

                    // Look in the available multipart sizes the ones that match the part count
                    var possiblePartSizes = new List<long>();
                    bool bGuess = false;
                    if (DefaultUploadPartSize > 0) {
                        if ((fileSize / DefaultUploadPartSize + 1) == digestPartCount && !possiblePartSizes.Contains(DefaultUploadPartSize))
                            possiblePartSizes.Add(DefaultUploadPartSize);
                    }
                    foreach (var partSize in AvailableMultipartSizes) {
                        if (partSize <= 0) {
                            bGuess = true;
                            continue;
                        }
                        if ((fileSize / partSize + 1) == digestPartCount && !possiblePartSizes.Contains(partSize))
                            possiblePartSizes.Add(partSize);
                    }
                    if (bGuess) {
                        long min = fileSize / digestPartCount;
                        long max = fileSize / (digestPartCount - 1);

                        long range = ((max - min) / (1024 * 1024));

                        // Adjust the step so we have a maximum of 9 MD5 to compute in parallel
                        long step = 1 * 1024 * 1024;
                        while(range >= 10) {
                            range /= 5;
                            step *= 5;
                        }

                        long alignedMin = (min / step) * step;
                        if (alignedMin < min) alignedMin += step;

                        for (long i = alignedMin; i < max; i += step) {
                            if (!possiblePartSizes.Contains(i)) possiblePartSizes.Add(i);
                        }
                    }
                    if (possiblePartSizes.Count == 0) {
                        return false;
                    }

                    // Create the list that will maintain the different hashes
                    var digests = new List<eTagHelper>();
                    foreach (var partSize in possiblePartSizes) {
                        digests.Add(new eTagHelper(partSize, digestPartCount));
                    }

                    long offset = 0;
                    byte[] readBuffer = new byte[ReadBufferSize];
                    while (offset < fileSize) {
                        // Which size we can read
                        int toRead = ReadBufferSize;
                        // Don't read after the EOF
                        if (toRead > fileSize - offset)
                            toRead = (int)(fileSize - offset);
                        // Align read to the end of possible multipart sizes
                        foreach (var d in digests) {
                            if (toRead > d.NextPartOffset - offset)
                                toRead = (int)(d.NextPartOffset - offset);
                        }

                        var read = await fs.ReadAsync(readBuffer, 0, toRead, cancellationToken);
                        if (read <= 0) throw new IOException(); // Should not happen

                        offset += read;

                        // Update hashes
                        foreach (var d in digests) {
                            if (offset == d.NextPartOffset) {
                                d.Md5.TransformFinalBlock(readBuffer, 0, read);
                                d.NextPartOffset += d.PartSize;
                                if (d.NextPartOffset > fileSize) d.NextPartOffset = fileSize;
                                d.Hashes.Write(d.Md5.Hash, 0, Md5HashSize);
                                d.Md5.Initialize();
                            } else {
                                int x = d.Md5.TransformBlock(readBuffer, 0, read, null, 0);
                                if (x != read)
                                    throw new IOException("MD5 TransformBlock error");
                            }
                        }
                    }

                    // Search for a matching hash
                    var md5 = MD5.Create();
                    foreach (var d in digests) {
                        d.Hashes.Position = 0;
                        var digest = md5.ComputeHash(d.Hashes);
                        var hexDigest = BitConverter.ToString(digest).Replace("-", "").ToLower() + "-" + digestPartCount;
                        if (hexDigest.Equals(Digest)) {
                            PartSize = d.PartSize;
                            return true;
                        }
                    }

                    // No match, just return false.
                    return false;
                }
            }
        }

        internal QBucketEntry(string directoryPath) {
            Name = directoryPath;
            FileFlags = FileFlags.Directory;
        }

        internal QBucketEntry(Amazon.S3.Model.S3Object obj, long defaultUploadPartSize, List<long> availableMultipartSizes) {
            Size = obj.Size;
            Name = obj.Key;
            LastModified = obj.LastModified;
            FileFlags = FileFlags.File;
            Digest = obj.ETag != null ? obj.ETag.Replace("\"", "").ToLower():"";
            AvailableMultipartSizes = availableMultipartSizes == null ? new List<long>():availableMultipartSizes;
            DefaultUploadPartSize = defaultUploadPartSize;
        }
    }

    /// <summary>
    /// This class manages the Qarnot Ceph buckets (S3 compatible).
    /// </summary>
    public class QBucket : QAbstractStorage {
        private Connection _api;
        private long _usedSpaceBytes;
        private int _fileCount;

        private static readonly char S3DirectorySeparator = '/';

        /// <summary>
        /// The bucket name.
        /// </summary>
        public override string UniqueId { get { return Shortname; } }
        /// <summary>
        /// The inner Connection object.
        /// </summary>
        public override Connection Connection { get { return _api; } }
        /// <summary>
        /// The bucket name.
        /// </summary>
        public override string Shortname { get; protected set; }
        /// <summary>
        /// Number of files in this bucket.
        /// Use Update or UpdateAsync to refresh.
        /// Note: only available for Qarnot's Buckets.
        /// </summary>
        public override int FileCount { get { return _fileCount; } }
        /// <summary>
        /// Size of this bucket in bytes.
        /// Use Update or UpdateAsync to refresh.
        /// Note: only available for Qarnot's Buckets.
        /// </summary>
        public override long UsedSpaceBytes { get { return _usedSpaceBytes; } }
        /// <summary>
        /// The bucket creation date
        /// </summary>
        public override DateTime CreationDate { get; }

        /// <summary>
        /// Add filtering to the bucket content so that only part of it is made available to tasks
        /// </summary>
        public ABucketFiltering Filtering { get; protected set; }

        /// <summary>
        /// Add a transformation to resources as seen in the execution environment
        /// </summary>
        public AResourcesTransformation ResourcesTransformation { get; protected set; }

        /// <summary>
        /// Time to live for the resource cache (in seconds).
        /// Override the task default value DefaultResourcesCacheTTLSec
        /// </summary>
        public int? CacheTTLSec { get; protected set; }

        /// <summary>
        /// Create a bucket object.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="shortname">The bucket name.</param>
        /// <param name="create">bool, to create the resource on the api too.</param>
        public QBucket(Connection connection, string shortname, bool create=true) {
            _api = connection;
            Shortname = shortname;

            if (create)
                this.CreateAsync().Wait();
        }

        internal QBucket() {}

        internal QBucket(QBucket originalBucket) : this(originalBucket.Connection, originalBucket.Shortname, create: false) {
            Filtering = originalBucket.Filtering;
            ResourcesTransformation = originalBucket.ResourcesTransformation;
            CacheTTLSec = originalBucket.CacheTTLSec;
        }

        internal QBucket(Connection connection, Amazon.S3.Model.S3Bucket s3Bucket) : this(connection, s3Bucket.BucketName, create: false) {
            CreationDate = s3Bucket.CreationDate;
        }

        internal async Task<QBucket> InitializeAsync(Connection qapi, string shortname, bool create=true, CancellationToken ct=default(CancellationToken)) {
             _api = qapi;
            Shortname = shortname;

            if (create)
                await this.CreateAsync(ct);
            return this;
        }


        internal async Task<QBucket> InitializeAsync(Connection qapi, ApiAdvancedResourceBucket advancedBucket, bool create=true, CancellationToken ct=default(CancellationToken)) {
             _api = qapi;
            Shortname = advancedBucket.BucketName;
            CacheTTLSec = advancedBucket.CacheTTLSec;
            if (advancedBucket.Filtering?.PrefixFiltering != null) {
                if (_api._shouldSanitizeBucketPaths)
                {
                    advancedBucket.Filtering.PrefixFiltering.Prefix = Utils.GetSanitizedBucketPath(advancedBucket.Filtering.PrefixFiltering.Prefix, _api._showBucketWarnings);
                }
                Filtering = new BucketFilteringPrefix(advancedBucket.Filtering.PrefixFiltering.Prefix);
            }
            if (advancedBucket.ResourcesTransformation?.StripPrefix != null) {
                if (_api._shouldSanitizeBucketPaths)
                {
                    advancedBucket.ResourcesTransformation.StripPrefix.Prefix = Utils.GetSanitizedBucketPath(advancedBucket.ResourcesTransformation.StripPrefix.Prefix, _api._showBucketWarnings);
                }
                ResourcesTransformation = new ResourcesTransformationStripPrefix(advancedBucket.ResourcesTransformation.StripPrefix.Prefix);
            }

            if (create)
                await this.CreateAsync(ct);
            return this;
        }

        internal async static Task<QBucket> CreateAsync(Connection qapi, string shortname, bool create=true, CancellationToken ct=default(CancellationToken)) {
            return await new QBucket().InitializeAsync(qapi, shortname, create, ct);
        }

        internal async static Task<QBucket> CreateAsync(Connection qapi, ApiAdvancedResourceBucket advancedBucket, bool create=true, CancellationToken ct=default(CancellationToken)) {
            return await new QBucket().InitializeAsync(qapi, advancedBucket, create, ct);
        }

        internal static  List<QBucket> GetBucketsFromResources(IEnumerable<QAbstractStorage> storages) {
            var buckets = new List<QBucket>();
            if (storages != null) {
                foreach (var d in storages) {
                    if (d is QBucket) buckets.Add((QBucket)d);
                }
            }
            return buckets;
        }

        internal static QBucket GetBucketFromResource(QAbstractStorage storage) {
            if (storage is QBucket) return (QBucket)storage;
            return null;
        }

        /// <summary>
        /// Create the bucket.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't create buckets, this connection is configured in read-only mode");

            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                var s3Response = await s3Client.PutBucketAsync(Shortname, cancellationToken);
            }
        }

        /// <summary>
        /// Delete this bucket.
        /// Note: this method also remove all the objects inside this bucket.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't delete buckets, this connection is configured in read-only mode");

            try {
                // First, we have to empty the bucket
                await DeleteEntryAsync(String.Empty, cancellationToken);

                using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                    // Then we can delete it
                    var s3Request = new Amazon.S3.Model.DeleteBucketRequest {
                        BucketName = Shortname
                    };
                    var s3Response = await s3Client.DeleteBucketAsync(s3Request, cancellationToken);
                }
            } catch (Amazon.S3.AmazonS3Exception ex) {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new QarnotApiResourceNotFoundException(ex.Message, ex);
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Update this bucket statistics.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public override async Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {

                var s3Request = new Amazon.S3.Model.GetPreSignedUrlRequest() {
                    BucketName = Shortname,
                    Verb = Amazon.S3.HttpVerb.HEAD,
                    Expires = DateTime.Now.Add(TimeSpan.FromSeconds(60)),
                    Protocol = _api.StorageUri.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase) ? Amazon.S3.Protocol.HTTP : Amazon.S3.Protocol.HTTPS
                };

                var s3Response = s3Client.GetPreSignedURL(s3Request);

                using (HttpClient client = _api._httpClientHandler == null ? new HttpClient():new HttpClient(_api._httpClientHandler, false)) {
                    var request = new HttpRequestMessage() {
                        Method = HttpMethod.Head,
                        RequestUri = new Uri(s3Response),
                    };
                    var response = await client.SendAsync(request, cancellationToken);
                    IEnumerable<string> radosGWBucketKeyCount;
                    IEnumerable<string> radosGWBucketSize;
                    if (response.Headers.TryGetValues("X-RGW-Object-Count", out radosGWBucketKeyCount)) {
                        foreach(var x in radosGWBucketKeyCount) {
                            if (int.TryParse(x, out _fileCount)) break;
                        }
                    }
                    if (response.Headers.TryGetValues("X-RGW-Bytes-Used", out radosGWBucketSize)) {
                        foreach (var x in radosGWBucketSize) {
                            if (long.TryParse(x, out _usedSpaceBytes)) break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Write a stream to a file in this bucket.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="remoteFile">The destination file name in this bucket.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public override async Task UploadStreamAsync(Stream sourceStream, string remoteFile, CancellationToken cancellationToken = default(CancellationToken))
            => await UploadStreamAsync(sourceStream, remoteFile, pathDirectorySeparator: Path.DirectorySeparatorChar, cancellationToken: cancellationToken);


        /// <summary>
        /// Write a stream to a file in this bucket.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="remoteFile">The destination file name in this bucket.</param>
        /// <param name="pathDirectorySeparator">PathDirectorySeparator char that will change the remote file path to match the folder hierarchy ('/' on linux, '\' on windows).</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public override async Task UploadStreamAsync(Stream sourceStream, string remoteFile, char pathDirectorySeparator, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_api.IsReadOnly) throw new Exception("Can't upload to buckets, this connection is configured in read-only mode");
            string remoteS3FileKey = pathDirectorySeparator == default(char) ? remoteFile : remoteFile.Replace(pathDirectorySeparator, S3DirectorySeparator);
            if (_api._shouldSanitizeBucketPaths)
            {
                remoteS3FileKey = Utils.GetSanitizedBucketPath(remoteS3FileKey, _api._showBucketWarnings);
            }
            if (_api.StorageUploadPartSize <= 0) {
                using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                    var s3Request = new Amazon.S3.Model.PutObjectRequest {
                        BucketName = Shortname,
                        Key = remoteS3FileKey,
                        InputStream = sourceStream,
                        AutoCloseStream = false
                    };
                    var s3Response = await s3Client.PutObjectAsync(s3Request, cancellationToken);
                }
            } else {
                using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                    var fileTransferUtilityRequest = new Amazon.S3.Transfer.TransferUtilityUploadRequest {
                        BucketName = Shortname,
                        InputStream = sourceStream,
                        AutoCloseStream = false,
                        PartSize = _api.StorageUploadPartSize,
                        Key = remoteS3FileKey
                    };

                    using (var fileTransferUtility = new Amazon.S3.Transfer.TransferUtility(s3Client))
                        await fileTransferUtility.UploadAsync(fileTransferUtilityRequest, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Get a stream on a file in this bucket.
        /// </summary>
        /// <param name="remoteFile">The source file name in this bucket.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A stream with the file's data.</returns>
        public override async Task<Stream> DownloadStreamAsync(string remoteFile, CancellationToken cancellationToken = default(CancellationToken))
            => await DownloadStreamAsync(remoteFile, pathDirectorySeparator: Path.DirectorySeparatorChar, cancellationToken: cancellationToken);

        /// <summary>
        /// Get a stream on a file in this bucket.(should be disposed)
        /// </summary>
        /// <param name="remoteFile">The source file name in this bucket.</param>
        /// <param name="pathDirectorySeparator">PathDirectorySeparator char that will change the remote file path to match the folder hierarchy ('/' on linux, '\' on windows).</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A stream with the file's data.</returns>
        public override async Task<Stream> DownloadStreamAsync(string remoteFile, char pathDirectorySeparator, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                string remoteS3FileKey = pathDirectorySeparator == default(char) ? remoteFile : remoteFile.Replace(pathDirectorySeparator, S3DirectorySeparator);
                if (_api._shouldSanitizeBucketPaths)
                {
                    remoteS3FileKey= Utils.GetSanitizedBucketPath(remoteS3FileKey, _api._showBucketWarnings);
                }
                var s3Request = new Amazon.S3.Model.GetObjectRequest {
                    BucketName = Shortname,
                    Key = remoteS3FileKey,
                };
                var s3Response = await s3Client.GetObjectAsync(s3Request, cancellationToken);
                return s3Response.ResponseStream;
            }
        }

        /// <summary>
        /// Delete a file or folder in this bucket.
        /// Note: this method also remove all the sub-objects inside the deleted folder.
        /// </summary>
        /// <param name="remotePath">The entry to remove.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns></returns>
        public override async Task DeleteEntryAsync(string remotePath, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't delete entries from buckets, this connection is configured in read-only mode");

            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                if (_api._shouldSanitizeBucketPaths)
                {
                    remotePath = Utils.GetSanitizedBucketPath(remotePath, _api._showBucketWarnings);
                }
                if (remotePath.EndsWith("/") || string.IsNullOrWhiteSpace(remotePath)) {
                    // It's a 'folder', we have to delete all the sub keys first
                    var s3ListRequest = new Amazon.S3.Model.ListObjectsRequest {
                        BucketName = Shortname,
                        MaxKeys = 1000,
                        Prefix = remotePath == "/" ? "":remotePath,
                    };

                    Amazon.S3.Model.ListObjectsResponse s3ListResponse;
                    do {
                        s3ListResponse = await s3Client.ListObjectsAsync(s3ListRequest, cancellationToken);

                        var s3DeleteRequest = new Amazon.S3.Model.DeleteObjectsRequest {
                            BucketName = Shortname,
                            Quiet = true,
                            Objects = s3ListResponse.S3Objects.Select(obj => new Amazon.S3.Model.KeyVersion { Key = obj.Key }).ToList()
                        };

                        await s3Client.DeleteObjectsAsync(s3DeleteRequest, cancellationToken);

                        s3ListRequest.Marker = s3ListResponse.NextMarker;

                    } while (s3ListResponse.IsTruncated);
                }

                // Then we can delete the entry
                if (!String.IsNullOrEmpty(remotePath)) {
                    var s3Request = new Amazon.S3.Model.DeleteObjectRequest {
                        BucketName = Shortname,
                        Key = remotePath
                    };
                    var s3Response = await s3Client.DeleteObjectAsync(s3Request, cancellationToken);
                }
            }
        }

        /// <summary>
        /// List the files and folders entries in the corresponding folder (without content of subfolders).
        /// </summary>
        /// <param name="remoteFolder">The folder to list.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of QAbstractStorageEntry</returns>
        public override async Task<List<QAbstractStorageEntry>> ListEntriesAsync(string remoteFolder="", CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api._shouldSanitizeBucketPaths)
            {
                remoteFolder = Utils.GetSanitizedBucketPath(remoteFolder, _api._showBucketWarnings);
            }
            if (remoteFolder.Length != 0 && !remoteFolder.EndsWith("/"))
                remoteFolder += '/';

            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                var s3Request = new Amazon.S3.Model.ListObjectsRequest {
                    BucketName = Shortname,
                    MaxKeys = 1000,
                    Prefix = remoteFolder,
                    Delimiter = "/",
                };

                var files = new List<QAbstractStorageEntry>();
                Amazon.S3.Model.ListObjectsResponse s3Response;
                do {
                    s3Response = await s3Client.ListObjectsAsync(s3Request, cancellationToken);

                    foreach (var obj in s3Response.CommonPrefixes) {
                        // Folders
                        files.Add(new QBucketEntry(obj));
                    }

                    foreach (var obj in s3Response.S3Objects) {
                        // Files
                        if (obj.Key.EndsWith("/")) continue;
                        files.Add(new QBucketEntry(obj, _api.StorageUploadPartSize, _api.StorageAvailablePartSizes));
                    }

                    s3Request.Marker = s3Response.NextMarker;
                } while (s3Response.IsTruncated);

                return files;
            }
        }

        /// <summary>
        /// List all files and folders from the root of the bucket.
        /// </summary>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of storage entries</returns>
        public override Task<List<QAbstractStorageEntry>> ListFilesAsync(CancellationToken cancellationToken = default) {
            return ListFilesAsync(prefix: default, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all files and folders from the root of the bucket.
        /// </summary>
        /// <param name="prefix">Prefix for file search.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of storage entries</returns>
        public override async Task<List<QAbstractStorageEntry>> ListFilesAsync(
            string prefix,
            CancellationToken cancellationToken = default)
        {
            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken))
            {
                if (_api._shouldSanitizeBucketPaths)
                {
                    prefix = Utils.GetSanitizedBucketPath(prefix, _api._showBucketWarnings);
                }

                var s3Request = new Amazon.S3.Model.ListObjectsRequest
                {
                    Prefix = prefix,
                    BucketName = Shortname,
                    MaxKeys = 1000,
                };

                var files = new List<QAbstractStorageEntry>();
                Amazon.S3.Model.ListObjectsResponse s3Response;
                do {
                    s3Response = await s3Client.ListObjectsAsync(s3Request, cancellationToken);
                    foreach (var obj in s3Response.S3Objects) {
                        if (obj.Key.EndsWith("/")) // folder
                            files.Add(new QBucketEntry(obj.Key));
                        else// files
                            files.Add(new QBucketEntry(obj, _api.StorageUploadPartSize, _api.StorageAvailablePartSizes));
                    }

                    // handle pagination
                    s3Request.Marker = s3Response.NextMarker;
                } while (s3Response.IsTruncated);

                return files;
            }
        }


        /// <summary> Returns a copy of the bucket object with a filtering added </summary>
        /// <param name="filtering"> The filtering to add </param>
        public QBucket WithFiltering(ABucketFiltering filtering) {
            var newBucket = new QBucket(this);
            if (_api._shouldSanitizeBucketPaths)
            {
                filtering.SanitizePaths(_api._showBucketWarnings);
            }
            newBucket.Filtering = filtering;
            return newBucket;
        }


        /// <summary> Returns a copy of the bucket object with a resources transformation added </summary>
        /// <param name="transformation"> The resources transformation to add </param>
        public QBucket WithResourcesTransformation(AResourcesTransformation transformation) {
            var newBucket = new QBucket(this);
            if (_api._shouldSanitizeBucketPaths)
            {
                transformation.SanitizePaths(_api._showBucketWarnings);
            }
            newBucket.ResourcesTransformation = transformation;
            return newBucket;
        }


        /// <summary> Returns a copy of the bucket object with a cache TTL added </summary>
        /// <param name="ttl"> The resources cache TTL to add </param>
        public QBucket WithCacheTTL(int ttl) {
            var newBucket = new QBucket(this);
            newBucket.CacheTTLSec = ttl;
            return newBucket;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QarnotSDK {
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
        public DateTime LastModified { get; protected set; }

        /// <summary>
        /// Size of one part if the file was uploaded in multi-part.
        /// Note: this value is filled only after a successful call to EqualsLocalFileDigest or EqualsLocalFileDigestAsync.
        /// </summary>
        public long PartSize { get; private set; }

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
        private readonly Connection _api;
        private long _usedSpaceBytes;
        private int _fileCount;

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
        public override string Shortname { get; }
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
        /// Create a bucket object.
        /// </summary>
        /// <param name="connection">The inner connection object.</param>
        /// <param name="shortname">The bucket name.</param>
        public QBucket(Connection connection, string shortname) {
            _api = connection;
            Shortname = shortname;
        }

        internal QBucket(Connection connection, Amazon.S3.Model.S3Bucket s3Bucket) : this(connection, s3Bucket.BucketName) {
            CreationDate = s3Bucket.CreationDate;
        }

        /// <summary>
        /// Create the bucket.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't create buckets, this connection is configured in read-only mode");

            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                var s3Request = new Amazon.S3.Model.PutBucketRequest {
                    BucketName = Shortname
                };
                var s3Response = await s3Client.PutBucketAsync(s3Request, cancellationToken);
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
                await DeleteEntryAsync("/", cancellationToken);

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
        public override async Task UploadStreamAsync(Stream sourceStream, string remoteFile, CancellationToken cancellationToken = default(CancellationToken)) {
            if (_api.IsReadOnly) throw new Exception("Can't upload to buckets, this connection is configured in read-only mode");

            if (_api.StorageUploadPartSize <= 0) {
                using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                    var s3Request = new Amazon.S3.Model.PutObjectRequest {
                        BucketName = Shortname,
                        Key = remoteFile
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
                        Key = remoteFile
                    };

                    var fileTransferUtility = new Amazon.S3.Transfer.TransferUtility(s3Client);
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
        public override async Task<Stream> DownloadStreamAsync(string remoteFile, CancellationToken cancellationToken = default(CancellationToken)) {
            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                var s3Request = new Amazon.S3.Model.GetObjectRequest {
                    BucketName = Shortname,
                    Key = remoteFile
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
                if (remotePath.EndsWith("/")) {
                    // It's a 'folder', we have to delete all the sub keys first
                    var s3ListRequest = new Amazon.S3.Model.ListObjectsV2Request {
                        BucketName = Shortname,
                        MaxKeys = 1000,
                        Prefix = remotePath == "/" ? "":remotePath,
                    };

                    Amazon.S3.Model.ListObjectsV2Response s3ListResponse;
                    do {
                        s3ListResponse = await s3Client.ListObjectsV2Async(s3ListRequest, cancellationToken);

                        var s3DeleteRequest = new Amazon.S3.Model.DeleteObjectsRequest {
                            BucketName = Shortname,
                            Quiet = true,
                            Objects = s3ListResponse.S3Objects.Select(obj => new Amazon.S3.Model.KeyVersion { Key = obj.Key }).ToList()
                        };

                        await s3Client.DeleteObjectsAsync(s3DeleteRequest, cancellationToken);

                        s3ListRequest.ContinuationToken = s3ListRequest.ContinuationToken;
                    } while (s3ListResponse.IsTruncated);
                }

                // Then we can delete the entry
                var s3Request = new Amazon.S3.Model.DeleteObjectRequest {
                    BucketName = Shortname,
                    Key = remotePath
                };
                var s3Response = await s3Client.DeleteObjectAsync(s3Request, cancellationToken);
            }
        }

        /// <summary>
        /// List the files and folders.
        /// </summary>
        /// <param name="remoteFolder">The folder to list.</param>
        /// <param name="cancellationToken">Optional token to cancel the request.</param>
        /// <returns>A list of QAbstractStorageEntry</returns>
        public override async Task<List<QAbstractStorageEntry>> ListEntriesAsync(string remoteFolder, CancellationToken cancellationToken = default(CancellationToken)) {
            if (remoteFolder == null || remoteFolder == "/")
                remoteFolder = "";
            if (remoteFolder.Length != 0 && !remoteFolder.EndsWith("/"))
                remoteFolder += '/';

            using (var s3Client = await _api.GetS3ClientAsync(cancellationToken)) {
                var s3Request = new Amazon.S3.Model.ListObjectsV2Request {
                    BucketName = Shortname,
                    MaxKeys = 1000,
                    Delimiter = "/",
                    Prefix = remoteFolder
                };

                var files = new List<QAbstractStorageEntry>();
                Amazon.S3.Model.ListObjectsV2Response s3Response;
                do {
                    s3Response = await s3Client.ListObjectsV2Async(s3Request, cancellationToken);

                    foreach (var obj in s3Response.CommonPrefixes) {
                        // Folders
                        files.Add(new QBucketEntry(obj));
                    }

                    foreach (var obj in s3Response.S3Objects) {
                        // Files
                        if (obj.Key.EndsWith("/")) continue;
                        files.Add(new QBucketEntry(obj, _api.StorageUploadPartSize, _api.StorageAvailablePartSizes));
                    }

                    s3Request.ContinuationToken = s3Request.ContinuationToken;
                } while (s3Response.IsTruncated);

                return files;
            }
        }

    }
}

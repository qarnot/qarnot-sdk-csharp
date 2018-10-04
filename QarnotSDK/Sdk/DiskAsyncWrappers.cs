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
    /// This class manages the Qarnot Disks.
    /// </summary>
    public partial class QDisk {
        /// <summary>
        /// Create the disk.
        /// </summary>
        /// <param name="dontFailIfExists">If true, no exception is thrown if the disk already exists.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        public void Create(bool dontFailIfExists, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                CreateAsync(dontFailIfExists, cancellationToken).Wait();
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
        public void Lock(bool lockState, CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                LockAsync(lockState, cancellationToken).Wait();
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
        public List<QAbstractStorageEntry> Tree(CancellationToken cancellationToken = default(CancellationToken)) {
            try {
                return TreeAsync(cancellationToken).Result;
            } catch (AggregateException ex) {
                throw ex.InnerException;
            }
        }
    }
}


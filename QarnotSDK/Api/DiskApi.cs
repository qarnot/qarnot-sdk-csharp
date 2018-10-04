using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace QarnotSDK {
    /// <summary>
    /// Represents the type of an entry (file, executable or folder) in a QDisk.
    /// </summary>
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileFlags {
        /// <summary>
        /// A file
        /// </summary>
        File = 1,
        /// <summary>
        /// A directory
        /// </summary>
        Directory = 2,
        /// <summary>
        /// An executable file
        /// </summary>
        ExecutableFile = 3
    }

    internal class FileApi {
        public long Size { get; set; }
        public string Name { get; set; }
        public string CreationDate { get; set; } // ISO 8601
        public FileFlags FileFlags { get; set; }
        public string Sha1sum { get; set; }

        internal FileApi() {
        }
    }

    internal class LockApi {
        public bool Locked { get; set; }

        public LockApi() { }
        internal LockApi(bool locked) {
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

        internal DiskApi() {
        }
    }
}


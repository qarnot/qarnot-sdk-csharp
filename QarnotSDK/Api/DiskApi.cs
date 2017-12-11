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
        File = 1,
        Directory = 2,
        ExecutableFile = 3
    }

    /// <summary>
    /// Represents an entry (file or folder) in a QDisk.
    /// </summary>
    public class QFile {
        public long Size { get; set; }
        public string Name { get; set; }
        public string CreationDate { get; set; }
        public FileFlags FileFlags { get; set; }
        public string Sha1sum { get; set; }

        public string GetNormalizedName() {
            var n = Name;
            if (n.EndsWith("/")) {
                n = n.Substring(0, n.Length - 1);
            }
            if (n.StartsWith("/")) {
                n = n.Substring(1);
            }
            return n;
        }

        public QFile() {
        }

        /*
        public QFile(string path, int prefixlen, bool isdir) {
            if (isdir)
                Size = 0;
            else
                Size = new FileInfo(path).Length;
            CreationDate = File.GetCreationTime(path).ToString("o", System.Globalization.CultureInfo.InvariantCulture); // ISO 8601
            Name = path.Substring(prefixlen);

            if (isdir)
                FileFlags = FileFlags.Directory;
            else
                FileFlags = FileFlags.File;
        }
        */
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


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace QarnotSDK {
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileFlags {
        File = 1,
        Directory = 2,
        ExecutableFile = 3
    }

    public class QFile {
        public long Size { get; set; }

        public string Name { get; set; }

        public string CreationDate { get; set; }

        public FileFlags FileFlags { get; set; }

        public QFile() {
        }

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
    }

    // This class is used to serialize constants in pools and tasks
    internal class KeyValHelper {
        public string Key { get; set; }

        public string Value { get; set; }

        public KeyValHelper(string key, string value) {
            Key = key;
            Value = value;
        }

        public KeyValHelper() {
        }
    }

    // This class is used to serialize errors
    internal class Error {
        public string Message { get; set; }

        public Error(string msg) {
            Message = msg;
        }

        public Error() {
        }
    }
}

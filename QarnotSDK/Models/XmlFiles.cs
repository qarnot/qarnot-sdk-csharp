using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace qarnotsdk
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FileFlags {
        File = 1,
        Directory = 2,
        ExecutableFile = 3
    }

    public class MyFile
    {
        public long Size { get; set; }

        public string Name { get; set; }

        public string CreationDate { get; set; }

        public FileFlags FileFlags { get; set; }

        public MyFile() {
        }

        public MyFile(string path, int prefixlen, bool isdir)
        {
            if (isdir)
                Size = 0;
            else
                Size = new FileInfo (path).Length;
            CreationDate = File.GetCreationTime (path).ToString("o", System.Globalization.CultureInfo.InvariantCulture); // ISO 8601
            Name = path.Substring (prefixlen);

            if (isdir)
                FileFlags = FileFlags.Directory;
            else
                FileFlags = FileFlags.File;
        }
    }
}


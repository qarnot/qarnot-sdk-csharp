using System;
using System.Collections.Generic;

namespace QarnotSDK
{

    internal class JobApi
    {
        public string Name { get; set; }
        public Guid Uuid { get; set; }
        public string PoolUuid { get; set; }
        public string State { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }
        public bool UseDependencies {get; set; }

        internal JobApi()
        {
            UseDependencies = false;
        }

        public override string ToString()
        {
            return string.Format(
                "[JobApi: Name={0}, Uuid={1}, PoolUuid={2}, State={3}, UseDependencies={4}, CreationDate={5}, LastModified={6} ]",
                Name, Uuid, PoolUuid, State, UseDependencies, CreationDate, LastModified
            );
        }
    }
}

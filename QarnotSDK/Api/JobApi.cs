using System;
using System.Collections.Generic;

namespace QarnotSDK
{

    internal class JobApi
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public string Shortname { get; set; }
        public string PoolUuid { get; set; }
        public string State { get; set; }
        public string PreviousState { get; set; }
        public DateTime StateTransitionTime { get; set; }
        public DateTime PreviousStateTransitionTime { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }
        public bool UseDependencies {get; set; }
        public Nullable<TimeSpan> MaxWallTime { get; set; }
        public bool AutoDeleteOnCompletion { get; set; }
        public TimeSpan CompletionTimeToLive { get; set; }
        public List<String> Tags { get; set; }


        internal JobApi()
        {
            Tags = new List<String>();
            UseDependencies = false;
            AutoDeleteOnCompletion = false;
        }

        public override string ToString()
        {
            return string.Format(
                "[JobApi: Name={0}, Shortname={1}, Uuid={2}, PoolUuid={3}, State={4}, UseDependencies={5}, CreationDate={6}, LastModified={7}, MaxWallTime{8} ]",
                Name, Shortname, Uuid, PoolUuid, State, UseDependencies, CreationDate, LastModified, MaxWallTime
            );
        }
    }
}

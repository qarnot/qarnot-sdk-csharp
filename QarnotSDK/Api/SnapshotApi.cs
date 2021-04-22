
namespace QarnotSDK
{
    /// <summary>
    /// Represents the Snapshot objects send to te API.
    /// </summary>
    internal class Snapshot
    {
        public string Whitelist { get; set; }
        public string Blacklist { get; set; }
        public string Bucket { get; set; }
        public string BucketPrefix { get; set; }
    }

    internal class PeriodicSnapshot : Snapshot
    {
        public override string ToString()
        {
            return string.Format("[Whitelist: {0}, Blacklist: {1}, Bucket: {2}, BucketPrefix: {3}, Interval: {4}]", Whitelist, Blacklist, Bucket, BucketPrefix, Interval);
        }

        public int Interval { get; set; }

        internal PeriodicSnapshot()
        {
        }
    }

    internal class UniqueSnapshot : Snapshot
    {
        public override string ToString()
        {
            return string.Format("[Whitelist: {0}, Blacklist: {1}, Bucket:{2}, BucketPrefix{3}]", Whitelist, Blacklist, Bucket, BucketPrefix);
        }

        internal UniqueSnapshot()
        {
        }
    }
}

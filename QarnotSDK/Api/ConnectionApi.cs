using System.Collections.Generic;

namespace QarnotSDK {
    /// <summary>
    /// Represents the Api settings.
    /// </summary>
    public class ApiSettings {
        /// <summary>
        /// Bucket storage endpoint Uri.
        /// </summary>
        public string Storage;

        internal ApiSettings() {
        }
    }

    /// <summary>
    /// Represents the quotas and disks information.
    /// </summary>
    public class UserInformation {
        /// <summary>
        /// User email address.
        /// </summary>
        public string Email;

        /// <summary>
        /// Maximum number of disks the user is allowed to create.
        /// </summary>
        public int MaxDisk;
        /// <summary>
        /// Number of disks owned by the user.
        /// </summary>
        public int DiskCount;
        /// <summary>
        /// Allowed quota in bytes for the user.
        /// </summary>
        public long QuotaBytes;
        /// <summary>
        /// Currently used quota in bytes.
        /// </summary>
        public long UsedQuotaBytes;

        /// <summary>
        /// Maximum number of tasks the user is allowed to create.
        /// </summary>
        public int MaxTask;
        /// <summary>
        /// Total number of tasks belonging to the user.
        /// </summary>
        public int TaskCount;
        /// <summary>
        /// Maximum number of running tasks the user is allowed to create.
        /// </summary>
        public int MaxRunningTask;
        /// <summary>
        /// Number of tasks currently submitted or running.
        /// </summary>
        public int RunningTaskCount;
        /// <summary>
        /// Maximum number of concurrent allowed instances.
        /// </summary>
        public int MaxInstances;

        internal UserInformation() {
        }
    }

    /// <summary>
    /// Represents a Constant in a profile.
    /// </summary>
    public class Constant {
        /// <summary>
        /// The constant name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The constant default value.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// The constant description.
        /// </summary>
        public string Description { get; set; }
    }

    internal class ProfilesConstantApi {
        public string Name { get; set; }
        public List<Constant> Constants { get; set; }
        ProfilesConstantApi() {
            Constants = new List<Constant>();
        }
    }
}

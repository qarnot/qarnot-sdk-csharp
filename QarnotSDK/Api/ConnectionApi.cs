using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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
    /// Represents the quotas and buckets information.
    /// </summary>
    public class UserInformation {
        /// <summary>
        /// User email address.
        /// </summary>
        public string Email;

        /// <summary>
        /// Maximum number of buckets the user is allowed to create.
        /// </summary>
        public int MaxBucket;
        /// <summary>
        /// Number of buckets owned by the user.
        /// </summary>
        public int BucketCount = -1;

        /// <summary>
        /// Allowed quota in bytes for the user.
        /// </summary>
        public long QuotaBytesBucket;
        /// <summary>
        /// Currently used quota in bytes.
        /// </summary>
        public long UsedQuotaBytesBucket;

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

        /// <summary>
        /// Maximum number of pools the user is allowed to create.
        /// </summary>
        public int MaxPool;
        /// <summary>
        /// Total number of pools belonging to the user.
        /// </summary>
        public int PoolCount;
        /// <summary>
        /// Maximum number of running pools the user is allowed to create.
        /// </summary>
        public int MaxRunningPool;
        /// <summary>
        /// Number of pools currently submitted or running.
        /// </summary>
        public int RunningPoolCount;

        /// <summary>
        /// Number of Instances currently submitted or running.
        /// </summary>
        public int RunningInstanceCount { get; set; }
        /// <summary>
        /// Number of cores currently submitted or running.
        /// </summary>
        public int RunningCoreCount { get; set; }

        /// <summary>
        /// Computing quotas of the user and their organization
        /// </summary>
        public ComputingQuotas ComputingQuotas { get; set; }

        /// <summary>
        /// Projects available to this user. Use <see cref="GetProjectByName"/> or
        /// <see cref="GetProjectBySlug"/> for convenient lookup before assigning to a task or pool.
        /// </summary>
        [JsonIgnore]
        public List<QProject> Projects
        {
            get
            {
                return _rawProjects?.Select(p => new QProject(p)).ToList()
                    ?? new List<QProject>();
            }
        }

        [JsonProperty("projects")]
        internal List<ProjectApi> _rawProjects { get; set; }

        /// <summary>
        /// Returns the first project whose name matches <paramref name="name"/> (case-insensitive),
        /// or null if not found.
        /// </summary>
        public QProject GetProjectByName(string name)
        {
            return Projects.FirstOrDefault(
                p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the first project whose slug matches <paramref name="slug"/> (case-insensitive),
        /// or null if not found.
        /// </summary>
        public QProject GetProjectBySlug(string slug)
        {
            return Projects.FirstOrDefault(
                p => string.Equals(p.Slug, slug, StringComparison.OrdinalIgnoreCase));
        }

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

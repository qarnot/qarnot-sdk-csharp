using System.Collections.Generic;
using System.Linq;

namespace QarnotSDK
{
    /// <summary>
    /// Description of all the secrets a task will have access to
    /// when running.
    /// </summary>
    public class QSecretAccessRights
    {
        /// <summary>
        /// Build an empty list of secrets access rights.
        /// </summary>
        public QSecretAccessRights()
        {
            BySecret = new List<QSecretAccessRightBySecret>();
            ByPrefix = new List<QSecretAccessRightByPrefix>();
        }

        /// <summary>
        /// The secrets the task will have access to, described using
        /// an exacty key match.
        /// </summary>
        public List<QSecretAccessRightBySecret> BySecret { get; set; }

        /// <summary>
        /// The secrets the task will have access to, described using
        /// a prefix key match.
        /// </summary>
        public List<QSecretAccessRightByPrefix> ByPrefix { get; set; }

        /// <summary>
        /// Add <paramref name="key" /> as an available secret to
        /// the task.
        /// </summary>
        /// <param name="key">Key to exactly match secrets on</param>
        public QSecretAccessRights AddSecretByKey(string key)
        {
            BySecret.Add(new QSecretAccessRightBySecret { Key = key });
            return this;
        }

        /// <summary>
        /// Add all <paramref name="keys" /> as available secrets to the
        /// task.
        /// </summary>
        /// <param name="keys">Keys to exactly match secrets on</param>
        public QSecretAccessRights AddSecretsByKey(IEnumerable<string> keys)
        {
            BySecret.AddRange(keys.Select(k => new QSecretAccessRightBySecret { Key = k }));
            return this;
        }

        /// <summary>
        /// Add all secrets starting with <paramref name="prefix" /> as
        /// available to the task.
        /// </summary>
        /// <param name="prefix">Prefix to match secret against</param>
        public QSecretAccessRights AddSecretByPrefix(string prefix)
        {
            ByPrefix.Add(new QSecretAccessRightByPrefix { Prefix = prefix });
            return this;
        }

        /// <summary>
        /// Add all secrets starting with any of the <paramref name="prefixes" /> as
        /// available to the task.
        /// </summary>
        /// <param name="prefixes">Prefixes to match secret against</param>
        public QSecretAccessRights AddSecretsByPrefix(IEnumerable<string> prefixes)
        {
            ByPrefix.AddRange(prefixes.Select(p => new QSecretAccessRightByPrefix { Prefix = p }));
            return this;
        }
    }

    /// <summary>
    /// Secret to be made available to a task, by exact match on its <c>Key</c> property.
    /// </summary>
    public class QSecretAccessRightBySecret
    {
        /// <summary>Used to exactly match secret against</summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// Secrets to be made available to a task, by prefix match on its <c>Prefix</c> property.
    /// </summary>
    public class QSecretAccessRightByPrefix
    {
        /// <summary>Used to prefix match secret against</summary>
        public string Prefix { get; set; }
    }
}

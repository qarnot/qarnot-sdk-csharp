using System;
using System.Collections.Generic;
using System.Linq;

namespace QarnotSDK
{
    /// <summary>
    /// Groups all dependency information for a task: declared simple and advanced
    /// dependencies, and the current state of dependency resolution.
    /// </summary>
    /// <remark>
    /// This is read-only, to set dependencies on tasks, use 
    /// <see cref="QTask.SetTaskDependencies(AdvancedDependency[])"/>.
    /// </remark>
    public class QTaskDependencies
    {
        /// <summary>
        /// List of task UUIDs this task depends on (simple dependencies).
        /// Empty when advanced dependencies are used.
        /// </summary>
        /// <remark>
        /// Considering that this is a proper subset of what can be expressed with
        /// <see cref="AdvancedDependsOn"/>, expect this field to become deprecated
        /// in some future.
        /// </remark>
        public IReadOnlyList<Guid> DependsOn { get; }

        /// <summary>
        /// List of advanced dependencies with per-dependency state conditions.
        /// Null or empty when simple dependencies are used.
        /// </summary>
        public IReadOnlyList<AdvancedDependency> AdvancedDependsOn { get; }

        /// <summary>
        /// The overall dependency state of this task, as an agregation of individual states.
        /// </summary>
        public DependencyState? State { get; }

        internal QTaskDependencies(Dependency apiDependency)
        {
            DependsOn = apiDependency.DependsOn != null
                ? apiDependency.DependsOn.AsReadOnly()
                : new List<Guid>().AsReadOnly();

            AdvancedDependsOn = ConvertAdvancedDependencies(apiDependency.AdvancedDependsOn);

            State = ParseDependencyState(apiDependency.State);
        }

        private static IReadOnlyList<AdvancedDependency> ConvertAdvancedDependencies(
            List<AdvancedDependencyApi> apiList)
        {
            if (apiList == null || apiList.Count == 0)
            {
                return null;
            }

            return apiList
                .Select(a => new AdvancedDependency(
                    a.TaskUuid,
                    ParseConditions(a.TaskFinalStateCondition),
                    a.ActualFinalState is null ? null : ParseTaskFinalState(a.ActualFinalState),
                    ParseDependencyState(a.State)))
                .ToList()
                .AsReadOnly();
        }

        private static IReadOnlyList<TaskFinalState> ParseConditions(List<string> conditions)
        {
            if (conditions == null || conditions.Count == 0)
            {
                return null;
            }

            return conditions
                .Select(ParseTaskFinalState)
                .Where(s => s.HasValue)
                .Select(s => s.Value)
                .ToList()
                .AsReadOnly();
        }

        private static TaskFinalState? ParseTaskFinalState(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (Enum.TryParse<TaskFinalState>(value, ignoreCase: true, out var result))
            {
                return result;
            }

            return null;
        }

        private static DependencyState? ParseDependencyState(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (Enum.TryParse<DependencyState>(value, ignoreCase: true, out var result))
            {
                return result;
            }

            return null;
        }
    }
}

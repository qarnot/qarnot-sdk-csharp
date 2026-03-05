using System;
using System.Collections.Generic;

namespace QarnotSDK
{
    /// <summary>
    /// Represents an advanced dependency on a specific task, with optional final state conditions.
    /// Use <see cref="QTask.SetTaskDependencies(AdvancedDependency[])"/> to set dependencies.
    /// </summary>
    public class AdvancedDependency
    {
        /// <summary>
        /// UUID of the task this dependency refers to.
        /// </summary>
        public Guid TaskUuid { get; }

        /// <summary>
        /// Required final states for the dependency to be fulfilled.
        /// Null or empty means any final state is accepted (task just needs to complete).
        /// </summary>
        public IReadOnlyList<TaskFinalState> TaskFinalStateCondition { get; }

        /// <summary>
        /// The actual final state of the dependency, if it's completed
        /// </summary>
        public TaskFinalState? ActualFinalState { get; }

        /// <summary>
        /// The current state of this dependency as reported by the API.
        /// </summary>
        public DependencyState? State { get; }

        /// <summary>
        /// Creates an advanced dependency on the given task UUID with optional final state conditions.
        /// </summary>
        /// <param name="taskUuid">UUID of the task to depend on.</param>
        /// <param name="conditions">
        /// Required final states. Pass none (or null) to accept any final state.
        /// </param>
        public AdvancedDependency(Guid taskUuid, params TaskFinalState[] conditions)
        {
            TaskUuid = taskUuid;
            TaskFinalStateCondition = conditions != null && conditions.Length > 0
                ? (IReadOnlyList<TaskFinalState>)conditions
                : null;
            ActualFinalState = null;
            State = null;
        }

        /// <summary>
        /// Creates an advanced dependency on the given task with optional final state conditions.
        /// </summary>
        /// <param name="task">The task to depend on.</param>
        /// <param name="conditions">
        /// Required final states. Pass none (or null) to accept any final state.
        /// </param>
        public AdvancedDependency(QTask task, params TaskFinalState[] conditions)
            : this(task.Uuid, conditions)
        {
        }

        /// <summary>
        /// Internal constructor used when deserializing from API responses (includes State).
        /// </summary>
        internal AdvancedDependency(
            Guid taskUuid,
            IReadOnlyList<TaskFinalState> conditions,
            TaskFinalState? actualFinalState,
            DependencyState? state)
        {
            TaskUuid = taskUuid;
            TaskFinalStateCondition = conditions;
            ActualFinalState = actualFinalState;
            State = state;
        }
    }
}

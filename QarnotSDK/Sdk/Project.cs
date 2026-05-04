using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QarnotSDK.Sdk;

namespace QarnotSDK
{
    /// <summary>
    /// Represents a Qarnot project. Projects group tasks and pools for quota and billing purposes.
    /// Assign a project to a task or pool via <see cref="QTask.ProjectUuid"/> or <see cref="QPool.ProjectUuid"/>
    /// before submission.
    /// </summary>
    public class QProject
    {
        private readonly ProjectApi _projectApi;

        /// <summary>The unique identifier of the project.</summary>
        public Guid Uuid => _projectApi.Uuid;

        /// <summary>The display name of the project.</summary>
        public string Name => _projectApi.Name;

        /// <summary>The UUID of the organization this project belongs to.</summary>
        public Guid OrganizationUuid => _projectApi.OrganizationUuid;

        /// <summary>The description of the project.</summary>
        public string Description => _projectApi.Description;

        /// <summary>
        /// The URL-friendly slug of the project.
        /// </summary>
        public string Slug => _projectApi.Slug;

        /// <summary>
        /// State if this project is the default one of the organization
        /// A default project is automatically selected at task or pool
        /// creation if none is provided
        /// </summary>
        public bool IsDefault => _projectApi.IsDefault;

        internal QProject(ProjectApi projectApi)
        {
            _projectApi = projectApi;
        }

        /// <summary>
        /// Retrieve the active budgets for this project.
        /// </summary>
        /// <param name="connection">The connection to use for the API call.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of active budgets.</returns>
        public async Task<List<Budget>> RetrieveActiveBudgetsAsync(Connection connection, CancellationToken ct = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            return await connection.CreditsClient.GetBudgetsAsync(Uuid, activeOnly: true, ct);
        }

        /// <summary>
        /// Retrieve all budgets for this project, including archived ones.
        /// </summary>
        /// <param name="connection">The connection to use for the API call.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A list of all budgets.</returns>
        public async Task<List<Budget>> RetrieveAllBudgetsAsync(Connection connection, CancellationToken ct = default)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            return await connection.CreditsClient.GetBudgetsAsync(Uuid, activeOnly: false, ct);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[QProject Uuid={Uuid}, Name={Name}, Slug={Slug}]";
        }
    }
}

using System;

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

        internal QProject(ProjectApi projectApi)
        {
            _projectApi = projectApi;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"[QProject Uuid={Uuid}, Name={Name}, Slug={Slug}]";
        }
    }
}

using System;

namespace QarnotSDK
{
    internal class ProjectApi
    {
        public Guid Uuid { get; set; }
        public string Name { get; set; }
        public Guid OrganizationUuid { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }

        internal ProjectApi() { }
    }
}

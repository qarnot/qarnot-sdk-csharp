namespace QarnotSDK.UnitTests
{
    public static class ProjectTestData
    {
        public const string ProjectUuid        = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
        public const string ProjectName        = "my-project";
        public const string ProjectSlug        = "my-project-slug";
        public const string ProjectDescription = "A test project";
        public const string OrganizationUuid   = "11111111-2222-3333-4444-555555555555";

        public const string ProjectJson = @"{
            ""uuid"": """ + ProjectUuid + @""",
            ""name"": """ + ProjectName + @""",
            ""organizationUuid"": """ + OrganizationUuid + @""",
            ""description"": """ + ProjectDescription + @""",
            ""slug"": """ + ProjectSlug + @"""
        }";

        public const string UserInfoWithProjectsJson = @"{
            ""email"": ""mail@mail.com"",
            ""maxBucket"": 10,
            ""maxTask"": 10,
            ""taskCount"": 0,
            ""maxPool"": 10,
            ""poolCount"": 0,
            ""maxRunningTask"": 10,
            ""maxRunningPool"": 10,
            ""runningTaskCount"": 0,
            ""runningPoolCount"": 0,
            ""projects"": [
                {
                    ""uuid"": """ + ProjectUuid + @""",
                    ""name"": """ + ProjectName + @""",
                    ""organizationUuid"": """ + OrganizationUuid + @""",
                    ""description"": """ + ProjectDescription + @""",
                    ""slug"": """ + ProjectSlug + @"""
                },
                {
                    ""uuid"": ""bbbbbbbb-cccc-dddd-eeee-ffffffffffff"",
                    ""name"": ""other-project"",
                    ""organizationUuid"": """ + OrganizationUuid + @""",
                    ""description"": ""Another project"",
                    ""slug"": ""other-project-slug""
                }
            ]
        }";

        public const string TaskWithProjectUuidJson = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""task-with-project"",
            ""profile"": ""docker-batch"",
            ""projectUuid"": """ + ProjectUuid + @"""
        }";

        public const string PoolWithProjectUuidJson = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""pool-with-project"",
            ""profile"": ""docker-batch"",
            ""projectUuid"": """ + ProjectUuid + @"""
        }";
    }
}
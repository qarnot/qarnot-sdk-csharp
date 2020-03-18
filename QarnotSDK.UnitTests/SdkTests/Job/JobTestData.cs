namespace QarnotSDK.UnitTests
{
    public static class JobTestsData
    {
        public const string JobResponseBody = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""job_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-batch"",
            ""state"": ""Success"",
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
        }";

        public const string JobSwitchUuid = "aaafdff8-7081-46e1-bb2f-d9cd4e185ece";
        public const string JobResponseUuid = "f78fdff8-7081-46e1-bb2f-d9cd4e185ece";
        public const string JobResponseName = "job_name";
        public const string GetJobsBody = @"[]";
    }
}

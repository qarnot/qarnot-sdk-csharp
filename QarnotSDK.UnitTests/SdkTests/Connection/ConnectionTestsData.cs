namespace QarnotSDK.UnitTests
{
    public static class ConnectionTestsData
    {
        public const string GetDefaultBodyUuid = "f78fdff8-7081-46e1-bb2f-d9cd4e185ece";

        public const string GetEmptyPage = @"{
            ""Data"": [],
            ""Token"": null,
            ""NextToken"": null,
            ""IsTruncated"": false
            }
";

        public const string GetOneJobPage = @"{
            ""Data"": [" + GetJobBody + @"],
            ""Token"": ""token"",
            ""NextToken"": ""next_token"",
            ""IsTruncated"": true
            }
";

        public const string GetOneTaskPage = @"{
            ""Data"": [" + GetTaskBody + @"],
            ""Token"": ""token"",
            ""NextToken"": ""next_token"",
            ""IsTruncated"": true
            }
";

        public const string GetOnePoolPage = @"{
            ""Data"": [" + GetPoolBody + @"],
            ""Token"": ""token"",
            ""NextToken"": ""next_token"",
            ""IsTruncated"": true
            }
";

        public const string GetDefaultBodyName = "default_name";

        public const string GetDefaultBodyProfile = "docker-bash";

        public const string GetUserInformation = @"{
            ""email"": ""mail@mail.com"",
            ""maxBucket"": 42,
            ""bucketCount"": 1,
            ""quotaBytesBucket"": 2,
            ""usedQuotaBytesBucket"": 3,
            ""maxTask"": 4,
            ""taskCount"": 5,
            ""maxRunningTask"": 6,
            ""runningTaskCount"": 7,
            ""maxInstances"": 8,
            ""maxPool"": 9,
            ""poolCount"": 10,
            ""maxRunningPool"": 11,
            ""RunningInstanceCount"": 13,
            ""RunningCoreCount"": 14,
            ""runningPoolCount"": 12
        }";

        public const string GetUserHardwareConstraints = @"{
            ""data"":
            [
                {
                    ""discriminator"": ""MinimumCoreHardwareConstraint"",
                    ""coreCount"": 16
                },
                {
                    ""discriminator"": ""MaximumCoreHardwareConstraint"",
                    ""coreCount"": 32
                },
                {
                    ""discriminator"": ""MinimumRamCoreRatioHardwareConstraint"",
                    ""minimumMemoryGBCoreRatio"": 0.4
                },
                {
                    ""discriminator"": ""MaximumRamCoreRatioHardwareConstraint"",
                    ""maximumMemoryGBCoreRatio"": 0.7
                },
                {
                    ""discriminator"": ""SpecificHardwareConstraint"",
                    ""specificationKey"": ""R7-2700X""
                },
                {
                    ""discriminator"": ""MinimumRamHardwareConstraint"",
                    ""minimumMemoryMB"": 4000
                },
                {
                    ""discriminator"": ""MaximumRamHardwareConstraint"",
                    ""maximumMemoryMB"": 32000
                },
                {
                    ""discriminator"": ""GpuHardwareConstraint""
                },
                {
                    ""discriminator"": ""NoGpuHardwareConstraint""
                },
                {
                    ""discriminator"": ""SSDHardwareConstraint""
                },
                {
                    ""discriminator"": ""NoSSDHardwareConstraint""
                },
            ],
            ""offset"": 0,
            ""limit"": 50,
            ""total"": 10
        }";

        public const string GetLimitedUserHardwareConstraints = @"{
            ""data"":
            [
                {
                    ""discriminator"": ""MinimumCoreHardwareConstraint"",
                    ""coreCount"": 16
                },
                {
                    ""discriminator"": ""MaximumCoreHardwareConstraint"",
                    ""coreCount"": 32
                }
            ],
            ""offset"": 0,
            ""limit"": 2,
            ""total"": 8
        }";

        public const string GetJobBody = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
        }";

        public const string GetJobsBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
        }]";

        public const string GetJobsPaginateBody = @"{
        ""token"":null,
        ""nextToken"":""ABABABABABA0BAB"",
        ""isTruncated"":false,
        ""data"":" + GetJobsBody + @"
        }";

        public const string GetJobsDetailBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
        }]";

        public const string GetPaginateJobsByTagsBody = @"{
        ""nextToken"":""TestToken"",
        ""isTruncated"":false,
        ""data"":[{
            ""name"": ""default_name"",
            ""tags"":[""tag1""],
        },{
            ""name"": ""Second_default_name"",
        }]}";

        public const string SubmitTasksBody = @"[{""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece""},{""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ecf""},]";

        public const string SubmitTaskBody = "{}";

        public const string GetTasksByTagsBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetTasksBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetTasksPaginateBody = @"{
        ""token"":null,
        ""nextToken"":""ABABABABABA0BAB"",
        ""isTruncated"":false,
        ""data"":" + GetTasksBody + @"
        }";

        public const string GetTasksByTagsPaginateBody = @"{
        ""token"":null,
        ""nextToken"":""ABABABABABA0BAB"",
        ""isTruncated"":false,
        ""data"":" + GetTasksByTagsBody + @"
        }";

        public const string GetTasksSearchBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetTaskSummaryBody = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        }";

        public const string GetTaskBody = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
            }";

        public const string GetTasksSummaryBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetTasksSummaryPaginateBody = @"{
        ""token"":null,
        ""nextToken"":""ABABABABABA0BAB"",
        ""isTruncated"":false,
        ""data"":" + GetTasksSummaryBody + @"
        }";

        public const string GetProfileConstants = @"{
            ""name"": ""MyName"",
            ""constants"": [{""name"":""constant1"",""value"":""value1"",""description"":""description1""}, {""name"":""constant2"",""value"":""value2"",""description"":""description2""}]
        }";

        public const string GetProfiles = @"[""docker-bash""]";

        public const string GetPoolsByTagsBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
            ""tags"":[""tag1""],
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetPoolsSummaryByTagsBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
        }]";

        public const string GetPoolSummaryBody = @"{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        }";

        public const string GetPoolsSummaryBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetPoolsBody = @"[{
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""profile"": ""docker-bash"",
        },{
            ""uuid"": ""078fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""Second_default_name"",
            ""profile"": ""docker-bash2"",
        }]";

        public const string GetPoolsPaginateBody = @"{
            ""token"":null,
            ""nextToken"":""ABABABABABA0BAB"",
            ""isTruncated"":false,
            ""data"":" + GetPoolsBody + @"
        }";

        public const string GetPoolsSummaryPaginateBody = @"{
            ""token"":null,
            ""nextToken"":""ABABABABABA0BAB"",
            ""isTruncated"":false,
            ""data"":" + GetPoolsSummaryBody + @"
        }";

        public const string GetPoolsSummaryByTagsPaginateBody = @"{
            ""token"":null,
            ""nextToken"":""ABABABABABA0BAB"",
            ""isTruncated"":false,
            ""data"":" + GetPoolsSummaryByTagsBody + @"
        }";

        public const string GetPoolsByTagsPaginateBody = @"{
            ""token"":null,
            ""nextToken"":""ABABABABABA0BAB"",
            ""isTruncated"":false,
            ""data"":" + GetPoolsByTagsBody + @"
        }";

        public const string GetPoolBody = @"{
            ""elasticProperty"": {
                ""isElastic"": false,
                ""minTotalSlots"": 0,
                ""maxTotalSlots"": 0,
                ""minIdleSlots"": 0,
                ""resizePeriod"": 90,
                ""rampResizeFactor"": 0.4,
                ""minIdleTimeSeconds"": 90
            },
            ""constants"": [],
            ""tags"": [],
            ""errors"": [],
            ""resourceBuckets"": [],
            ""status"": {
                ""timestamp"": ""0001-01-01T00:00:00Z"",
                ""lastUpdateTimestamp"": ""0001-01-01T00:00:00Z"",
                ""downloadProgress"": 100,
                ""executionProgress"": 0,
                ""uploadProgress"": 0,
                ""instanceCount"": 3,
                ""downloadTime"": ""00:00:00"",
                ""downloadTimeSec"": 0,
                ""environmentTime"": ""00:00:00"",
                ""environmentTimeSec"": 0,
                ""executionTime"": ""00:00:25.1000003"",
                ""executionTimeSec"": 25.100000299999998,
                ""executionTimeByCpuModel"": [
                    {
                        ""model"": ""AMD Ryzen 7 1700X Eight-Core Processor"",
                        ""time"": 25.1,
                        ""core"": 8
                    }
                ],
                ""executionTimeGhzByCpuModel"": [
                    {
                        ""model"": ""AMD Ryzen 7 1700X Eight-Core Processor"",
                        ""timeGhz"": 68.14348,
                        ""core"": 8,
                        ""clockRatio"": 1
                    }
                ],
                ""uploadTime"": ""00:00:00"",
                ""uploadTimeSec"": 0,
                ""wallTime"": ""00:02:12"",
                ""wallTimeSec"": 132,
                ""succeededRange"": """",
                ""executedRange"": """",
                ""failedRange"": """",
                ""startedOnceRange"": ""0-2"",
                ""runningInstancesInfo"": {
                    ""perRunningInstanceInfo"": [
                        {
                            ""phase"": ""execution"",
                            ""instanceId"": 0,
                            ""maxFrequencyGHz"": 3.4,
                            ""currentFrequencyGHz"": 3.4,
                            ""cpuUsage"": 11.25,
                            ""maxMemoryMB"": 314,
                            ""currentMemoryMB"": 314,
                            ""networkInKbps"": 1476,
                            ""networkOutKbps"": 91,
                            ""progress"": 0,
                            ""executionTimeSec"": 129,
                            ""executionTimeGHz"": 341.31247,
                            ""cpuModel"": ""AMD Ryzen 7 1700X Eight-Core Processor"",
                            ""coreCount"": 8,
                            ""activeForwards"": [{""applicationPort"":42, ""forwarderHost"":""host23"", ""forwarderPort"":23}],
                            ""memoryUsage"": 1,
                            ""clockRatio"": 1
                        },
                        {
                            ""phase"": ""execution"",
                            ""instanceId"": 1,
                            ""maxFrequencyGHz"": 3.4,
                            ""currentFrequencyGHz"": 3.4,
                            ""cpuUsage"": 8.75,
                            ""maxMemoryMB"": 305,
                            ""currentMemoryMB"": 304,
                            ""networkInKbps"": 1463,
                            ""networkOutKbps"": 100,
                            ""progress"": 0,
                            ""executionTimeSec"": 122,
                            ""executionTimeGHz"": 340.12228,
                            ""cpuModel"": ""AMD Ryzen 7 1700X Eight-Core Processor"",
                            ""coreCount"": 8,
                            ""activeForwards"": [],
                            ""memoryUsage"": 0.9967213,
                            ""clockRatio"": 1
                        },
                        {
                            ""phase"": ""execution"",
                            ""instanceId"": 2,
                            ""maxFrequencyGHz"": 0,
                            ""currentFrequencyGHz"": 0,
                            ""cpuUsage"": 0,
                            ""maxMemoryMB"": 0,
                            ""currentMemoryMB"": 0,
                            ""networkInKbps"": 0,
                            ""networkOutKbps"": 0,
                            ""progress"": 0,
                            ""executionTimeSec"": 0,
                            ""executionTimeGHz"": 0,
                            ""cpuModel"": """",
                            ""coreCount"": 0,
                            ""activeForwards"": [],
                            ""memoryUsage"": 0,
                            ""clockRatio"": 0
                        }
                    ],
                    ""snapshotResults"": [],
                    ""timestamp"": ""0001-01-01T00:00:00Z"",
                    ""averageFrequencyGHz"": 2.2666667,
                    ""maxFrequencyGHz"": 3.4,
                    ""minFrequencyGHz"": 0,
                    ""averageMaxFrequencyGHz"": 2.2666667,
                    ""averageCpuUsage"": 6.6666665,
                    ""clusterPowerIndicator"": 1,
                    ""averageMemoryUsage"": 0.6655738,
                    ""averageNetworkInKbps"": 979.6667,
                    ""averageNetworkOutKbps"": 63.666668,
                    ""totalNetworkInKbps"": 2939,
                    ""totalNetworkOutKbps"": 191,
                    ""runningCoreCountByCpuModel"": [
                        {
                            ""model"": ""AMD Ryzen 7 1700X Eight-Core Processor"",
                            ""core"": 8,
                            ""runningCoreCount"": 16
                        }
                    ]
                }
            },
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""default_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-bash"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01
        }";
    }
}

namespace QarnotSDK.UnitTests
{
    public static class PoolTestsData
    {
        public const string PoolSwitchUuid = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";

        public const string PoolResponseUuid = "f78fdff8-7081-46e1-bb2f-d9cd4e185ece";

        public const string PoolResponseName = "pool_name";

        public const string PoolsListUuid = "796a5321-0001-4a5c-2f42-54cce169dff8,796a5321-0002-4a5c-2f42-54cce169dff8,796a5321-0003-4a5c-2f42-54cce169dff8";

        public const string PoolResponseBody = @"{
            ""elasticProperty"": {},
            ""constants"": [],
            ""tags"": [],
            ""errors"": [],
            ""resourceBuckets"": [],
            ""status"": {
            },
            ""uuid"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""name"": ""pool_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece-shortname"",
            ""profile"": ""docker-batch"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01,
            ""poolUsage"": 0.91,
            ""totalSlotCapacity"": 5,
            ""queuedOrRunningTaskInstancesCount"": 71
        }";

        public const string PoolsListBodies = @"[{
            ""elasticProperty"": {},
            ""constants"": [],
            ""constraints"": [],
            ""tags"": [],
            ""errors"": [],
            ""resourceBuckets"": [],
            ""status"": {},
            ""uuid"": ""796a5321-0001-4a5c-2f42-54cce169dff8"",
            ""name"": ""AddConstTagHandler-Name1"",
            ""shortname"": ""AddConstTagHandler-shortname1"",
            ""profile"": ""AddConstTagHandler-Profile"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01
        },{
            ""elasticProperty"": {},
            ""constants"": [],
            ""constraints"": [],
            ""tags"": [],
            ""errors"": [],
            ""resourceBuckets"": [],
            ""status"": {},
            ""uuid"": ""796a5321-0002-4a5c-2f42-54cce169dff8"",
            ""name"": ""AddConstTagHandler-Name2"",
            ""shortname"": ""AddConstTagHandler-shortname2"",
            ""profile"": ""AddConstTagHandler-Profile"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01
        },{
            ""elasticProperty"": {},
            ""constants"": [],
            ""constraints"": [],
            ""tags"": [],
            ""errors"": [],
            ""resourceBuckets"": [],
            ""status"": {},
            ""uuid"": ""796a5321-0003-4a5c-2f42-54cce169dff8"",
            ""name"": ""AddConstTagHandler-Name3"",
            ""shortname"": ""AddConstTagHandler-shortname3"",
            ""profile"": ""AddConstTagHandler-Profile"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01
        }]";

        public const string PoolConstTagUuidHandler = @"796a5321-ffff-4a5c-2f42-54cce169dff8";

        public const string PoolConstTagHandler = @"{
            ""elasticProperty"": {
            },
            ""constants"": [
                {
                    ""key"": ""constantskey1"",
                    ""value"": ""constantsvalue1"",
                },
                {
                    ""key"": ""constantskey2"",
                    ""value"": ""constantsvalue2"",
                },
                {
                    ""key"": ""constantskey3"",
                    ""value"": ""constantsvalue3"",
                },
            ],
            ""constraints"": [
                {
                    ""key"": ""constraintskey1"",
                    ""value"": ""constraintsvalue1"",
                },
                {
                    ""key"": ""constraintskey2"",
                    ""value"": ""constraintsvalue2"",
                },
                {
                    ""key"": ""constraintskey3"",
                    ""value"": ""constraintsvalue3"",
                },
            ],
            ""tags"": [""tag1"", ""tag2"", ""tag3""],
            ""errors"": [],
            ""resourceBuckets"": [],
            ""status"": {
            },
            ""uuid"": ""796a5321-ffff-4a5c-2f42-54cce169dff8"",
            ""name"": ""AddConstTagHandler-Name"",
            ""shortname"": ""AddConstTagHandler-shortname"",
            ""profile"": ""AddConstTagHandler-Profile"",
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

        public const string PoolResponseFullBody = @"{
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
            ""name"": ""pool_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-batch"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01,
            ""poolUsage"": 0.91,
            ""totalSlotCapacity"": 5,
            ""queuedOrRunningTaskInstancesCount"": 71,
            ""privileges"": {
                ""exportApiAndStorageCredentialsInEnvironment"": false
            },
            ""defaultResourcesCacheTTLSec"": 7776000,
            ""defaultRetrySettings"": {
                ""maxTotalRetries"": 12,
                ""maxPerInstanceRetries"": 12
            },
            ""schedulingType"": ""Reserved"",
            ""targetedReservedMachineKey"": ""some-reserved-machine"",
            ""forcedConstants"": [
                {
                    ""constantName"": ""the-name-1"",
                    ""forcedValue"": ""the-value-1"",
                    ""forceExportInEnvironment"": true,
                },
                {
                    ""constantName"": ""the-name-2"",
                    ""forcedValue"": ""the-value-2"",
                    ""access"": ""ReadOnly""
                },
            ],
            ""forcedNetworkRules"": [
                {
                    ""Inbound"": true,
                    ""Proto"": ""tcp"",
                    ""To"": ""bound-to-be-alive"",
                    ""Port"": ""1234"",
                    ""Priority"": ""1000"",
                    ""Description"": ""Inbound test"",
                },
                {
                    ""Inbound"": false,
                    ""Proto"": ""tcp"",
                    ""PublicHost"": ""bound-to-the-devil"",
                    ""PublicPort"": ""666"",
                    ""Priority"": ""1000"",
                    ""Description"": ""Outbound test"",
                }
            ],
        }";


        public const string PoolResponseWithAdvancedBucketsFullBody = @"{
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
            ""advancedResourceBuckets"": [
                {
                    ""bucketName"": ""someBucket"",
                    ""filtering"": {
                        ""prefixFiltering"": {
                            ""prefix"": ""some/prefix/""
                        }
                    },
                    ""resourcesTransformation"": {
                        ""stripPrefix"": {
                            ""prefix"": ""transformed-prefix/""
                        },
                    },
                    ""cacheTTLSec"": 1000
                },
                {
                    ""bucketName"": ""someOtherBucket"",
                    ""filtering"": null,
                    ""resourcesTransformation"": null
                }
            ],
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
            ""name"": ""pool_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-batch"",
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


        public const string PoolResponseWithLegacyBucketsFullBody = @"{
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
            ""resourceBuckets"": [""someBucket"", ""someOtherBucket""],
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
            ""name"": ""pool_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-batch"",
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


        public static string PoolResponse_WithTaskDefaultWaitForPoolResourcesSynchronization(bool? wait) => @"{
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
            ""advancedResourceBuckets"": [
                {
                    ""bucketName"": ""someBucket"",
                    ""filtering"": {
                        ""prefixFiltering"": {
                            ""prefix"": ""some/prefix/""
                        }
                    },
                    ""resourcesTransformation"": {
                        ""stripPrefix"": {
                            ""prefix"": ""transformed-prefix/""
                        },
                    }
                },
                {
                    ""bucketName"": ""someOtherBucket"",
                    ""filtering"": null,
                    ""resourcesTransformation"": null
                }
            ],
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
            ""name"": ""pool_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-batch"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01,"
                +
            $"\"taskDefaultWaitForPoolResourcesSynchronization\": {(wait == null ? "null" : wait.Value ? "true" : "false")}"
                +
        "}";



        public const string PoolResponseWithScalingFullBody = @"{
           ""elasticProperty"": {
                ""isElastic"": false,
                ""minTotalSlots"": 0,
                ""maxTotalSlots"": 0,
                ""minIdleSlots"": 0,
                ""resizePeriod"": 90,
                ""rampResizeFactor"": 0.4,
                ""minIdleTimeSeconds"": 90
            },
            ""scaling"": {
                ""activePolicyName"": ""managed-policy"",
                ""policies"": [
                    {
                        ""type"": ""ManagedTasksQueue"",
                        ""name"": ""managed-policy"",
                        ""enabledPeriods"": [
                            {
                                ""type"": ""Weekly"",
                                ""name"": ""thursday-evening"",
                                ""days"": [ ""Thursday"" ],
                                ""startTimeUtc"": ""19:30:00"",
                                ""endTimeUtc"": ""22:00:00""
                            },
                            {
                                ""type"": ""Weekly"",
                                ""name"": ""wednesdays"",
                                ""days"": [""Wednesday""],
                                ""startTimeUtc"": ""00:00:00"",
                                ""endTimeUtc"": ""23:59:59.9999999""
                            }
                        ],
                        ""minTotalSlots"": 0,
                        ""maxTotalSlots"": 10,
                        ""minIdleSlots"": 1,
                        ""minIdleTimeSeconds"": 90,
                        ""scalingFactor"": 0.5
                    },
                    {
                        ""type"": ""Fixed"",
                        ""name"": ""fixed-policy"",
                        ""enabledPeriods"": [
                            {
                                ""type"": ""Always"",
                                ""name"": ""really-always""
                            }
                        ],
                        ""slotsCount"": 4
                    }
                ]
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
            ""name"": ""pool_name"",
            ""shortname"": ""f78fdff8-7081-46e1-bb2f-d9cd4e185ece"",
            ""profile"": ""docker-batch"",
            ""state"": ""Success"",
            ""instanceCount"": 3,
            ""creationDate"": ""2019-11-08T10:54:11Z"",
            ""endDate"": ""0001-01-01T00:00:00Z"",
            ""runningInstanceCount"": 3,
            ""runningCoreCount"": 16,
            ""executionTime"": ""00:00:25.1000003"",
            ""wallTime"": ""00:02:12"",
            ""credits"": 0.01,
            ""poolUsage"": 0.91,
            ""totalSlotCapacity"": 5,
            ""queuedOrRunningTaskInstancesCount"": 71,
            ""privileges"": {
                ""exportApiAndStorageCredentialsInEnvironment"": false
            },
            ""defaultResourcesCacheTTLSec"": 7776000,
            ""defaultRetrySettings"": {
                ""maxTotalRetries"": 12,
                ""maxPerInstanceRetries"": 12
            },
        }";


    }
}

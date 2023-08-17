namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class UnitTestConnection
    {
        private const string StorageUrl = "http://storage";
        private const string ComputeUrl = "http://compute";
        private const string Token = "token";
        private Connection Api;

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler();
            Api = new Connection(ComputeUrl, StorageUrl, Token, HttpHandler);
        }

        [TearDown]
        public void TearDown()
        {
            HttpHandler?.Dispose();
        }

        [Test]
        public async Task SubmitTasksAsyncShouldPostOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.SubmitTasksBody;

            List<QTask> tasks = new List<QTask>();
            await Api.SubmitTasksAsync(tasks);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummaryByShortnameAsyncShouldGetOnCorrectEndpoint()
        {
            string poolShortname = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByShortnameAsync(poolShortname);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/{poolShortname}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummaryByShortnameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            string poolShortname = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByShortnameAsync(poolShortname);
            Assert.AreEqual(poolSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(poolSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(poolSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolSummaryByUuidAsyncShouldGetOnCorrectEndpoint()
        {
            string poolId = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByUuidAsync(poolId);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/{poolId}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummaryByUuidAsyncCheckTheReturnBodyUuidNameProfile()
        {
            string poolId = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByUuidAsync(poolId);
            Assert.AreEqual(poolSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(poolSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(poolSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolByShortnameAsyncShouldGetOnCorrectEndpoint()
        {
            string poolShortname = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByShortnameAsync(poolShortname);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/{poolShortname}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolByShortnameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            string poolShortname = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByShortnameAsync(poolShortname);

            Assert.AreEqual(poolSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(poolSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(poolSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolByUuidAsyncShouldGetOnCorrectEndpoint()
        {
            string poolId = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByUuidAsync(poolId);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/{poolId}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolByUuidAsyncCheckTheReturnBodyUuidNameProfile()
        {
            string poolId = Guid.NewGuid().ToString();

            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolBody;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByUuidAsync(poolId);

            Assert.AreEqual(poolSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(poolSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(poolSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolSummaryByNameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsPaginateBody;

            string name = "mypoolname";
            await Api.RetrievePoolSummaryByNameAsync(name);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/summaries", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummaryByNameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsPaginateBody;

            string name = ConnectionTestsData.GetDefaultBodyName;
            QPoolSummary poolSummary = await Api.RetrievePoolSummaryByNameAsync(name);

            Assert.AreEqual(poolSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(poolSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(poolSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolsByNameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsBody;

            string name = ConnectionTestsData.GetDefaultBodyName;
            await Api.RetrievePoolsByNameAsync(name);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/search", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolsByNameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsBody;

            string name = ConnectionTestsData.GetDefaultBodyName;
            QPool pool = (await Api.RetrievePoolsByNameAsync(name))[0];

            Assert.AreEqual(pool.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(pool.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(pool.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolByNameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsPaginateBody;

            string name = ConnectionTestsData.GetDefaultBodyName;
            await Api.RetrievePoolByNameAsync(name);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolByNameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsPaginateBody;

            string name = ConnectionTestsData.GetDefaultBodyName;
            QPool pool = await Api.RetrievePoolByNameAsync(name);

            Assert.AreEqual(pool.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(pool.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(pool.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveJobByShortnameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobBody;

            string jobShortname = Guid.NewGuid().ToString();
            QJob job = await Api.RetrieveJobByShortnameAsync(jobShortname);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/jobs/{jobShortname}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobByShortnameAsyncCheckTheReturnBodyUuidName()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobBody;

            QJob job = await Api.RetrieveJobByShortnameAsync(ConnectionTestsData.GetDefaultBodyUuid);

            Assert.AreEqual(job.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(job.Name, ConnectionTestsData.GetDefaultBodyName);
        }

        [Test]
        public async Task RetrieveJobByUuidAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobBody;

            string jobUuid = Guid.NewGuid().ToString();
            QJob job = await Api.RetrieveJobByUuidAsync(jobUuid);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/jobs/{jobUuid}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobByUuidAsyncCheckTheReturnBodyUuidName()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobBody;

            QJob job = await Api.RetrieveJobByUuidAsync(ConnectionTestsData.GetDefaultBodyUuid);

            Assert.AreEqual(job.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(job.Name, ConnectionTestsData.GetDefaultBodyName);
        }

        [Test]
        public async Task RetrieveTaskSummaryByShortnameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskSummaryBody;

            string taskShortname = Guid.NewGuid().ToString();
            QTaskSummary taskSummary = await Api.RetrieveTaskSummaryByShortnameAsync(taskShortname);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/{taskShortname}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskSummaryByShortnameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskSummaryBody;

            string taskShortname = Guid.NewGuid().ToString();
            QTaskSummary taskSummary = await Api.RetrieveTaskSummaryByShortnameAsync(taskShortname);

            Assert.AreEqual(taskSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(taskSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(taskSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskSummaryByUuidAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskSummaryBody;

            string taskUuid = Guid.NewGuid().ToString();
            QTaskSummary taskSummary = await Api.RetrieveTaskSummaryByUuidAsync(taskUuid);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/{taskUuid}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskSummaryByUuidAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskSummaryBody;

            string taskUuid = Guid.NewGuid().ToString();
            QTaskSummary taskSummary = await Api.RetrieveTaskSummaryByUuidAsync(taskUuid);

            Assert.AreEqual(taskSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(taskSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(taskSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskByShortnameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskBody;

            string taskShortname = Guid.NewGuid().ToString();
            QTask task = await Api.RetrieveTaskByShortnameAsync(taskShortname);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/{taskShortname}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskByShortnameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskBody;

            string taskShortname = ConnectionTestsData.GetDefaultBodyUuid;
            QTask task = await Api.RetrieveTaskByShortnameAsync(taskShortname);

            Assert.AreEqual(task.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(task.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(task.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskByUuidAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskBody;

            string taskUuid = Guid.NewGuid().ToString();
            QTask task = await Api.RetrieveTaskByUuidAsync(taskUuid);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/{taskUuid}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskByUuidAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTaskBody;

            string taskUuid = ConnectionTestsData.GetDefaultBodyUuid;
            QTask task = await Api.RetrieveTaskByUuidAsync(taskUuid);

            Assert.AreEqual(task.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(task.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(task.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskSummaryByNameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksSummaryPaginateBody;

            string taskName = "mytaskname";
            QTaskSummary taskSummary = await Api.RetrieveTaskSummaryByNameAsync(taskName);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/summaries/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskSummaryByNameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksSummaryPaginateBody;

            string taskName = ConnectionTestsData.GetDefaultBodyName;
            QTaskSummary taskSummary = await Api.RetrieveTaskSummaryByNameAsync(taskName);

            Assert.AreEqual(taskSummary.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(taskSummary.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(taskSummary.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTasksByNameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksBody;

            string taskName = "mytaskname";
            QTask task = (await Api.RetrieveTasksByNameAsync(taskName))[0];

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/search", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksByNameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksBody;

            string taskName = ConnectionTestsData.GetDefaultBodyName;
            List<QTask> taskList = await Api.RetrieveTasksByNameAsync(taskName);
            QTask task = taskList[0];

            Assert.AreEqual(task.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(task.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(task.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskByNameAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksPaginateBody;

            string taskName = "mytaskname";
            QTask task = await Api.RetrieveTaskByNameAsync(taskName);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskByNameAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksPaginateBody;

            string taskName = ConnectionTestsData.GetDefaultBodyName;
            QTask task = await Api.RetrieveTaskByNameAsync(taskName);

            Assert.AreEqual(task.Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(task.Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(task.Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveConstantsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetProfileConstants;
            string profile = "myprofile";

            List<Constant> constants = await Api.RetrieveConstantsAsync(profile);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/profiles/{profile}", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveConstantsAsyncCheckTheReturnConstants()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetProfileConstants;
            string profile = "myprofile";

            List<Constant> constants = await Api.RetrieveConstantsAsync(profile);
            Assert.AreEqual(constants[0].Name, "constant1");
            Assert.AreEqual(constants[1].Name, "constant2");
            Assert.AreEqual(constants[0].Value, "value1");
            Assert.AreEqual(constants[1].Value, "value2");
            Assert.AreEqual(constants[0].Description, "description1");
            Assert.AreEqual(constants[1].Description, "description2");
        }

        [Test]
        public async Task RetrieveProfilesAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetProfiles;

            List<string> profiles = await Api.RetrieveProfilesAsync();

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/profiles", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveProfilesAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetProfiles;

            List<string> profiles = await Api.RetrieveProfilesAsync();
            Assert.AreEqual(profiles[0], ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveUserInformationAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetUserInformation;

            UserInformation userInfo = await Api.RetrieveUserInformationAsync(false);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/info", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveUserInformationAsyncCheckRetrunBodyInfos()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetUserInformation;

            UserInformation userInfo = await Api.RetrieveUserInformationAsync(false);

            Assert.AreEqual(Api.StorageAccessKey, "mail@mail.com");
            Assert.AreEqual(userInfo.Email, "mail@mail.com");
            Assert.AreEqual(userInfo.MaxBucket, 42);
            Assert.AreEqual(userInfo.BucketCount, 1);
            Assert.AreEqual(userInfo.QuotaBytesBucket, 2);
            Assert.AreEqual(userInfo.UsedQuotaBytesBucket, 3);
            Assert.AreEqual(userInfo.MaxTask, 4);
            Assert.AreEqual(userInfo.TaskCount, 5);
            Assert.AreEqual(userInfo.MaxRunningTask, 6);
            Assert.AreEqual(userInfo.RunningTaskCount, 7);
            Assert.AreEqual(userInfo.MaxInstances, 8);
            Assert.AreEqual(userInfo.MaxPool, 9);
            Assert.AreEqual(userInfo.PoolCount, 10);
            Assert.AreEqual(userInfo.MaxRunningPool, 11);
            Assert.AreEqual(userInfo.RunningPoolCount, 12);
            Assert.AreEqual(userInfo.RunningInstanceCount, 13);
            Assert.AreEqual(userInfo.RunningCoreCount, 14);
        }

        [Test]
        public async Task RetrieveUserHardwareConstraintsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetUserHardwareConstraints;

            OffsetPageResponse<HardwareConstraint> hardwareConstraints = await Api.RetrieveUserHardwareConstraintsPageAsync(new());

            var hardwareConstraintsRequests = HttpHandler.ParsedRequests.Where(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/hardware-constraints", StringComparison.InvariantCultureIgnoreCase));
            Assert.True(hardwareConstraintsRequests.Any());

            var requestsCountBefore = hardwareConstraintsRequests.Count();

            var constraints = await Api.RetrieveUserHardwareConstraintsAsync();

            hardwareConstraintsRequests = HttpHandler.ParsedRequests.Where(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/hardware-constraints", StringComparison.InvariantCultureIgnoreCase));

            Assert.Greater(hardwareConstraintsRequests.Count(), requestsCountBefore);
        }

        [Test]
        public async Task RetrieveUserHardwareConstraintsAsyncCheckReturnBodyInfos()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetUserHardwareConstraints;

            OffsetPageResponse<HardwareConstraint> paginatedHardwareConstraints = await Api.RetrieveUserHardwareConstraintsPageAsync(new());

            Assert.AreEqual(10, paginatedHardwareConstraints.Total);
            var hardwareConstraints = paginatedHardwareConstraints.Data;
            Assert.IsInstanceOf<List<HardwareConstraint>>(hardwareConstraints);
            Assert.IsInstanceOf<MinimumCoreHardware>(hardwareConstraints[0]);
            Assert.AreEqual((hardwareConstraints[0] as MinimumCoreHardware).Discriminator, "MinimumCoreHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[0] as MinimumCoreHardware).CoreCount, 16);
            Assert.IsInstanceOf<MaximumCoreHardware>(hardwareConstraints[1]);
            Assert.AreEqual((hardwareConstraints[1] as MaximumCoreHardware).Discriminator, "MaximumCoreHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[1] as MaximumCoreHardware).CoreCount, 32);
            Assert.IsInstanceOf<MinimumRamCoreRatioHardware>(hardwareConstraints[2]);
            Assert.AreEqual((hardwareConstraints[2] as MinimumRamCoreRatioHardware).Discriminator, "MinimumRamCoreRatioHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[2] as MinimumRamCoreRatioHardware).MinimumMemoryGBCoreRatio, 0.4);
            Assert.IsInstanceOf<MaximumRamCoreRatioHardware>(hardwareConstraints[3]);
            Assert.AreEqual((hardwareConstraints[3] as MaximumRamCoreRatioHardware).Discriminator, "MaximumRamCoreRatioHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[3] as MaximumRamCoreRatioHardware).MaximumMemoryGBCoreRatio, 0.7);
            Assert.IsInstanceOf<SpecificHardware>(hardwareConstraints[4]);
            Assert.AreEqual((hardwareConstraints[4] as SpecificHardware).Discriminator, "SpecificHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[4] as SpecificHardware).SpecificationKey, "R7-2700X");
            Assert.IsInstanceOf<MinimumRamHardware>(hardwareConstraints[5]);
            Assert.AreEqual((hardwareConstraints[5] as MinimumRamHardware).Discriminator, "MinimumRamHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[5] as MinimumRamHardware).MinimumMemoryMB, 4000);
            Assert.IsInstanceOf<MaximumRamHardware>(hardwareConstraints[6]);
            Assert.AreEqual((hardwareConstraints[6] as MaximumRamHardware).Discriminator, "MaximumRamHardwareConstraint");
            Assert.AreEqual((hardwareConstraints[6] as MaximumRamHardware).MaximumMemoryMB, 32000);
            Assert.IsInstanceOf<GpuHardware>(hardwareConstraints[7]);
            Assert.AreEqual((hardwareConstraints[7] as GpuHardware).Discriminator, "GpuHardwareConstraint");
            Assert.IsInstanceOf<SSDHardware>(hardwareConstraints[8]);
            Assert.AreEqual((hardwareConstraints[8] as SSDHardware).Discriminator, "SSDHardwareConstraint");
            Assert.IsInstanceOf<NoSSDHardware>(hardwareConstraints[9]);
            Assert.AreEqual((hardwareConstraints[9] as NoSSDHardware).Discriminator, "NoSSDHardwareConstraint");
        }

        [Test]
        public async Task RetrieveJobsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobsPaginateBody;

            List<QJob> jobs = await Api.RetrieveJobsAsync();

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/jobs/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobsAsynCheckTheReturnBodyUuidName()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobsPaginateBody;

            List<QJob> jobs = await Api.RetrieveJobsAsync();
            Assert.AreEqual(jobs[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(jobs[0].Name, ConnectionTestsData.GetDefaultBodyName);
        }

        [Test]
        public async Task RetrieveJobsAsyncSearchShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobsBody;

            var level = new QDataDetail<QJob>()
            {
                Filter = QFilter<QJob>.And(new[]
                {
                    QFilter<QJob>.Eq<string>(t => t.Name, "sample11-task1"),
                    QFilter<QJob>.Lte<string>(t => t.Shortname, "profile"),
                }),
                Select = QSelect<QJob>.Select()
                    .Include(t => t.Uuid),
                MaximumResults = 2,
            };

            List<QJob> jobs = await Api.RetrieveJobsAsync(level);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/jobs/search", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobsAsyncSearchCheckTheReturnBodyUuidName()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetJobsBody;

            var level = new QDataDetail<QJob>()
            {
                Filter = QFilter<QJob>.And(new[]
                {
                    QFilter<QJob>.Eq<string>(t => t.Name, "default_name"),
                }),
                Select = QSelect<QJob>.Select()
                    .Include(t => t.Uuid),
                MaximumResults = 2,
            };

            List<QJob> jobs = await Api.RetrieveJobsAsync(level);
            Assert.AreEqual(jobs[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(jobs[0].Name, ConnectionTestsData.GetDefaultBodyName);
        }

        [Test]
        public async Task RetrieveJobsByTagsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPaginateJobsByTagsBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            var jobs = await Api.RetrieveJobsByTagsAsync(taglist);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/jobs/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobsByTagsAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPaginateJobsByTagsBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            var jobs = (await Api.RetrieveJobsByTagsAsync(taglist)).ToList();

            Assert.AreEqual(jobs.Count, 2);
            Assert.AreEqual(jobs[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(jobs[0].Tags[0], "tag1");
        }

        [Test]
        public async Task RetrievePoolsByTagsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsByTagsPaginateBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            List<QPool> pools = await Api.RetrievePoolsByTagsAsync(taglist);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolsByTagsAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsByTagsPaginateBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            List<QPool> pools = await Api.RetrievePoolsByTagsAsync(taglist);

            Assert.AreEqual(pools.Count, 2);
            Assert.AreEqual(pools[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(pools[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(pools[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolsAsyncSearchShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsBody;

            var level = new QDataDetail<QPool>()
            {
                Filter = QFilter<QPool>.And(new[]
                {
                    QFilter<QPool>.Eq<string>(t => t.Name, "sample11-task1"),
                }),
                Select = QSelect<QPool>.Select()
                    .Include(t => t.Uuid),
                MaximumResults = 2,
            };
            List<QPool> pools = await Api.RetrievePoolsAsync(level);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/search", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolsAsyncSearchCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsBody;

            var level = new QDataDetail<QPool>()
            {
                Filter = QFilter<QPool>.And(new[]
                {
                    QFilter<QPool>.Eq<string>(t => t.Name, "sample11-task1"),
                }),
                Select = QSelect<QPool>.Select()
                    .Include(t => t.Uuid),
                MaximumResults = 2,
            };
            List<QPool> pools = await Api.RetrievePoolsAsync(level);

            Assert.AreEqual(pools[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(pools[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(pools[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolSummariesAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsSummaryPaginateBody;

            List<QPoolSummary> poolSummaries = await Api.RetrievePoolSummariesAsync();

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/summaries/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummariesAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsSummaryPaginateBody;

            List<QPoolSummary> poolSummaries = await Api.RetrievePoolSummariesAsync();

            Assert.AreEqual(poolSummaries[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(poolSummaries[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(poolSummaries[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrievePoolsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsSummaryPaginateBody;

            List<QPool> pools = await Api.RetrievePoolsAsync();

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolsAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsSummaryPaginateBody;

            List<QPool> pools = await Api.RetrievePoolsAsync();
            Assert.AreEqual(pools[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(pools[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(pools[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskSummariesByTagsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsByTagsBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            List<QTaskSummary> taskSummaries = await Api.RetrieveTaskSummariesByTagsAsync(taglist);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("GET", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains("?tag=tag1,tag2", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskSummariesByTagsAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsByTagsBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            List<QTaskSummary> taskSummaries = await Api.RetrieveTaskSummariesByTagsAsync(taglist);

            Assert.AreEqual(taskSummaries[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(taskSummaries[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(taskSummaries[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTasksByTagsAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetPoolsByTagsPaginateBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            List<QTask> tasks = await Api.RetrieveTasksByTagsAsync(taglist);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksByTagsAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksByTagsPaginateBody;

            List<string> taglist = new List<string> { { "tag1" }, { "tag2" }, };
            List<QTask> tasks = await Api.RetrieveTasksByTagsAsync(taglist);

            Assert.AreEqual(tasks[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(tasks[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(tasks[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTasksAsyncSearchShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksSearchBody;

            var level = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.And(new[]
                {
                    QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                }),
                Select = QSelect<QTask>.Select()
                    .Include(t => t.Uuid),
                MaximumResults = 2,
            };
            List<QTask> tasks = await Api.RetrieveTasksAsync(level);

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/search", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksAsyncSearchCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksSearchBody;

            var level = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.And(new[]
                {
                    QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                }),
                Select = QSelect<QTask>.Select()
                    .Include(t => t.Uuid),
                MaximumResults = 2,
            };
            List<QTask> tasks = await Api.RetrieveTasksAsync(level);

            Assert.AreEqual(tasks[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(tasks[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(tasks[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTaskSummariesAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksSummaryPaginateBody;

            List<QTaskSummary> taskSummaries = await Api.RetrieveTaskSummariesAsync();

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/summaries/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskSummariesAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksSummaryPaginateBody;

            List<QTaskSummary> taskSummaries = await Api.RetrieveTaskSummariesAsync();

            Assert.AreEqual(taskSummaries[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(taskSummaries[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(taskSummaries[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTasksAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksPaginateBody;

            List<QTask> tasks = await Api.RetrieveTasksAsync();

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksAsyncCheckTheReturnBodyUuidNameProfile()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetTasksPaginateBody;

            List<QTask> tasks = await Api.RetrieveTasksAsync();
            Assert.AreEqual(tasks[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
            Assert.AreEqual(tasks[0].Name, ConnectionTestsData.GetDefaultBodyName);
            Assert.AreEqual(tasks[0].Profile, ConnectionTestsData.GetDefaultBodyProfile);
        }

        [Test]
        public async Task RetrieveTasksPaginateAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QTask> tasks = await Api.RetrievePaginatedTaskAsync(new PaginatedRequest<QTask>(1));

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksPaginateAsyncShouldSendTheCorrectJsonRequest()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;
            var name = "task_name";

            PaginatedResponse<QTask> task_page = await Api.RetrievePaginatedTaskAsync(new PaginatedRequest<QTask>(1));
            task_page = await Api.RetrievePaginatedTaskAsync(new PaginatedRequest<QTask>(5, "token_value", QFilter<QTask>.Gt(t => t.Name, name)));

            var first_request = "{\"Token\":null,\"Filter\":null,\"MaximumResults\":1}";
            var second_request = "{\"Token\":\"token_value\",\"Filter\":{\"Value\":\"" + name + "\",\"Field\":\"Name\",\"Operator\":\"GreaterThan\"},\"MaximumResults\":5}";

            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(first_request, StringComparison.InvariantCultureIgnoreCase)));
            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(second_request, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksSummariesPaginateAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QTaskSummary> tasks = await Api.RetrievePaginatedTaskSummariesAsync(new PaginatedRequest<QTaskSummary>(1));

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/tasks/summaries/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTasksSummariesPaginateAsyncShouldSendTheCorrectJsonRequest()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QTaskSummary> task_page = await Api.RetrievePaginatedTaskSummariesAsync(new PaginatedRequest<QTaskSummary>(1));

            var first_request = "{\"Token\":null,\"Filter\":null,\"MaximumResults\":1}";

            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(first_request, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveTaskSummariesPaginateAsyncShouldReturnAnHydratedResponse()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetOneTaskPage;

            PaginatedResponse<QTaskSummary> taskPage = await Api.RetrievePaginatedTaskSummariesAsync(new PaginatedRequest<QTaskSummary>(1));

            Assert.True(taskPage.IsTruncated);
            Assert.AreEqual(taskPage.Token, "token");
            Assert.AreEqual(taskPage.NextToken, "next_token");
            Assert.AreEqual(taskPage.Data[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
        }

        [Test]
        public async Task RetrievePoolsPaginateAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QPool> pools = await Api.RetrievePaginatedPoolAsync(new PaginatedRequest<QPool>(1));

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolsPaginateAsyncShouldSendTheCorrectJsonRequest()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;
            var name = "pool_name";

            PaginatedResponse<QPool> pool_page = await Api.RetrievePaginatedPoolAsync(new PaginatedRequest<QPool>(1));
            pool_page = await Api.RetrievePaginatedPoolAsync(new PaginatedRequest<QPool>(5, "token_value", QFilter<QPool>.Gt(t => t.Name, name)));

            var first_request = "{\"Token\":null,\"Filter\":null,\"MaximumResults\":1}";
            var second_request = "{\"Token\":\"token_value\",\"Filter\":{\"Value\":\"" + name + "\",\"Field\":\"Name\",\"Operator\":\"GreaterThan\"},\"MaximumResults\":5}";

            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(first_request, StringComparison.InvariantCultureIgnoreCase)));
            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(second_request, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolsSummariesPaginateAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QPoolSummary> pools = await Api.RetrievePaginatedPoolSummariesAsync(new PaginatedRequest<QPoolSummary>(1));

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/pools/summaries/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummariesPaginateAsyncShouldSendTheCorrectJsonRequest()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QPoolSummary> pool_page = await Api.RetrievePaginatedPoolSummariesAsync(new PaginatedRequest<QPoolSummary>(1));

            var first_request = "{\"Token\":null,\"Filter\":null,\"MaximumResults\":1}";

            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(first_request, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrievePoolSummariesPaginateAsyncShouldReturnAnHydratedResponse()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetOnePoolPage;

            PaginatedResponse<QPoolSummary> poolPage = await Api.RetrievePaginatedPoolSummariesAsync(new PaginatedRequest<QPoolSummary>(1));

            Assert.True(poolPage.IsTruncated);
            Assert.AreEqual(poolPage.Token, "token");
            Assert.AreEqual(poolPage.NextToken, "next_token");
            Assert.AreEqual(poolPage.Data[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
        }

        [Test]
        public async Task RetrieveJobsPaginateAsyncShouldGetOnCorrectEndpoint()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;

            PaginatedResponse<QJob> jobs = await Api.RetrievePaginatedJobAsync(new PaginatedRequest<QJob>(1));

            Assert.True(HttpHandler.ParsedRequests.Any(req =>
                req.Method.Contains("POST", StringComparison.InvariantCultureIgnoreCase) &&
                req.Uri.Contains($"{ComputeUrl}/jobs/paginate", StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobsPaginateAsyncShouldSendTheCorrectJsonRequest()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetEmptyPage;
            var name = "job_name";

            PaginatedResponse<QJob> jobs = await Api.RetrievePaginatedJobAsync(new PaginatedRequest<QJob>(1));
            jobs = await Api.RetrievePaginatedJobAsync(new PaginatedRequest<QJob>(5, "token_value", QFilter<QJob>.Gt(t => t.Name, name)));

            var first_request = "{\"Token\":null,\"Filter\":null,\"MaximumResults\":1}";
            var second_request = "{\"Token\":\"token_value\",\"Filter\":{\"Value\":\"" + name + "\",\"Field\":\"Name\",\"Operator\":\"GreaterThan\"},\"MaximumResults\":5}";

            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(first_request, StringComparison.InvariantCultureIgnoreCase)));
            Assert.True(HttpHandler.ParsedRequests.Any(req => req.Content.Contains(second_request, StringComparison.InvariantCultureIgnoreCase)));
        }

        [Test]
        public async Task RetrieveJobsPaginateAsyncShouldReturnAnHydratedResponse()
        {
            HttpHandler.ResponseBody = ConnectionTestsData.GetOneJobPage;

            PaginatedResponse<QJob> jobPage = await Api.RetrievePaginatedJobAsync(new PaginatedRequest<QJob>(1));

            Assert.True(jobPage.IsTruncated);
            Assert.AreEqual(jobPage.Token, "token");
            Assert.AreEqual(jobPage.NextToken, "next_token");
            Assert.AreEqual(jobPage.Data[0].Uuid.ToString(), ConnectionTestsData.GetDefaultBodyUuid);
        }

        [Test]
        public void CreateTaskFromConnectionCheckReturnBody()
        {
            Connection connect = new Connection("https://localhost", "token");
            QTask task = connect.CreateTask("name");
            QPool pool = connect.CreatePool("name");
            QJob job = connect.CreateJob("name");
            Assert.AreEqual(task.Name, "name");
            task = connect.CreateTask("name", "profile", "shortname");
            Assert.AreEqual(task.Name, "name");
            Assert.AreEqual(task.Profile, "profile");
            Assert.AreEqual(task.Shortname, "shortname");
            task = connect.CreateTask("name", "profile", 777, "shortname");
            task = connect.CreateTask("name", "profile", new AdvancedRanges("9-99, 0-1"), "shortname");
            task = connect.CreateTask("name", pool, 777, "shortname");
            task = connect.CreateTask("name", pool, new AdvancedRanges("9-99, 0-1"), "shortname");
            task = connect.CreateTask("name", job, 777, "shortname");
            task = connect.CreateTask("name", job, new AdvancedRanges("9-99, 0-1"), "shortname");

            task = connect.CreateTask("name", pool, new AdvancedRanges("9-99, 0-1"), "shortname", waitForPoolResourcesSynchronization: true);
            Assert.That(task.WaitForPoolResourcesSynchronization, Is.True);

            task = connect.CreateTask("name", pool, new AdvancedRanges("9-99, 0-1"), "shortname", waitForPoolResourcesSynchronization: false);
            Assert.That(task.WaitForPoolResourcesSynchronization, Is.False);

            task = connect.CreateTask("name", pool, new AdvancedRanges("9-99, 0-1"), "shortname", waitForPoolResourcesSynchronization: null);
            Assert.That(task.WaitForPoolResourcesSynchronization, Is.Null);

            task = connect.CreateTask("name", pool, 3, "shortname", waitForPoolResourcesSynchronization: true);
            Assert.That(task.WaitForPoolResourcesSynchronization, Is.True);

            task = connect.CreateTask("name", pool, 3, "shortname", waitForPoolResourcesSynchronization: false);
            Assert.That(task.WaitForPoolResourcesSynchronization, Is.False);

            task = connect.CreateTask("name", pool, 3, "shortname", waitForPoolResourcesSynchronization: null);
            Assert.That(task.WaitForPoolResourcesSynchronization, Is.Null);
        }

        [Test]
        public void CreateJobFromConnectionCheckReturnBody()
        {
            QJob job = Api.CreateJob("name");
            Assert.AreEqual(job.Name, "name");
            job = Api.CreateJob("name", pool: (QPool)null, shortname: "shortname", UseTaskDependencies: true);
            Assert.AreEqual(job.Shortname, "shortname");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [TestCase(null)]
        public void CreatePoolFromConnectionCheckReturnBody(bool? taskDefaultWaitForPoolResourcesSynchronization)
        {
            QPool pool = Api.CreatePool("name");
            Assert.AreEqual(pool.Name, "name");
            pool = Api.CreatePool("name", profile: "profile", initialNodeCount: 5, shortname: "shortname",
                                  taskDefaultWaitForPoolResourcesSynchronization: taskDefaultWaitForPoolResourcesSynchronization);
            Assert.AreEqual(pool.Shortname, "shortname");
            Assert.AreEqual(pool.Profile, "profile");
            Assert.AreEqual(pool.TaskDefaultWaitForPoolResourcesSynchronization, taskDefaultWaitForPoolResourcesSynchronization);
        }

        [Test]
        public async Task S3ClientChangeRetryClientValue()
        {
            var newConnection1 = new Connection(ComputeUrl, StorageUrl, Token, HttpHandler) {
                StorageAccessKey = "notEmpty@qarnot.com",
            };
            var s3Client = await newConnection1.GetS3ClientAsync(default(CancellationToken));
            Assert.AreEqual(s3Client.Config.MaxErrorRetry, 3);
            var newConnection2 = new Connection(ComputeUrl, StorageUrl, Token, HttpHandler) {
                StorageAccessKey = "notEmpty@qarnot.com",
                MaxStorageRetry = 10,
            };
            s3Client = await newConnection2.GetS3ClientAsync(default(CancellationToken));
            Assert.AreEqual(s3Client.Config.MaxErrorRetry, 10);
        }

        [Test]
        public void AddS3HttpClinetFactory()
        {
            var connect = new Connection("token") {
                S3HttpClientFactory = new UnsafeS3HttpClientFactory(),
            };
        }

        [Test]
        public void CheckConnectionConstructorsS3HttpClientFactory()
        {
            Connection connect = null;

            connect = new Connection("https://localhost/uri", "https://localhost/stockage_uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost/uri", "https://localhost/stockage_uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost/uri", "https://localhost/stockage_uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost/uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost/uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost/uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
        }


        [Test]
        public void CheckConnectionConstructorsSetValues()
        {
            Connection connect = null;
            connect = new Connection("token");
            connect = new Connection("token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost", "token");
            connect = new Connection("https://localhost/uri", "token",  httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            connect = new Connection("https://localhost/uri", "https://localhost/stockage_uri", "token");
            connect = new Connection("https://localhost/uri", "https://localhost/stockage_uri", "token", httpClientHandler: default(HttpClientHandler), retryHandler: default(IRetryHandler), forceStoragePathStyle: true);
            Assert.AreEqual(connect.StorageUploadPartSize, 8 * 1024 * 1024);
            Assert.AreEqual(connect.Uri.ToString(), "https://localhost/uri");
            Assert.AreEqual(connect.StorageUri, "https://localhost/stockage_uri");
            Assert.AreEqual(connect.Token, "token");
            Assert.AreEqual(connect.StorageSecretKey, "token");
            Assert.IsNotNull(connect._client);
            Assert.IsNotNull(connect._httpClientHandler);
            Assert.IsNotNull(connect._retryHandler);
        }
    }
}

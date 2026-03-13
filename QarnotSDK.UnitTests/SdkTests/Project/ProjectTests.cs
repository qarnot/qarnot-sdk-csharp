namespace QarnotSDK.UnitTests.SdkTests.Carbon
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    [Category("Project")]
    public class ProjectTests
    {
        private const string StorageUrl = "http://storage";
        private const string ApiUrl = "http://api";
        private const string Token = "token";

        private Connection Connection { get; set; }

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler();
            Connection = new Connection(ApiUrl, StorageUrl, Token, HttpHandler)
            {
                StorageAccessKey = "fake@mail.com",
            };
        }

        [TearDown]
        public void TearDown()
        {
            HttpHandler.Dispose();
        }

        [Test]
        public async Task TestProjectDeserialization()
        {
            var projectJson = ProjectTestData.ProjectJson;
            var projectApi = JsonConvert.DeserializeObject<ProjectApi>(projectJson);
            var project = new QProject(projectApi);
            Assert.That(project.Uuid.ToString(), Is.EqualTo(ProjectTestData.ProjectUuid), "Uuid of the project should have the value of the uuid json field");
            Assert.That(project.Name, Is.EqualTo(ProjectTestData.ProjectName), "Name of the project should have the value of the name json field");
            Assert.That(project.Slug, Is.EqualTo(ProjectTestData.ProjectSlug), "Slug of the project should have the value of the slug json field");
            Assert.That(project.Description, Is.EqualTo(ProjectTestData.ProjectDescription), "Description of the project should have the value of the description json field");
            Assert.That(project.OrganizationUuid.ToString(), Is.EqualTo(ProjectTestData.OrganizationUuid), "OrganizationUuid of the project should have the value of the organizationUuid json field");
        }

        [Test]
        public async Task TestUserProjectsDeserialization()
        {
            HttpHandler.ResponseBody = ProjectTestData.UserInfoWithProjectsJson;
            var userInformation = await Connection.RetrieveUserInformationAsync(false);

            Assert.That(userInformation.Projects, Has.Count.EqualTo(2), "All projects from json should be deserialized in user info");
            var firstProject = userInformation.Projects.First();
            Assert.That(firstProject.Uuid.ToString(), Is.EqualTo(ProjectTestData.ProjectUuid), "Uuid of the project should have the value of the uuid json field");
            Assert.That(firstProject.Name, Is.EqualTo(ProjectTestData.ProjectName), "Name of the project should have the value of the name json field");
            Assert.That(firstProject.Slug, Is.EqualTo(ProjectTestData.ProjectSlug), "Slug of the project should have the value of the slug json field");
            Assert.That(firstProject.Description, Is.EqualTo(ProjectTestData.ProjectDescription), "Description of the project should have the value of the description json field");
            Assert.That(firstProject.OrganizationUuid.ToString(), Is.EqualTo(ProjectTestData.OrganizationUuid), "OrganizationUuid of the project should have the value of the organizationUuid json field");
        }

        [Test]
        public void TestGetProjectByName()
        {
            var userInformation = JsonConvert.DeserializeObject<UserInformation>(ProjectTestData.UserInfoWithProjectsJson);

            var foundProject = userInformation.GetProjectByName(ProjectTestData.ProjectName);
            var missingProject = userInformation.GetProjectByName("nonexistent-project");
            var caseInsensitiveProject = userInformation.GetProjectByName(ProjectTestData.ProjectName.ToUpperInvariant());

            Assert.That(foundProject, Is.Not.Null, "GetProjectByName should return the project when the name matches");
            Assert.That(foundProject.Uuid.ToString(), Is.EqualTo(ProjectTestData.ProjectUuid), "GetProjectByName should return the project with the matching name");
            Assert.That(missingProject, Is.Null, "GetProjectByName should return null when no project matches the given name");
            Assert.That(caseInsensitiveProject, Is.Not.Null, "GetProjectByName should match project names case-insensitively");
        }

        [Test]
        public void TestGetProjectBySlug()
        {
            var userInformation = JsonConvert.DeserializeObject<UserInformation>(ProjectTestData.UserInfoWithProjectsJson);

            var foundProject = userInformation.GetProjectBySlug(ProjectTestData.ProjectSlug);
            var missingProject = userInformation.GetProjectBySlug("nonexistent-slug");
            var caseInsensitiveProject = userInformation.GetProjectBySlug(ProjectTestData.ProjectSlug.ToUpperInvariant());

            Assert.That(foundProject, Is.Not.Null, "GetProjectBySlug should return the project when the slug matches");
            Assert.That(foundProject.Uuid.ToString(), Is.EqualTo(ProjectTestData.ProjectUuid), "GetProjectBySlug should return the project with the matching slug");
            Assert.That(missingProject, Is.Null, "GetProjectBySlug should return null when no project matches the given slug");
            Assert.That(caseInsensitiveProject, Is.Not.Null, "GetProjectBySlug should match project slugs case-insensitively");
        }

        [Test]
        public void TestTaskProjectUuidPropertyRoundTrip()
        {
            var expectedProjectUuid = new Guid(ProjectTestData.ProjectUuid);
            var task = new QTask(Connection, "test-task", "docker-batch", 1);

            task.ProjectUuid = expectedProjectUuid;

            Assert.That(task.ProjectUuid, Is.EqualTo(expectedProjectUuid), "ProjectUuid getter should return the same value that was set");
        }

        [Test]
        public void TestPoolProjectUuidPropertyRoundTrip()
        {
            var expectedProjectUuid = new Guid(ProjectTestData.ProjectUuid);
            var pool = new QPool(Connection, "test-pool", "docker-batch", 1);

            pool.ProjectUuid = expectedProjectUuid;

            Assert.That(pool.ProjectUuid, Is.EqualTo(expectedProjectUuid), "ProjectUuid getter should return the same value that was set");
        }

        [Test]
        public async Task TestTaskUpdateWithProjectUuid()
        {
            HttpHandler.ResponseBody = ProjectTestData.TaskWithProjectUuidJson;
            string uuid = Guid.NewGuid().ToString();
            QTask task = new QTask(Connection, uuid);
            await task.UpdateStatusAsync();

            Assert.That(task.ProjectUuid.ToString(), Is.EqualTo(ProjectTestData.ProjectUuid), "Task ProjectUuid should be update by the deserialization from the projectUuid json field");
        }

        [Test]
        public async Task TestPoolUpdateWithProjectUuid()
        {
            HttpHandler.ResponseBody = ProjectTestData.PoolWithProjectUuidJson;
            string uuid = Guid.NewGuid().ToString();
            QPool pool = new QPool(Connection, uuid);
            await pool.UpdateStatusAsync();

            Assert.That(pool.ProjectUuid.ToString(), Is.EqualTo(ProjectTestData.ProjectUuid), "Pool ProjectUuid should be update by the deserialization from the projectUuid json field");;
        }

        [Test]
        public async Task TestProjectUuidIsSentOnTaskSubmission()
        {
            var userInformation = JsonConvert.DeserializeObject<UserInformation>(ProjectTestData.UserInfoWithProjectsJson);
            var project = userInformation.GetProjectByName(ProjectTestData.ProjectName);

            var task = new QTask(Connection, "task-with-project", "docker-batch", 1);
            task.ProjectUuid = project.Uuid;

            await task.SubmitAsync();

            var taskPostRequest = HttpHandler.ParsedRequests.FirstOrDefault(req =>
                req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                req.Uri.Contains("tasks", StringComparison.OrdinalIgnoreCase));

            Assert.That(taskPostRequest, Is.Not.Null, "A POST to tasks should have been made during SubmitAsync");

            var submittedBody = JObject.Parse(taskPostRequest.Content);
            Assert.That(
                submittedBody["ProjectUuid"]?.ToString(),
                Is.EqualTo(ProjectTestData.ProjectUuid),
                "The projectUuid should be included in the task POST body with the value assigned before submission");
        }

        [Test]
        public async Task TestProjectUuidIsSentOnPoolSubmission()
        {
            HttpHandler.ResponseBody = ProjectTestData.PoolWithProjectUuidJson;

            var userInformation = JsonConvert.DeserializeObject<UserInformation>(ProjectTestData.UserInfoWithProjectsJson);
            var project = userInformation.GetProjectByName(ProjectTestData.ProjectName);

            var pool = new QPool(Connection, "pool-with-project", "docker-batch", 1);
            pool.ProjectUuid = project.Uuid;

            await pool.StartAsync();

            var poolPostRequest = HttpHandler.ParsedRequests.FirstOrDefault(req =>
                req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase) &&
                req.Uri.Contains("pools", StringComparison.OrdinalIgnoreCase));

            Assert.That(poolPostRequest, Is.Not.Null, "A POST to pools should have been made during StartAsync");

            var submittedBody = JObject.Parse(poolPostRequest.Content);
            Assert.That(
                submittedBody["ProjectUuid"]?.ToString(),
                Is.EqualTo(ProjectTestData.ProjectUuid),
                "The projectUuid should be included in the pool POST body with the value assigned before submission");
        }
    }
}

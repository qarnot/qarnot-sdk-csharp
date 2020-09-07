namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class RetrieveTasksTest
    {
        private Connection Connect { get; set; }

        private InterceptingFakeHttpHandler HttpHandler { get; set; }

        [SetUp]
        public void SetUp()
        {
            HttpHandler = new InterceptingFakeHttpHandler();
            HttpHandler.ResponseBody = TaskTestsData.TasksListBodies;
            Connect = new Connection("http://api", "http://storage", "token", HttpHandler);
        }

        [TearDown]
        public void TearDown()
        {
            HttpHandler.Dispose();
        }

        [Test]
        public async Task RetrieveTasksEndedLessThan15MinAgoSendAValidRequest()
        {
            QDataDetail<QTask> dataDetail = new QDataDetail<QTask>();
            DateTime change15MinutesAgo = DateTime.UtcNow.AddMinutes(-15);

            // Determinate the time, for the check
            change15MinutesAgo = new DateTime(10, 10, 3, 10, 10, 10, DateTimeKind.Utc);
            dataDetail.Filter = QFilter<QTask>.And(new[]
                {
                    QFilter<QTask>.Gt(t => t.StateTransitionTime, change15MinutesAgo),
                    QFilter<QTask>.Or(new[]
                    {
                        QFilter<QTask>.Eq(t => t.PreviousState, QTaskStates.Submitted),
                        QFilter<QTask>.Eq(t => t.PreviousState, QTaskStates.PartiallyDispatched),
                        QFilter<QTask>.Eq(t => t.PreviousState, QTaskStates.FullyDispatched),
                        QFilter<QTask>.Eq(t => t.PreviousState, QTaskStates.PartiallyExecuting),
                    }),
                    QFilter<QTask>.Or(new[]
                    {
                        QFilter<QTask>.Eq(t => t.State, QTaskStates.FullyExecuting),
                        QFilter<QTask>.Eq(t => t.State, QTaskStates.UploadingResults),
                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Cancelled),
                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Success),
                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Failure),
                    }),
                });

            HttpHandler.ResponseBody = TaskTestsData.TasksListBodies;
            List<QTask> tasks = await Connect.RetrieveTasksAsync(dataDetail);
            foreach (var parsedRequest in HttpHandler.ParsedRequests)
            {
                Console.WriteLine(parsedRequest.Content.ToString());
            }

            string filterWaited = "Filter\":{\"Operator\":\"And\",\"Filters\":[{\"Value\":\"0010-10-03T10:10:10Z\",\"Field\":\"StateTransitionTime\",\"Operator\":\"GreaterThan\"},{\"Operator\":\"Or\",\"Filters\":[{\"Value\":\"Submitted\",\"Field\":\"PreviousState\",\"Operator\":\"Equal\"},{\"Value\":\"PartiallyDispatched\",\"Field\":\"PreviousState\",\"Operator\":\"Equal\"},{\"Value\":\"FullyDispatched\",\"Field\":\"PreviousState\",\"Operator\":\"Equal\"},{\"Value\":\"PartiallyExecuting\",\"Field\":\"PreviousState\",\"Operator\":\"Equal\"}]},{\"Operator\":\"Or\",\"Filters\":[{\"Value\":\"FullyExecuting\",\"Field\":\"State\",\"Operator\":\"Equal\"},{\"Value\":\"UploadingResults\",\"Field\":\"State\",\"Operator\":\"Equal\"},{\"Value\":\"Cancelled\",\"Field\":\"State\",\"Operator\":\"Equal\"},{\"Value\":\"Success\",\"Field\":\"State\",\"Operator\":\"Equal\"},{\"Value\":\"Failure\",\"Field\":\"State\",\"Operator\":\"Equal\"}]}]}";
            StringAssert.Contains(filterWaited, HttpHandler.ParsedRequests[0].Content);
        }
    }
}
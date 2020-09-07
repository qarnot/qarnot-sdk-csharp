namespace QarnotSDK.UnitTests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class DataDetailTests
    {
        [Test]
        public void GetPropertyInfoInvalidReturnTypeShouldFail()
        {
            Exception ex = Assert.Throws<ArgumentException>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => "basic string"));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetPropertyInfoNullFuncShouldFail()
        {
            Exception ex = Assert.Throws<ArgumentNullException>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(null));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPoolPropertyNameAsUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, Guid>(t => t.Uuid);
            Assert.AreEqual(value, "Uuid");
        }

        [Test]
        public void GetAPISelectPoolPropertyNameAsUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, Guid>(t => t.Uuid);
            Assert.AreEqual(value, "Uuid");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolConnectionShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QPool, Connection>(t => t.Connection));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolConnectionShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QPool, Connection>(t => t.Connection));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForTaskUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, Guid>(t => t.Uuid);
            Assert.AreEqual(value, "Uuid");
        }

        [Test]
        public void GetAPISelectPropertyNameForTaskUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, Guid>(t => t.Uuid);
            Assert.AreEqual(value, "Uuid");
        }

        [Test]
        public void GetAPIFilterPropertyNameForTaskConnectionShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, Connection>(t => t.Connection));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForTaskConnectionShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, Connection>(t => t.Connection));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobPoolShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QJob, QPool>(t => t.Pool));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForJobPoolShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QJob, QPool>(t => t.Pool));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForMaxWallTimeShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QJob, TimeSpan>(t => t.MaximumWallTime));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForMaxWallTimeShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QJob, TimeSpan>(t => t.MaximumWallTime));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobUseDependenciesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, bool>(t => t.UseDependencies);
            Assert.AreEqual(value, "UseDependencies");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobUseDependenciesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, bool>(t => t.UseDependencies);
            Assert.AreEqual(value, "UseDependencies");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobLastModifiedReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, System.DateTime?>(t => t.LastModified);
            Assert.AreEqual(value, "LastModified");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobLastModifiedReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, System.DateTime?>(t => t.LastModified);
            Assert.AreEqual(value, "LastModified");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobCreationDateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, System.DateTime?>(t => t.CreationDate);
            Assert.AreEqual(value, "CreationDate");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobCreationDateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, System.DateTime>(t => t.CreationDate);
            Assert.AreEqual(value, "CreationDate");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobStateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, string>(t => t.State);
            Assert.AreEqual(value, "State");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobStateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, string>(t => t.State);
            Assert.AreEqual(value, "State");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobPoolUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, Guid>(t => t.PoolUuid);
            Assert.AreEqual(value, "PoolUuid");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobPoolUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, Guid>(t => t.PoolUuid);
            Assert.AreEqual(value, "PoolUuid");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobNameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, string>(t => t.Name);
            Assert.AreEqual(value, "Name");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobNameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, string>(t => t.Name);
            Assert.AreEqual(value, "Name");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobShortnameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, string>(t => t.Shortname);
            Assert.AreEqual(value, "Shortname");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobShortnameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, string>(t => t.Shortname);
            Assert.AreEqual(value, "Shortname");
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobSelfUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QJob, Guid>(t => t.Uuid);
            Assert.AreEqual(value, "Uuid");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobSelfUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QJob, Guid>(t => t.Uuid);
            Assert.AreEqual(value, "Uuid");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolElasticMinimumIdlingTimeReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, uint>(t => t.ElasticMinimumIdlingTime);
            Assert.AreEqual(value, "ElasticProperty.MinIdleTimeSeconds");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolElasticMinimumIdlingTimeReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, uint>(t => t.ElasticMinimumIdlingTime);
            Assert.AreEqual(value, "ElasticProperty.MinIdleTimeSeconds");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolElasticResizeFactorReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, float>(t => t.ElasticResizeFactor);
            Assert.AreEqual(value, "ElasticProperty.RampResizeFactor");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolElasticResizeFactorReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, float>(t => t.ElasticResizeFactor);
            Assert.AreEqual(value, "ElasticProperty.RampResizeFactor");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolElasticResizePeriodReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, uint>(t => t.ElasticResizePeriod);
            Assert.AreEqual(value, "ElasticProperty.ResizePeriod");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolElasticResizePeriodReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, uint>(t => t.ElasticResizePeriod);
            Assert.AreEqual(value, "ElasticProperty.ResizePeriod");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolElasticMinimumIdlingNodesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, uint>(t => t.ElasticMinimumIdlingNodes);
            Assert.AreEqual(value, "ElasticProperty.MinIdleSlots");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolElasticMinimumIdlingNodesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, uint>(t => t.ElasticMinimumIdlingNodes);
            Assert.AreEqual(value, "ElasticProperty.MinIdleSlots");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolElasticMaximumTotalNodesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, uint>(t => t.ElasticMaximumTotalNodes);
            Assert.AreEqual(value, "ElasticProperty.MaxTotalSlots");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolElasticMaximumTotalNodesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, uint>(t => t.ElasticMaximumTotalNodes);
            Assert.AreEqual(value, "ElasticProperty.MaxTotalSlots");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolElasticMinimumTotalNodesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, uint>(t => t.ElasticMinimumTotalNodes);
            Assert.AreEqual(value, "ElasticProperty.MinTotalSlots");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolElasticMinimumTotalNodesReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, uint>(t => t.ElasticMinimumTotalNodes);
            Assert.AreEqual(value, "ElasticProperty.MinTotalSlots");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolIsElasticReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, bool>(t => t.IsElastic);
            Assert.AreEqual(value, "ElasticProperty.IsElastic");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolPreparationCommandLineReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, string>(t => t.PreparationCommandLine);
            Assert.AreEqual(value, "PreparationTask.CommandLine");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolConstraintsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QPool, Dictionary<string, string>>(t => t.Constraints));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolConstraintsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, Dictionary<string, string>>(t => t.Constraints);
            Assert.AreEqual(value, "Constraints");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolConstantsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QPool, Dictionary<string, string>>(t => t.Constants));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolConstantsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, Dictionary<string, string>>(t => t.Constants);
            Assert.AreEqual(value, "Constants");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolTagsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, List<string>>(t => t.Tags);
            Assert.AreEqual(value, "Tags");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolTagsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, List<string>>(t => t.Tags);
            Assert.AreEqual(value, "Tags");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolNodeCountReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, uint>(t => t.NodeCount);
            Assert.AreEqual(value, "InstanceCount");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolNodeCountReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, uint>(t => t.NodeCount);
            Assert.AreEqual(value, "InstanceCount");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolCreationDateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, System.DateTime>(t => t.CreationDate);
            Assert.AreEqual(value, "CreationDate");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolCreationDateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, System.DateTime>(t => t.CreationDate);
            Assert.AreEqual(value, "CreationDate");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolStatusShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QPool, QarnotSDK.QPoolStatus>(t => t.Status));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolStatusReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, QarnotSDK.QPoolStatus>(t => t.Status);
            Assert.AreEqual(value, "Status");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolErrorsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QPool, List<QarnotSDK.QPoolError>>(t => t.Errors));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolErrorsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, List<QarnotSDK.QPoolError>>(t => t.Errors);
            Assert.AreEqual(value, "Errors");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolStateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, string>(t => t.State);
            Assert.AreEqual(value, "State");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolStateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, string>(t => t.State);
            Assert.AreEqual(value, "State");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolResourcesShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QPool, List<QarnotSDK.QAbstractStorage>>(t => t.Resources));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolResourcesShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QPool, List<QarnotSDK.QAbstractStorage>>(t => t.Resources));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolProfileReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, string>(t => t.Profile);
            Assert.AreEqual(value, "Profile");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolProfileReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, string>(t => t.Profile);
            Assert.AreEqual(value, "Profile");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolNameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, string>(t => t.Name);
            Assert.AreEqual(value, "Name");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolNameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, string>(t => t.Name);
            Assert.AreEqual(value, "Name");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolShortnameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QPool, string>(t => t.Shortname);
            Assert.AreEqual(value, "Shortname");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolShortnameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QPool, string>(t => t.Shortname);
            Assert.AreEqual(value, "Shortname");
        }

        [Test]
        public void GetAPIFilterPropertyNameForSnapshotBlacklistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.SnapshotBlacklist);
            Assert.AreEqual(value, "SnapshotBlacklist");
        }

        [Test]
        public void GetAPISelectPropertyNameForSnapshotBlacklistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.SnapshotBlacklist);
            Assert.AreEqual(value, "SnapshotBlacklist");
        }

        [Test]
        public void GetAPIFilterPropertyNameForSnapshotWhitelistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.SnapshotWhitelist);
            Assert.AreEqual(value, "SnapshotWhitelist");
        }

        [Test]
        public void GetAPISelectPropertyNameForSnapshotWhitelistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.SnapshotWhitelist);
            Assert.AreEqual(value, "SnapshotWhitelist");
        }

        [Test]
        public void GetAPIFilterPropertyNameForResultsBlacklistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.ResultsBlacklist);
            Assert.AreEqual(value, "ResultsBlacklist");
        }

        [Test]
        public void GetAPISelectPropertyNameForResultsBlacklistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.ResultsBlacklist);
            Assert.AreEqual(value, "ResultsBlacklist");
        }

        [Test]
        public void GetAPIFilterPropertyNameForResultsWhitelistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.ResultsWhitelist);
            Assert.AreEqual(value, "ResultsWhitelist");
        }

        [Test]
        public void GetAPISelectPropertyNameForResultsWhitelistReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.ResultsWhitelist);
            Assert.AreEqual(value, "ResultsWhitelist");
        }

        [Test]
        public void GetAPIFilterPropertyNameForInstanceCountReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, uint>(t => t.InstanceCount);
            Assert.AreEqual(value, "InstanceCount");
        }

        [Test]
        public void GetAPISelectPropertyNameForInstanceCountReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, uint>(t => t.InstanceCount);
            Assert.AreEqual(value, "InstanceCount");
        }

        [Test]
        public void GetAPIFilterPropertyNameForExecutingShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, bool>(t => t.Executing));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForExecutingShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, bool>(t => t.Executing));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForCompletedShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, bool>(t => t.Completed));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForCompletedShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, bool>(t => t.Completed));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, QJob>(t => t.Job));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForJobShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, QJob>(t => t.Job));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForJobUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, System.Guid>(t => t.JobUuid);
            Assert.AreEqual(value, "JobUuid");
        }

        [Test]
        public void GetAPISelectPropertyNameForJobUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, System.Guid>(t => t.JobUuid);
            Assert.AreEqual(value, "JobUuid");
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, QPool>(t => t.Pool));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, QPool>(t => t.Pool));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForPoolUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, System.Guid>(t => t.PoolUuid);
            Assert.AreEqual(value, "PoolUuid");
        }

        [Test]
        public void GetAPISelectPropertyNameForPoolUuidReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, System.Guid>(t => t.PoolUuid);
            Assert.AreEqual(value, "PoolUuid");
        }

        [Test]
        public void GetAPIFilterPropertyNameForSnapshotIntervalReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, int>(t => t.SnapshotInterval);
            Assert.AreEqual(value, "SnapshotInterval");
        }

        [Test]
        public void GetAPISelectPropertyNameForSnapshotIntervalReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, int>(t => t.SnapshotInterval);
            Assert.AreEqual(value, "SnapshotInterval");
        }

        [Test]
        public void GetAPIFilterPropertyNameForDependsOnReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, List<System.Guid>>(t => t.DependsOn);
            Assert.AreEqual(value, "Dependencies.DependsOn");
        }

        [Test]
        public void GetAPISelectPropertyNameForDependsOnReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, List<System.Guid>>(t => t.DependsOn);
            Assert.AreEqual(value, "Dependencies.DependsOn");
        }

        [Test]
        public void GetAPIFilterPropertyNameForConstraintsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, Dictionary<string, string>>(t => t.Constraints);
            Assert.AreEqual(value, "Constraints");
        }

        [Test]
        public void GetAPISelectPropertyNameForConstraintsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, Dictionary<string, string>>(t => t.Constraints);
            Assert.AreEqual(value, "Constraints");
        }

        [Test]
        public void GetAPIFilterPropertyNameForConstantsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, Dictionary<string, string>>(t => t.Constants);
            Assert.AreEqual(value, "Constants");
        }

        [Test]
        public void GetAPISelectPropertyNameForConstantsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, Dictionary<string, string>>(t => t.Constants);
            Assert.AreEqual(value, "Constants");
        }

        [Test]
        public void GetAPIFilterPropertyNameForTagsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, List<string>>(t => t.Tags);
            Assert.AreEqual(value, "Tags");
        }

        [Test]
        public void GetAPISelectPropertyNameForTagsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, List<string>>(t => t.Tags);
            Assert.AreEqual(value, "Tags");
        }

        [Test]
        public void GetAPIFilterPropertyNameForResultsCountReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, uint>(t => t.ResultsCount);
            Assert.AreEqual(value, "ResultsCount");
        }

        [Test]
        public void GetAPISelectPropertyNameForResultsCountReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, uint>(t => t.ResultsCount);
            Assert.AreEqual(value, "ResultsCount");
        }

        [Test]
        public void GetAPIFilterPropertyNameForCreationDateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, System.DateTime>(t => t.CreationDate);
            Assert.AreEqual(value, "CreationDate");
        }

        [Test]
        public void GetAPISelectPropertyNameForCreationDateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, System.DateTime>(t => t.CreationDate);
            Assert.AreEqual(value, "CreationDate");
        }

        [Test]
        public void GetAPIFilterPropertyNameForStatusShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, QarnotSDK.QTaskStatus>(t => t.Status));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForStatusReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, QarnotSDK.QTaskStatus>(t => t.Status);
            Assert.AreEqual(value, "Status");
        }

        [Test]
        public void GetAPIFilterPropertyNameForErrorsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, List<QarnotSDK.QTaskError>>(t => t.Errors));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForErrorsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, List<QarnotSDK.QTaskError>>(t => t.Errors);
            Assert.AreEqual(value, "Errors");
        }

        [Test]
        public void GetAPIFilterPropertyNameForStateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.State);
            Assert.AreEqual(value, "State");
        }

        [Test]
        public void GetAPISelectPropertyNameForStateReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.State);
            Assert.AreEqual(value, "State");
        }

        [Test]
        public void GetAPIFilterPropertyNameForResultBucketReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, QBucket>(t => t.ResultsBucket);
            Assert.AreEqual(value, "ResultBucket");
        }

        [Test]
        public void GetAPISelectPropertyNameForResultBucketReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, QBucket>(t => t.ResultsBucket);
            Assert.AreEqual(value, "ResultBucket");
        }

        [Test]
        public void GetAPIFilterPropertyNameForResultsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, QarnotSDK.QAbstractStorage>(t => t.Results));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForResultsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, QarnotSDK.QAbstractStorage>(t => t.Results));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForResourcesBucketsShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, IEnumerable<QarnotSDK.QBucket>>(t => t.ResourcesBuckets));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForResourcesBucketsReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, IEnumerable<QarnotSDK.QBucket>>(t => t.ResourcesBuckets);
            Assert.AreEqual(value, "ResourceBuckets");
        }

        [Test]
        public void GetAPIFilterPropertyNameForResourcesShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPIFilterPropertyName<QTask, List<QarnotSDK.QAbstractStorage>>(t => t.Resources));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPISelectPropertyNameForResourcesShouldFail()
        {
            Exception ex = Assert.Throws<Exception>(() => DataDetailHelper.GetAPISelectPropertyName<QTask, List<QarnotSDK.QAbstractStorage>>(t => t.Resources));
            Assert.IsNotNull(ex);
        }

        [Test]
        public void GetAPIFilterPropertyNameForProfileReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.Profile);
            Assert.AreEqual(value, "Profile");
        }

        [Test]
        public void GetAPISelectPropertyNameForProfileReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.Profile);
            Assert.AreEqual(value, "Profile");
        }

        [Test]
        public void GetAPIFilterPropertyNameForShortnameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.Shortname);
            Assert.AreEqual(value, "Shortname");
        }

        [Test]
        public void GetAPISelectPropertyNameForShortnameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.Shortname);
            Assert.AreEqual(value, "Shortname");
        }

        [Test]
        public void GetAPIFilterPropertyNameForNameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPIFilterPropertyName<QTask, string>(t => t.Name);
            Assert.AreEqual(value, "Name");
        }

        [Test]
        public void GetAPISelectPropertyNameForNameReturnTheGoodStringValue()
        {
            var value = DataDetailHelper.GetAPISelectPropertyName<QTask, string>(t => t.Name);
            Assert.AreEqual(value, "Name");
        }
    }

    [TestFixture]
    public class UnitTestQSelect
    {
        [Test]
        public void SelectIncludeAllTest()
        {
            var select = QSelect<QPool>.Select()
                .Include(t => t.Name)
                .Include(t => t.Uuid)
                .Include(t => t.Constants)
                .Include(t => t.Constraints)
                .Include(t => t.CreationDate)
                .Include(t => t.ElasticMaximumTotalNodes)
                .Include(t => t.ElasticMinimumIdlingNodes)
                .Include(t => t.ElasticMinimumIdlingTime)
                .Include(t => t.ElasticMinimumTotalNodes)
                .Include(t => t.ElasticResizeFactor)
                .Include(t => t.ElasticResizePeriod)
                .Include(t => t.Errors)
                .Include(t => t.IsElastic)
                .Include(t => t.NodeCount)
                .Include(t => t.Profile)
                .Include(t => t.ResourcesBuckets)
                .Include(t => t.Shortname)
                .Include(t => t.State)
                .Include(t => t.Status)
                .Include(t => t.Tags)
                .Include(t => t.Uuid);
            string tostring = select.ToString();
            string waitstring = "<Fields: Name,Uuid,Constants,Constraints,CreationDate,ElasticProperty.MaxTotalSlots,ElasticProperty.MinIdleSlots,ElasticProperty.MinIdleTimeSeconds,ElasticProperty.MinTotalSlots,ElasticProperty.RampResizeFactor,ElasticProperty.ResizePeriod,Errors,ElasticProperty.IsElastic,InstanceCount,Profile,ResourceBuckets,Shortname,State,Status,Tags,Uuid>";
            Assert.AreEqual(tostring, waitstring);
            var select2 = QSelect<QTask>.Select()
                .Include(t => t.Name)
                .Include(t => t.Uuid)
                .Include(t => t.Constants)
                .Include(t => t.Constraints)
                .Include(t => t.CreationDate)
                .Include(t => t.Errors)
                .Include(t => t.Profile)
                .Include(t => t.ResourcesBuckets)
                .Include(t => t.Shortname)
                .Include(t => t.State)
                .Include(t => t.Status)
                .Include(t => t.Tags)
                .Include(t => t.Uuid);
            tostring = select2.ToString();
            waitstring = "<Fields: Name,Uuid,Constants,Constraints,CreationDate,Errors,Profile,ResourceBuckets,Shortname,State,Status,Tags,Uuid>";
            Assert.AreEqual(tostring, waitstring);
        }

        [Test]
        public void SelectIncludeBasicTest()
        {
            var select = QSelect<QTask>.Select()
            .Include(t => t.Name)
            .Include(t => t.Uuid)
            .Include(t => t.State);
            select.Include(t => t.Status);
            string tostring = select.ToString();
            string waitstring = "<Fields: Name,Uuid,State,Status>";
            Assert.AreEqual(tostring, waitstring);
        }

        [Test]
        public void SelectSimpleBasicTest()
        {
            var select = QSelect<QTask>.Select();
            select = QSelect<QTask>.Select(t => t.Name);
            string tostring = select.ToString();
            string waitstring = "<Fields: Name>";
            Assert.AreEqual(tostring, waitstring);
        }
    }

    [TestFixture]
    public class UnitTestQDataDetail
    {
        [Test]
        public void DataDetailTests()
        {
            var regexDetails = new QDataDetail<QTask>();
            regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.And(new[]
                    {
                        QFilter<QTask>.Or(
                            QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                            QFilter<QTask>.Eq(t => t.State, QTaskStates.Cancelled),
                            QFilter<QTask>.Eq(t => t.State, QTaskStates.Failure),
                            QFilter<QTask>.In(t => t.State, new[] { QTaskStates.FullyExecuting, QTaskStates.PartiallyDispatched })),
                        QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                        QFilter<QTask>.Lte<uint>(t => t.InstanceCount, 10),
                    }),
                Select = QSelect<QTask>.Select()
                    .Include(t => t.Name)
                    .Include(t => t.Uuid)
                    .Include(t => t.State)
                    .Include(t => t.Status),
                MaximumResults = 2,
            };
            string teststring = $"{regexDetails}";
            regexDetails.Filter = QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted);
            regexDetails.Select = QSelect<QTask>.Select();
            regexDetails.MaximumResults = 5;
        }
    }

    [TestFixture]
    public class UnitTestQFilter
    {
        [Test]
        public void AndOrBigNestedTests()
        {
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.And(new[]
                    {
                        QFilter<QTask>.Or(
                            QFilter<QTask>.And(new[]
                            {
                                QFilter<QTask>.Or(new[]
                                {
                                    QFilter<QTask>.And(new[]
                                    {
                                        QFilter<QTask>.Or(new[]
                                        {
                                            QFilter<QTask>.And(new[]
                                            {
                                                QFilter<QTask>.Or(new[]
                                                {
                                                    QFilter<QTask>.And(new[]
                                                    {
                                                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                    }),
                                                    QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                    QFilter<QTask>.Or(new[]
                                                    {
                                                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                    }),
                                                    QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                    QFilter<QTask>.And(new[]
                                                    {
                                                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                        QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                    }),
                                                    QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                                                }),
                                                QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                                            }),
                                            QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                                        }),
                                        QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                                    }),
                                    QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                                }),
                                QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                            }),
                            QFilter<QTask>.Eq(t => t.State, QTaskStates.Submitted),
                            QFilter<QTask>.Eq(t => t.State, QTaskStates.Cancelled),
                            QFilter<QTask>.Eq(t => t.State, QTaskStates.Failure),
                            QFilter<QTask>.In(t => t.State, new[] { QTaskStates.FullyExecuting, QTaskStates.PartiallyDispatched })),
                        QFilter<QTask>.Eq<string>(t => t.Name, "sample11-task1"),
                        QFilter<QTask>.Lte<uint>(t => t.InstanceCount, 10),
                    }),
                Select = QSelect<QTask>.Select()
                    .Include(t => t.Status),
                MaximumResults = 2,
            };
        }

        [Test]
        public void AndFilterSimpleTest()
        {
            string value1 = "sample-name15489";
            string value2 = "sample-name87654";
            string subOp1 = "Equal";
            string subOp2 = "Like";
            string theOperator = "And";
            string field1 = "Name";
            string field2 = "Profile";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.And(
                    QFilter<QTask>.Eq(t => t.Name, value1),
                    QFilter<QTask>.Like(t => t.Profile, value2)),
            };
            var filters = (Node<QTask>)regexDetails.Filter._filterApi;
            UnitValueLeaf<QTask, string> firstValueAdd = (UnitValueLeaf<QTask, string>)filters.Filters[0];
            UnitValueLeaf<QTask, string> secondValueAdd = (UnitValueLeaf<QTask, string>)filters.Filters[1];
            Assert.AreEqual(filters.Operator, theOperator);
            Assert.AreEqual(firstValueAdd.Value, value1);
            Assert.AreEqual(secondValueAdd.Value, value2);
            Assert.AreEqual(firstValueAdd.Operator, subOp1);
            Assert.AreEqual(secondValueAdd.Operator, subOp2);
            Assert.AreEqual(firstValueAdd.Field, field1);
            Assert.AreEqual(secondValueAdd.Field, field2);
        }

        [Test]
        public void OrFilterSimpleTest()
        {
            string value1 = "sample-name15489";
            string value2 = "sample-name87654";
            string subOp1 = "Equal";
            string subOp2 = "Like";
            string theOperator = "Or";
            string field1 = "Name";
            string field2 = "Profile";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Or(
                    QFilter<QTask>.Eq(t => t.Name, value1),
                    QFilter<QTask>.Like(t => t.Profile, value2)),
            };
            var filters = (Node<QTask>)regexDetails.Filter._filterApi;
            UnitValueLeaf<QTask, string> firstValueAdd = (UnitValueLeaf<QTask, string>)filters.Filters[0];
            UnitValueLeaf<QTask, string> secondValueAdd = (UnitValueLeaf<QTask, string>)filters.Filters[1];
            Assert.AreEqual(filters.Operator, theOperator);
            Assert.AreEqual(firstValueAdd.Value, value1);
            Assert.AreEqual(secondValueAdd.Value, value2);
            Assert.AreEqual(firstValueAdd.Operator, subOp1);
            Assert.AreEqual(secondValueAdd.Operator, subOp2);
            Assert.AreEqual(firstValueAdd.Field, field1);
            Assert.AreEqual(secondValueAdd.Field, field2);
        }

        [Test]
        public void EqFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "Equal";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Eq(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void EqFilterArrayValues()
        {
            string value = "tag_45";
            string theOperator = "Equal";
            string field = "Tags";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Contains(t => t.Tags, value),
            };
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void NeqFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "NotEqual";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Neq(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void NeqFilterArrayValues()
        {
            string value = "tag_45";
            string theOperator = "NotEqual";
            string field = "Tags";
            var regexDetails = new QDataDetail<QTask>() { Filter = QFilter<QTask>.NotContains(t => t.Tags, value) };
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void InFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "In";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.In(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((MultipleValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Values[0], value);
            Assert.AreEqual(((MultipleValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((MultipleValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void NinFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "NotIn";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Nin(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((MultipleValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Values[0], value);
            Assert.AreEqual(((MultipleValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((MultipleValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void LtFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "LessThan";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Lt(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void LteFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "LessThanOrEqual";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Lte(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void GteFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "GreaterThanOrEqual";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Gte(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void LikeFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "Like";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Like(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }

        [Test]
        public void GtFilterSimpleTest()
        {
            int max = 2;
            string value = "sample-name15489";
            string theOperator = "GreaterThan";
            string field = "Name";
            var regexDetails = new QDataDetail<QTask>()
            {
                Filter = QFilter<QTask>.Gt(t => t.Name, value),
                MaximumResults = max,
            };
            Assert.AreEqual(regexDetails.MaximumResults, max);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Value, value);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Operator, theOperator);
            Assert.AreEqual(((UnitValueLeaf<QTask, string>)regexDetails.Filter._filterApi).Field, field);
        }
    }
}
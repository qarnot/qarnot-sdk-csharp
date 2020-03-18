namespace QarnotSDK.UnitTests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class QBucketTests
    {
        [Test]
        public void GetBucketsFromResourcesShouldFilterNullBucket()
        {
            List<QBucket> astorage = new List<QBucket>();
            astorage.Add(new QBucket());
            astorage.Add(null);
            astorage.Add(new QBucket());
            astorage.Add(null);
            var buckets = QBucket.GetBucketsFromResources(astorage);
            Assert.IsTrue(buckets.Count == 2);
        }

        [Test]
        public void GetBucketsFromResourcesShouldNotFailWithAnEmptyList()
        {
            List<QBucket> buckets = QBucket.GetBucketsFromResources(null);
            Assert.IsEmpty(buckets);
        }

        [Test]
        public async Task QBucketCreateAsyncShouldContainsTheCorrectShortname()
        {
            var shortname = "mybucketshortname";
            Connection connect = new Connection("https://localhost/", "token");
            QBucket bucket = await QBucket.CreateAsync(connect, shortname, false);
            Assert.AreEqual(shortname, bucket.Shortname);
        }

        [Test]
        public async Task QBucketInitializeAsyncShouldContainsTheCorrectShortname()
        {
            var shortname = "mybucketshortname";
            Connection connect = new Connection("https://localhost/", "token");

            var bucket = new QBucket();
            bucket = await bucket.InitializeAsync(connect, shortname, false);
            Assert.AreEqual(shortname, bucket.Shortname);
        }
    }
}

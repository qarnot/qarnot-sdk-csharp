namespace QarnotSDK.UnitTests
{
    using System.IO;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture("QBucketEntryName")]
    public class QBucketEntryTests : QBucketEntry
    {
        private const string EMPTY_FILE_DIGEST = "d41d8cd98f00b204e9800998ecf8427e";

        public QBucketEntryTests(string directoryPath)
            : base(directoryPath)
        {
        }

        public async Task<bool> ComputeLocalFileDigest(string localFilePath)
        {
            return await EqualsLocalFileDigestAsync(localFilePath);
        }

        [Test]
        public async Task EnsureLocalEmptyFileDigestIsCorrect()
        {
            var emptyFilename = Path.GetRandomFileName();
            using (File.Create(emptyFilename))
            {
            }

            Assert.False(await ComputeLocalFileDigest(emptyFilename));
            Digest = EMPTY_FILE_DIGEST;
            Assert.True(await ComputeLocalFileDigest(emptyFilename));
        }
    }
}

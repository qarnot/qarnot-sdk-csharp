namespace QarnotSDK.UnitTests
{
    using System;
    using NUnit.Framework;
    using QarnotSDK;

    [TestFixture]
    public class AdvancedRangesTests
    {
        [Test]
        public void AdvancedRangesWithOneValueShouldWork()
        {
            AdvancedRanges test = new AdvancedRanges("1");
            uint count_values = test.Count;
            Assert.True(count_values == 1);
            Assert.IsTrue(test.ToString().Equals("1", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldAllowOverlappingRanges()
        {
            AdvancedRanges range = new AdvancedRanges(" 1 - 100  , 5 ,     9  - 99    ,0-1");
            Assert.That(range.Count, Is.EqualTo(99 + 1 + 90 + 1));
            Assert.IsTrue(range.ToString().Equals("1-100,5,9-99,0", StringComparison.Ordinal));
            int[] max = new int[] { 100, 5, 99, 1, 0 };
            int[] min = new int[] { 1, 5, 9, 0, 0 };
            int j = 0;
            int i = min[j];
            foreach (int elem in range)
            {
                Assert.That(elem, Is.EqualTo(i));
                i++;
                if (i >= max[j])
                {
                    j++;
                    i = min[j];
                }
            }
        }

        [Test]
        public void AdvancedRangesShouldAllowEmptyRange()
        {
            AdvancedRanges test = new AdvancedRanges(string.Empty);
            uint count_values = test.Count;
            Assert.True(test.Empty);
            Assert.That(test.ToString(), Is.EqualTo(string.Empty));
            test = new AdvancedRanges(null);
            count_values = test.Count;
            Assert.True(test.Empty);
            Assert.That(test.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void AdvancedRangesShouldAllowOneNonIntergeValue()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("test"));
            Assert.IsTrue(ex.Message.Contains("Non-integer number found", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldNotAllowNonNumberWithHyphen()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("te-st"));
            Assert.IsTrue(ex.Message.Contains("Non-integer number found", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldNotAllowMalformedRange()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("0--"));
            Assert.IsTrue(ex.Message.Contains("Range should have 2 bounds min-max instead of", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldNotAllowSingleComma()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges(","));
            Assert.IsTrue(ex.Message.Contains("Non-integer number found", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldAllowSingleHyphen()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("-"));
            Assert.IsTrue(ex.Message.Contains("Non-integer number found", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldNotContainWhitespaceString()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges(" "));
            Assert.IsTrue(ex.Message.Contains("Non-integer number found", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldNotContainHexadecimalNumber()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("0xa-0xb"));
            Assert.IsTrue(ex.Message.Contains("Non-integer number found", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldContainRangesWithAscendingLimits()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("5-2"));
            Assert.IsTrue(ex.Message.Contains("Range of the form ", StringComparison.Ordinal));
        }

        [Test]
        public void AdvancedRangesShouldContainsSeparatedRanges()
        {
            Exception ex = Assert.Throws<Exception>(() => new AdvancedRanges("1-2-3-4"));
            Assert.IsTrue(ex.Message.Contains("Range should have 2 bounds min-max instead of ", StringComparison.Ordinal));
        }
    }
}

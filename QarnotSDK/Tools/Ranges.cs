using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QarnotSDK {
    internal struct Range {
        public Range(UInt32 min, UInt32 max) { Min = min;  Max = max; }
        public Range(UInt32 val) { Min = val; Max = val+1; }

        public UInt32 Min { get; internal set; }

        public UInt32 Max { get; internal set; }

        public UInt32 Length { get { return Max > Min ? Max - Min : 0; } }
    }

    /// <summary>
    /// Represents a complex range (non continuous, with an offset...) of instances to execute.
    /// </summary>
    public class AdvancedRanges : IEnumerable<UInt32> {
        /// <summary>
        /// Construct a new AdvanedRange object.
        /// </summary>
        /// <param name="ranges">A string in the advanced range format: "1,5-10,15"</param>
        public AdvancedRanges(string ranges) {
            if (String.IsNullOrEmpty(ranges)) return;

            // First, remove all whitespace, so that users can put some wherever they want
            string trimmed = Regex.Replace(ranges, @"\s+", "");

            var parts = trimmed.Split(',');
            foreach (var part in parts) {
                if (part.Contains("-")) {
                    // It's an interval
                    var bounds = part.Split('-');
                    uint begin = 0;
                    uint end = 0;

                    if (!uint.TryParse(bounds[0], out begin)) {
                        throw new Exception("Non-integer number found in range: " + bounds[0]);
                    }

                    if (!uint.TryParse(bounds[1], out end)) {
                        throw new Exception("Non-integer number found in range: " + bounds[1]);
                    }

                    if (begin > end) {
                        throw new Exception("Range of the form min-max must have min <= max");
                    }

                    _ranges.Add(new Range(begin, end)); // Constructor is start, length
                } else {
                    // It's a singleton
                    uint val = 0;
                    if (!uint.TryParse(part, out val)) {
                        throw new Exception("Non-integer number found in range: " + part);
                    }

                    _ranges.Add(new Range(val));
                }
            }
        }

        private List<Range> _ranges = new List<Range>();

        /// <summary>
        /// True if this range is empty.
        /// </summary>
        public virtual bool Empty {
            get { return Count == 0; }
        }

        /// <summary>
        /// Returns the number of instance ids in this range.
        /// </summary>
        public virtual UInt32 Count {
            get {
                UInt32 count = 0;
                foreach (var r in _ranges) {
                    count += r.Length;
                }
                return count;
            }
        }

        /// <summary>
        /// Outputs this range in the advanced range format.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            var sb = new StringBuilder();

            bool first = true;
            foreach (var r in _ranges) {
                if (first) first = false;
                else sb.Append(",");

                if (r.Length == 1) {
                    sb.AppendFormat("{0}", r.Min);
                } else {
                    sb.AppendFormat("{0}-{1}", r.Min, r.Max);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Iterates though all the instance ids in this range.
        /// </summary>
        /// <returns>An iterator on the instance ids in this range.</returns>
        IEnumerator<UInt32> IEnumerable<UInt32>.GetEnumerator() {
            foreach (var cr in _ranges) {
                for (uint i = cr.Min; i < cr.Max; i++) {
                    yield return i;
                }
            }
        }

        /// <summary>
        /// Iterates though all the instance ids in this range.
        /// </summary>
        /// <returns>An iterator on the instance ids in this range.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            foreach (var cr in _ranges) {
                for (uint i = cr.Min; i < cr.Max; i++) {
                    yield return i;
                }
            }
        }
    }
}

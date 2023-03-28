namespace QarnotSDK {

    using System.Collections.Generic;

    // This class is used to serialize constants in pools and tasks
    internal class KeyValHelper {
        public string Key { get; set; }

        public string Value { get; set; }

        internal KeyValHelper(string key, string value) {
            Key = key;
            Value = value;
        }

        internal KeyValHelper() {
        }
    }

    // This class is used to serialize errors
    internal class Error {
        public string Message { get; set; }
        public ProblemDetailsWithErrors ProblemDetails { get; set; }

        internal Error(string msg, ProblemDetailsWithErrors problemDetails=null) {
            Message = msg;
            ProblemDetails = problemDetails;
        }

        internal Error() {
        }
    }


    internal class ProblemDetailsWithErrors
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string TraceId { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }


    // Common to tasks and pools

    internal class ApiBucketFilteringPrefix {
        public string Prefix { get; set; }
    }

    internal class ApiBucketFiltering {
        public ApiBucketFilteringPrefix PrefixFiltering { get; set; }

        // If other types of filtering are added (e.g PrefixList or Regex), they
        // should be added as new fields here.
    }

    internal class ApiResourcesTransformationStripPrefix {
        public string Prefix { get; set; }
    }

    internal class ApiResourcesTransformation {
        public ApiResourcesTransformationStripPrefix StripPrefix { get; set; }

        // If other types of transformations are added (e.g PrefixReplace), they
        // should be added as new fields here.
    }


    internal class ApiAdvancedResourceBucket
    {
        public string BucketName { get; set; }
        public ApiBucketFiltering Filtering { get; set; }
        public ApiResourcesTransformation ResourcesTransformation { get; set; }
        public int? CacheTTLSec { get; set; }
    }


}

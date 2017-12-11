using System;

namespace QarnotSDK
{
    /// <summary>
    /// Represents errors that occur during an Api request.
    /// </summary>
    public class QarnotApiException : Exception {
        internal QarnotApiException(string error, Exception inner) : base(error, inner) { }
    }

    /// <summary>
    /// Represents errors 404 that occur during an Api request.
    /// </summary>
    public class QarnotApiResourceNotFoundException : QarnotApiException {
        internal QarnotApiResourceNotFoundException(string error, Exception inner) : base(error, inner) { }
    }

    /// <summary>
    /// Represents errors that occur when a shortname is not unique.
    /// </summary>
    public class QarnotApiResourceAlreadyExistsException : QarnotApiException {
        internal QarnotApiResourceAlreadyExistsException(string error, Exception inner) : base(error, inner) { }
    }
}


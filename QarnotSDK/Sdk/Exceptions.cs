using System;

namespace QarnotSDK
{
    /// <summary>
    /// Represents errors that occur during an Api request.
    /// </summary>
    public class QarnotApiException : Exception {
        /// <summary>
        /// Could contain extra information to help debugging.
        /// </summary>
        public object DebugHelper { get; private set; }
        internal QarnotApiException(string error, Exception inner, object debugObject = null) : base(error, inner) {
            DebugHelper = debugObject;
        }
    }

    /// <summary>
    /// Represents errors 404 that occur during an Api request.
    /// </summary>
    public class QarnotApiResourceNotFoundException : QarnotApiException {
        internal QarnotApiResourceNotFoundException(string error, Exception inner, object debugObject = null) : base(error, inner, debugObject) { }
    }

    /// <summary>
    /// Represents errors that occur when a shortname is not unique.
    /// </summary>
    public class QarnotApiResourceAlreadyExistsException : QarnotApiException {
        internal QarnotApiResourceAlreadyExistsException(string error, Exception inner, object debugObject = null) : base(error, inner, debugObject) { }
    }
}


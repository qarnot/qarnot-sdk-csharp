using System;

namespace QarnotSDK
{
    public class QarnotApiException : Exception {
        public QarnotApiException(string error, Exception inner) : base(error, inner) { }
    }

    public class QarnotApiResourceNotFoundException : QarnotApiException {
        public QarnotApiResourceNotFoundException(string error, Exception inner) : base(error, inner) { }
    }

    public class QarnotApiResourceAlreadyExistsException : QarnotApiException {
        public QarnotApiResourceAlreadyExistsException(string error, Exception inner) : base(error, inner) { }
    }

    public class PoolFailedException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        public PoolFailedException() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public PoolFailedException(string message) : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public PoolFailedException(string message, Exception inner) : base(message, inner) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected PoolFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }

    public class TaskFailedException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        public TaskFailedException() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        public TaskFailedException(string message) : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public TaskFailedException(string message, Exception inner) : base(message, inner) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MyException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected TaskFailedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }
    }
}


using System;
using System.Runtime.Serialization;

namespace AppBoot.Repos.Exceptions
{
    /// <summary> Represents an exception in casting sessions. </summary>
    [Serializable]
    public class InvalidSessionTypeException : RepositoryException
    {
        public InvalidSessionTypeException()
        {
        }

        public InvalidSessionTypeException(Type expectedType, Type actualType)
            : this(expectedType, actualType, null)
        {
        }

        public InvalidSessionTypeException(Type expectedType, Type actualType, Exception innerException)
            : this(CreateMessage(expectedType, actualType), innerException)
        {
        }

        public InvalidSessionTypeException(string message)
            : this(message, null)
        {
        }

        public InvalidSessionTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidSessionTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary> InternalForTestOnly. </summary>
        internal const String m_ErrorFmt = "Unexpected session type [{0}] while expecting [{1}].";

        private static String CreateMessage(Type expectedType, Type actualType)
        {
            if (expectedType == null) throw new ArgumentNullException("expectedType");
            if (actualType == null) throw new ArgumentNullException("actualType");

            return String.Format(m_ErrorFmt, actualType, expectedType);
        }
    }
}
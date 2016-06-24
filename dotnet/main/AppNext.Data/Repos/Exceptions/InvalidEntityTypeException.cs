using System;
using System.Runtime.Serialization;

namespace AppBoot.Repos.Exceptions
{
    /// <summary> Represents an exception in casting entities. </summary>
    [Serializable]
    public class InvalidEntityTypeException : RepositoryException
    {
        public InvalidEntityTypeException()
        {
        }

        public InvalidEntityTypeException(Type expectedType, Type actualType)
            : this(expectedType, actualType, null)
        {
        }

        public InvalidEntityTypeException(Type expectedType, Type actualType, Exception innerException)
            : this(CreateMessage(expectedType, actualType), innerException)
        {
        }

        public InvalidEntityTypeException(string message)
            : this(message, null)
        {
        }

        public InvalidEntityTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidEntityTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary> InternalForTestOnly. </summary>
        internal const String m_ErrorFmt = "Unexpected entity type [{0}] while expecting [{1}].";

        private static String CreateMessage(Type expectedType, Type actualType)
        {
            if (expectedType == null) throw new ArgumentNullException("expectedType");
            if (actualType == null) throw new ArgumentNullException("actualType");

            return String.Format(m_ErrorFmt, actualType, expectedType);
        }
    }
}
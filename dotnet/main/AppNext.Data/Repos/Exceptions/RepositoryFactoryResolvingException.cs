using System;
using System.Runtime.Serialization;

namespace AppBoot.Repos.Exceptions
{
    /// <summary> Represents an exception in resolving repository factories. </summary>
    [Serializable]
    public class RepositoryFactoryResolvingException : RepositoryException
    {
        public RepositoryFactoryResolvingException()
        {
        }

        public RepositoryFactoryResolvingException(Type repositoryType)
            : this(CreateMessage(repositoryType))
        {
        }

        public RepositoryFactoryResolvingException(String message)
            : base(message, null)
        {
        }

        public RepositoryFactoryResolvingException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected RepositoryFactoryResolvingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary> InternalForTestOnly. </summary>
        internal const String m_ErrorFmt = "Cannot resolve repository factory of type [{0}].";

        private static String CreateMessage(Type repositoryType)
        {
            if (repositoryType == null) throw new ArgumentNullException("repositoryType");

            return String.Format(m_ErrorFmt, repositoryType);
        }
    }
}
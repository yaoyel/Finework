using System;
using System.Runtime.Serialization;

namespace AppBoot.Repos.Exceptions
{
    /// <summary> Represents an exception in resolving repositories. </summary>
    [Serializable]
    public class RepositoryResolvingException : RepositoryException
    {
        public RepositoryResolvingException()
        {
        }

        public RepositoryResolvingException(string message)
            : this(message, null)
        {
        }

        public RepositoryResolvingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected RepositoryResolvingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
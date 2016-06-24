using System;
using System.Runtime.Serialization;

namespace AppBoot.Repos.Exceptions
{
    /// <summary> Represents the base exception type of all repository exceptions. </summary>
    [Serializable]
    public class RepositoryException : Exception
    {
        public RepositoryException()
        {
        }

        public RepositoryException(string message)
            : this(message, null)
        {
        }

        public RepositoryException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected RepositoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

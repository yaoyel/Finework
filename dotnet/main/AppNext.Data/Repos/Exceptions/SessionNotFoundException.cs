using System;
using System.Runtime.Serialization;

namespace AppBoot.Repos.Exceptions
{
    /// <summary> Represents an exception that the <see cref="ISession"/> is not found. </summary>
    [Serializable]
    public class SessionNotFoundException : RepositoryException
    {
        public SessionNotFoundException()
        {
        }

        public SessionNotFoundException(string message)
            : this(message, null)
        {
        }

        public SessionNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SessionNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

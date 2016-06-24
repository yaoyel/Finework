using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace FineWork.Common
{
    /// <summary> The base class for all finework exceptions. </summary>
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class FineWorkException : Exception, IFriendlyMessage
    {
        /// <summary> Creates an instance. </summary>
        public FineWorkException()
        {
        }

        /// <summary> Creates an instance. </summary>
        public FineWorkException(String message)
            : base(message)
        {
        }

        /// <summary> Creates an instance. </summary>
        public FineWorkException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary> Creates an instance from serialization data. </summary>
        protected FineWorkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private String m_FriendlyMessage;

        /// <summary> 用户友好的信息. </summary>
        public String FriendlyMessage
        {
            get { return m_FriendlyMessage ?? this.Message; }
            set { m_FriendlyMessage = value; }
        }
    }
}

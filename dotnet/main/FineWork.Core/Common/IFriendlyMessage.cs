using System;

namespace FineWork.Common
{
    public interface IFriendlyMessage
    {
        String FriendlyMessage { get; set; }
    }

    public static class FriendlyMessageExtensions
    {
        /// <summary> 通过 fluent API 设置 <see cref="FineWorkException.FriendlyMessage"/>. </summary>
        public static T FriendlyMessage<T>(this T exception, String friendlyMessage) where T : IFriendlyMessage
        {
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            exception.FriendlyMessage = friendlyMessage;
            return exception;
        }
    }
}
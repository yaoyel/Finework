using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class ConversationExistsResult : FineWorkCheckResult
    {
        public ConversationExistsResult(bool isSucceed, String message, ConversationEntity conversation)
            : base(isSucceed, message)
        {
            this.Conversation = conversation;
        }

        public ConversationEntity Conversation { get; set; }
        public static ConversationExistsResult Check(IConversationManager conversationManager,string convrId)
        {
            var convr = conversationManager.FindById(convrId);
            return Check(convr, null);
        }

       
        private static ConversationExistsResult Check([CanBeNull]  ConversationEntity convr, String message)
        {
            if (convr == null)
            {
                return new ConversationExistsResult(false, message, null);
            }

            return new ConversationExistsResult(true, null, convr);
        }
    }
}

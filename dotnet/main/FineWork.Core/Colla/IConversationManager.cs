using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IConversationManager
    {
        ConversationEntity CreateConversation(string conversationId,bool? isUnique=null);

        ConversationEntity FindById(string conversationId);

        ConversationEntity FindConversationByStaffIds(Guid taskId, params Guid[] staffIds);

        IEnumerable<ConversationEntity> FetchConvertionsByStaffId(Guid staffId, bool isIncludeUnique = false);

        void UpdateConversation(ConversationEntity conv);

        dynamic GetConversatonAttrs(string conversationId);
    }
}
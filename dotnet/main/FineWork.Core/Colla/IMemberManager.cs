using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IMemberManager
    {
        IEnumerable<MemberEntity> CreateMember(string conversationId, params Guid[] staffIds);

        IEnumerable<MemberEntity> FetchMembersByConversationId(string conversationId);

        void DeleteMember(string conversationId, params Guid[] staffIds);

        MemberEntity FindByStaffIdAndConvrId(Guid staffId, string convrId);

        void ClearLog(string convId, Guid staffId);

        IEnumerable<MemberEntity> FetchMembersByStaffId(Guid staffId);
    }
}
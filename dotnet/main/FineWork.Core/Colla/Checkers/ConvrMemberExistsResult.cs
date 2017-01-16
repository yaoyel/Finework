using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class ConvrMemberExistsResult:FineWorkCheckResult
    {
        public ConvrMemberExistsResult(bool isSucceed, String message, MemberEntity member)
            : base(isSucceed, message)
        {
            this.Member = member;
        }

        public MemberEntity  Member { get; set; }
        public static ConvrMemberExistsResult Check(IMemberManager conversationMemberManager, Guid staffId,string convrId)
        {
            var member = conversationMemberManager.FindByStaffIdAndConvrId(staffId,convrId);
            return Check(member, null);
        }
 

        private static ConvrMemberExistsResult Check([CanBeNull]  MemberEntity member, String message)
        {
            if (member == null)
            {
                return new ConvrMemberExistsResult(false, message, null);
            }

            return new ConvrMemberExistsResult(true, null, member);
        }
    }
}

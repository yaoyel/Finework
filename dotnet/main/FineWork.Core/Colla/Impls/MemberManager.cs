using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using System.Data.Entity;
namespace FineWork.Colla.Impls
{
    public class MemberManager : AefEntityManager<MemberEntity, Guid>, IMemberManager
    {
        public MemberManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IConversationManager conversationManager)
            : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
        
            m_StaffManager = staffManager;
            m_ConversationManager = conversationManager; 
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IConversationManager m_ConversationManager; 
        public IEnumerable<MemberEntity> CreateMember(string conversationId, params Guid[] staffIds)
        { 
            var convr =
                ConversationExistsResult.Check(m_ConversationManager, conversationId).ThrowIfFailed().Conversation;

            var result=new List<MemberEntity>();
            if (staffIds.Any())
            {
                foreach (var staffId in staffIds)
                {
                    var member = ConvrMemberExistsResult.Check(this, staffId, conversationId).Member;
                    if (member == null)
                    {
                        var staff = StaffExistsResult.Check(m_StaffManager, staffId).ThrowIfFailed().Staff;

                        var conversationMember = new MemberEntity();
                        conversationMember.Conversation = convr;
                        conversationMember.Id = Guid.NewGuid();
                        conversationMember.Staff = staff;
                        this.InternalInsert(conversationMember);
                        result.Add(conversationMember);
                    }
                }
            } 
            return result;
        }
         
        public IEnumerable<MemberEntity> FetchMembersByConversationId(string conversationId)
        {
            return this.InternalFetch(p => p.Conversation.Id == conversationId);
        } 

        public void DeleteMember(string conversationId, params Guid[] staffIds)
        {
            if (staffIds.Any())
            {
                foreach (var staffId in staffIds)
                {
                    var member = ConvrMemberExistsResult.Check(this, staffId, conversationId).Member;
                    if(member!=null)
                        this.InternalDelete(member);
                } 
            }
        }

        public MemberEntity FindByStaffIdAndConvrId(Guid staffId, string convrId)
        {
            return this.InternalFetch(p => p.Conversation.Id == convrId && p.Staff.Id == staffId).FirstOrDefault();
        }

        public List<Tuple<string, Guid[]>> FetchConvrMembersByStaffIds(params Guid[] staffIds)
        {
            if (!staffIds.Any()) return null;

            var result = this.InternalFetch(p => staffIds.Contains(p.Staff.Id)).GroupBy(p => p.Conversation.Id)
                .Select(p => new Tuple<string, Guid[]>(p.Key, p.Select(s => s.Staff.Id).ToArray())).ToList();
            return result;
        } 

        public string FindConversationIdByStaffIds( Guid[] staffIds)
        {
            var convrMembers = this.FetchConvrMembersByStaffIds(staffIds);

            if (convrMembers.Any())
                foreach (var conver in convrMembers)
                {
                    if (!conver.Item2.Except(staffIds).Any() && !staffIds.Except(conver.Item2).Any())
                        return conver.Item1;
                }
            return string.Empty;
        }

        public void ClearLog(string convId, Guid staffId)
        {
            var members = this.InternalFetch(p => p.Conversation.Id == convId && p.Staff.Id == staffId).ToList();
            if (members.Any())
                members.ForEach(p =>
                {
                    p.ClearLogAt = DateTime.Now;
                    this.InternalUpdate(p);
                });

        } 

        public IEnumerable<MemberEntity> FetchMembersByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId && (p.Conversation.IsEnabled == null || p.Conversation.IsEnabled.Value));
        }
    }
}
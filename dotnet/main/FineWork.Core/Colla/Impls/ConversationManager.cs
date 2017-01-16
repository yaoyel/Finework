using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class ConversationManager : AefEntityManager<ConversationEntity, string>, IConversationManager
    {
        public ConversationManager(ISessionProvider<AefSession> sessionProvider,
            ILazyResolver<ITaskManager> taskManageResolver,
            ILazyResolver<ITaskAlarmManager> taskAlararmManagerResolver ) : base(sessionProvider)
        {
            Args.NotNull(taskManageResolver, nameof(taskManageResolver));
            Args.NotNull(taskAlararmManagerResolver, nameof(taskAlararmManagerResolver));

            m_TaskManageResolver = taskManageResolver;
            m_TaskAlarmManagerResolver = taskAlararmManagerResolver;
        }


        private readonly ILazyResolver<ITaskManager> m_TaskManageResolver;
        private readonly ILazyResolver<ITaskAlarmManager> m_TaskAlarmManagerResolver;
        
        private ITaskManager TaskManager {
            get { return m_TaskManageResolver.Required; }
        }

        private ITaskAlarmManager TaskAlarmManager {
            get { return m_TaskAlarmManagerResolver.Required; }
        }

        public ConversationEntity CreateConversation(string conversationId,bool? isUnique=null)
        {
            Args.NotNull(conversationId, nameof(conversationId));

            var convr = ConversationExistsResult.Check(this, conversationId).Conversation;

            if (convr != null) return convr;

            convr = new ConversationEntity();
            convr.Id = conversationId;
            convr.IsUnique = isUnique;
            this.InternalInsert(convr);
            return convr;
        }

        public ConversationEntity FindById(string conversationId)
        {
            return this.InternalFind(conversationId);
        }

        public ConversationEntity FindConversationByStaffIds(Guid taskId,params Guid[] staffIds)
        {
            return
                this.InternalFetch(
                    p =>p.IsUnique==null && p.TaskAlarms.Any(a=>a.Task.Id==taskId) &&
                        !p.Members.Select(s => s.Staff.Id).Except(staffIds).Any() &&
                        !staffIds.Except(p.Members.Select(m => m.Staff.Id)).Any()).FirstOrDefault();
        }

        public IEnumerable<ConversationEntity> FetchConvertionsByStaffId(Guid staffId,bool isIncludeUnique=false)
        {
            return this.InternalFetch(p =>p.IsUnique==isIncludeUnique && p.Members.Any(a => a.Staff.Id == staffId));
        }

        public void UpdateConversation(ConversationEntity conv)
        {
            if(conv!=null)
                this.InternalUpdate(conv);
        }

        public dynamic GetConversatonAttrs(string conversationId)
        {
            dynamic result = null;
            var conversation = ConversationExistsResult.Check(this, conversationId);
            if (conversation != null)
            {
                var task = this.TaskManager.FindByConvrId(conversationId);
                if (task != null)
                {
                    result.TaskId = task.Id;
                    result.TaskName = task.Name;
                    result.Progress = task.Progress;
                    result.Creator = task.Creator.Id;
                    result.LeaderStaffId = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
                    result.ConversationId = task.ConversationId;
                    result.IsDeserted = task.IsDeserted;
                    result.IsEnd = task.Progress == 100;
                }

                var alarms = this.TaskAlarmManager.FetchAlarmsByConversationId(conversationId).ToList();

                if (alarms.Any())
                {
                    result.AlarmCount = alarms.Count(p => p.ResolveStatus != ResolveStatus.Closed);
                    result.ResolvedCount = alarms.Count(p => p.ResolveStatus == ResolveStatus.Closed);
                } 
            }

            return result;
        }
    }
}
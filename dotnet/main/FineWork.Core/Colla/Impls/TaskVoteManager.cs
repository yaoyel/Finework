using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Core; 
using System.Data.Entity;
using System.Threading.Tasks;
using AppBoot.Checks;
using FineWork.Colla.Checkers;
using FineWork.Message;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class TaskVoteManager : AefEntityManager<TaskVoteEntity, Guid>, ITaskVoteManager
    {
        public TaskVoteManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager
            ,ILazyResolver<IVoteManager>  voteManageLazyResolver,
            IStaffManager staffManager,
            ITaskLogManager taskLogManager,
            IIMService imService,
            IConfiguration config,
            IPushLogManager pushLogManager,
            INotificationManager notificationManager)
            : base(sessionProvider)
        {
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(voteManageLazyResolver, nameof(voteManageLazyResolver));

            m_TaskManager = taskManager;
            m_VoteManagerLazyResolver = voteManageLazyResolver;
            m_StaffManager = staffManager;
            m_TaskLogManager = taskLogManager;
            m_IMService = imService;
            m_Config = config;
            m_PushLogManager = pushLogManager;
            m_NotificationManager = notificationManager; 
        }

        private readonly ITaskManager m_TaskManager;
        private readonly ILazyResolver<IVoteManager> m_VoteManagerLazyResolver;
        private readonly IStaffManager m_StaffManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly IPushLogManager m_PushLogManager;
        private readonly INotificationManager m_NotificationManager;

        private IVoteManager VoteManager
        {
            get { return m_VoteManagerLazyResolver.Required; }
        }

        public TaskVoteEntity CreateTaskVote(Guid taskId, Guid staffId, Guid voteId)
        {
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            var vote = VoteExistsResult.Check(VoteManager, voteId).ThrowIfFailed().Vote;
            var staff = StaffExistsResult.Check(m_StaffManager, staffId).ThrowIfFailed().Staff;

            var result=this.InternalCreateTaskVote(task, vote);
            //记录日志
            var imMessage = $"创建了一个共识"; 

            m_TaskLogManager.CreateTaskLog(task.Id, staff.Id, vote.GetType().FullName, vote.Id, ActionKinds.InsertTable, imMessage);

            var imMesasge = string.Format(m_Config["LeanCloud:Messages:Task:Vote"], staff.Name);
            m_IMService.SendTextMessageByConversationAsync(task.Id, vote.Creator.Account.Id, task.Conversation.Id, task.Name, imMesasge); 
            return result;
        }

        private TaskVoteEntity InternalCreateTaskVote(TaskEntity task, VoteEntity vote)
        {
            Args.NotNull(task, nameof(task));
            Args.NotNull(vote, nameof(vote));

            var taskVoteEntity=new TaskVoteEntity();
            taskVoteEntity.Id=Guid.NewGuid();
            taskVoteEntity.Task = task;
            taskVoteEntity.Vote = vote;

            this.InternalInsert(taskVoteEntity);
            return taskVoteEntity; 
        }

        public TaskVoteEntity FineVoteById(Guid voteId)
        {
            return this.InternalFetch(p => p.Vote.Id == voteId).FirstOrDefault();
        }

        public IEnumerable<TaskVoteEntity> FetchVoteByTaskId(Guid taskId)
        {

             return this.InternalFetch(s => s.Where(p => p.Task.Id == taskId)
             .Include(i => i.Vote.VoteOptions.Select(e => e.Votings))); 
        }

        public IEnumerable<TaskVoteEntity> FetchAllVotes()
        {
            return this.InternalFetchAll();
        }

        public IEnumerable<TaskVoteEntity> FetchVotesByTime(DateTime time)
        {
            var context = this.Session.DbContext;
            var set = context.Set<TaskVoteEntity>().Include(p=>p.Task.Partakers.Select(s=>s.Staff.Account))
                .Include(p => p.Task.Partakers.Select(s => s.Staff.Org))
                .Include(p => p.Vote)
                .AsNoTracking().AsEnumerable();

            //换算成东八区时间 
            TimeZoneInfo local = TimeZoneInfo.Local;
            time = time.AddHours(8 - local.BaseUtcOffset.Hours);

            var timeOfDay = time.TimeOfDay;

            var timeFormat = new DateTime(time.Year, time.Month, time.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);

            return
                set.Where(p => p.Vote.StartAt == timeFormat).ToList();
        } 
    }
}
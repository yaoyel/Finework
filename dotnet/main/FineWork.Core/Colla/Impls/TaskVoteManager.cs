﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Core; 
using System.Data.Entity;
using AppBoot.Checks;
using FineWork.Colla.Checkers;
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
            IConfiguration config)
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

        }

        private readonly ITaskManager m_TaskManager;
        private readonly ILazyResolver<IVoteManager> m_VoteManagerLazyResolver;
        private readonly IStaffManager m_StaffManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;

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
            var message = $"创建了一个共识";
            m_TaskLogManager.CreateTaskLog(task.Id, staff.Id, vote.GetType().FullName, vote.Id, ActionKinds.InsertTable, message);

            var imMesasge = string.Format(m_Config["LeanCloud:Messages:Task:Vote"], staff.Name);
            m_IMService.SendTextMessageByConversationAsync(task.Id, vote.Creator.Account.Id, task.ConversationId, task.Name, imMesasge);


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
    }
}
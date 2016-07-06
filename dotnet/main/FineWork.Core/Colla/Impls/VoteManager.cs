using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using System.Data.Entity;
using FineWork.Common;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class VoteManager : AefEntityManager<VoteEntity, Guid>, IVoteManager
    {
        public VoteManager(ISessionProvider<AefSession> dbContextProvider,
            IStaffManager staffManager,
            ITaskManager taskManager,
            IVoteOptionManager voteOptionManager,
            ITaskLogManager taskLogManager,
            IIMService imService, IConfiguration config)
            : base(dbContextProvider)
        {
            if (dbContextProvider == null) throw new ArgumentException(nameof(dbContextProvider));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));
            if (taskManager == null) throw new ArgumentException(nameof(taskManager));
            if (voteOptionManager == null) throw new ArgumentException(nameof(voteOptionManager));
            if (taskLogManager == null) throw new ArgumentException(nameof(taskLogManager));

            m_StaffManager = staffManager;
            m_TaskManager = taskManager;
            m_VoteOptionManager = voteOptionManager;
            m_TaskLogManager = taskLogManager;
            m_IMService = imService;
            m_Config = config;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IVoteOptionManager m_VoteOptionManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        public VoteEntity CreateVote(CreateVoteModel voteModel)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, voteModel.TaskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(this.m_StaffManager, voteModel.CreatorStaffId).ThrowIfFailed().Staff;

            //创建共识
            var vote = new VoteEntity()
            {
                Id = Guid.NewGuid(),
                Subject = voteModel.Subject,
                StartAt = voteModel.StartAt,
                EndAt = voteModel.EndAt,
                IsMultiEnabled = voteModel.IsMultiEnabled,
                IsAnonEnabled = voteModel.IsAnonEnabled,
                Task = task,
                Creator = staff
            };

            this.InternalInsert(vote);

            if (!voteModel.VoteOptions.Any()) throw new FineWorkException("请添加共识选项.");
            if (voteModel.VoteOptions.GroupBy(p => p.Content).Any(p => p.Count() > 1))
                throw new FineWorkException("共识选项不可以相同.");

            if (voteModel.VoteOptions.Any())
                //共识的选项
            { 
                voteModel.VoteOptions.ToList().ForEach(p =>
                {
                    var optionModel = new CreateVoteOptionModel
                    {
                        Content = p.Content,
                        IsNeedReason = p.IsNeedReason,
                        Order=p.Order
                    };
                    var voteOption = this.m_VoteOptionManager.CreateVoteOption(vote, optionModel);
                    vote.VoteOptions.Add(voteOption);
                });
            }

            //记录日志
            var message = $"创建了一个共识";
            m_TaskLogManager.CreateTaskLog(task.Id, staff.Id,  vote.GetType().FullName, vote.Id, ActionKinds.InsertTable,message);

            var imMesasge = string.Format(m_Config["LeanCloud:Messages:Task:Vote"], staff.Name);
            m_IMService.SendTextMessageByConversationAsync(task.Id, vote.Creator.Account.Id, task.ConversationId,task.Name,imMesasge);
            return vote;
           
        }  
        
        public IEnumerable<VoteEntity> FetchVotesByTaskId(Guid taskId, bool? isApproved)
        {
            var votes = this.InternalFetch(s => s.Where(p => p.Task.Id == taskId)
                .Include(i => i.VoteOptions.Select(e => e.Votings)));

            if (isApproved.HasValue)
                votes = votes.Where(p => p.IsApproved == isApproved.Value).ToList();
            else
                votes = votes.Where(p => p.IsApproved == null).ToList();

            return votes.Select(s=>new VoteEntity()
            {
                Id=s.Id,
                Subject=s.Subject,
                StartAt=s.StartAt,
                EndAt=s.EndAt,
                IsMultiEnabled=s.IsMultiEnabled,
                IsAnonEnabled=s.IsAnonEnabled,
                IsApproved=s.IsApproved,
                CreatedAt=s.CreatedAt,
                Creator=s.Creator,
                Task=s.Task,
                VoteOptions=s.VoteOptions.OrderBy(p=>p.Order).ToList()
            });
        }

        public VoteEntity FindVoteByVoteId(Guid voteId)
        {
            return this.InternalFind(voteId);
        }

        public void UpdateVoteApprovedStatus(VoteEntity vote, bool isApproved)
        {
            if (vote == null) throw new ArgumentException(nameof(vote));

            vote.IsApproved = isApproved;
            this.InternalUpdate(vote);
        }
         
     
    }
}

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
using AppBoot.Common;
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
            IIMService imService, IConfiguration config,
            ITaskVoteManager taskVoteManager)
            : base(dbContextProvider)
        {
            if (dbContextProvider == null) throw new ArgumentException(nameof(dbContextProvider));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));
            if (taskManager == null) throw new ArgumentException(nameof(taskManager));
            if (voteOptionManager == null) throw new ArgumentException(nameof(voteOptionManager));
            if (taskLogManager == null) throw new ArgumentException(nameof(taskLogManager));
            if (taskVoteManager == null) throw new ArgumentException(nameof(taskVoteManager));
            m_StaffManager = staffManager;
            m_TaskManager = taskManager;
            m_VoteOptionManager = voteOptionManager;
            m_TaskLogManager = taskLogManager;
            m_IMService = imService;
            m_Config = config;
            m_TaskVoteManager = taskVoteManager;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IVoteOptionManager m_VoteOptionManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly ITaskVoteManager m_TaskVoteManager;

        public VoteEntity CreateVote(CreateVoteModel voteModel)
        {
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
                        Order = p.Order
                    };
                    var voteOption = this.m_VoteOptionManager.CreateVoteOption(vote, optionModel);
                    vote.VoteOptions.Add(voteOption);
                });
            }

            return vote;
        }



        public IEnumerable<VoteEntity> FetchVotesByTaskId(Guid taskId, bool? isApproved)
        {
            var votes = this.m_TaskVoteManager.FetchVoteByTaskId(taskId).Select(p => p.Vote);

            if (isApproved.HasValue)
                votes = votes.Where(p => p.IsApproved == isApproved.Value).ToList();
            else
                votes = votes.Where(p => p.IsApproved == null).ToList();

            return votes.Select(s => new VoteEntity()
            {
                Id = s.Id,
                Subject = s.Subject,
                StartAt = s.StartAt,
                EndAt = s.EndAt,
                IsMultiEnabled = s.IsMultiEnabled,
                IsAnonEnabled = s.IsAnonEnabled,
                IsApproved = s.IsApproved,
                CreatedAt = s.CreatedAt,
                Creator = s.Creator,
                VoteOptions = s.VoteOptions.OrderBy(p => p.Order).ToList()
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

        public void UpdateVote(UpdateVoteModel voteModel)
        {
            Args.NotNull(voteModel, nameof(voteModel));

            var vote = VoteExistsResult.Check(this, voteModel.VoteId).ThrowIfFailed().Vote;

            if (vote.StartAt < DateTime.Now)
                throw new FineWorkException("共识已经开始，不可以修改");

            if (vote.Creator.Id != voteModel.CreatorStaffId)
                throw new FineWorkException("你没有权限修改此共识.");

            vote.Subject = voteModel.Subject;
            vote.StartAt = voteModel.StartAt;
            vote.EndAt = voteModel.EndAt;
            vote.IsAnonEnabled = voteModel.IsAnonEnabled;
            vote.IsMultiEnabled = voteModel.IsMultiEnabled;

            if (voteModel.VoteOptions.Any())
            {
                foreach (var option in voteModel.VoteOptions)
                {
                    this.m_VoteOptionManager.UpdateVoteOption(option);
                }
            }

            this.InternalUpdate(vote);
        }

    }
}

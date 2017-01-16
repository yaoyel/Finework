using System;
using System.Collections.Generic;
using System.Linq; 
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using System.Data.Entity;
using FineWork.Common;
using FineWork.Core;
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public class VotingManager : AefEntityManager<VotingEntity, Guid>, IVotingManager
    {
        public VotingManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IVoteOptionManager voteOptionManager,
            ITaskVoteManager taskVoteManager)
            : base(sessionProvider)
        {
            m_StaffManager = staffManager;
            m_VoteOptionManager = voteOptionManager;
            m_TaskVoteManager = taskVoteManager;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IVoteOptionManager m_VoteOptionManager;
        private readonly ITaskVoteManager m_TaskVoteManager;
         
         

        public void DeleteVotingByVoteId(VoteEntity vote,Guid staffId)
        { 
            if (vote != null)
            {
                var votings = FetchVotingByVoteId(vote.Id).Where(p => p.Staff.Id == staffId).ToList();
                if(votings.Any())
                    foreach (var voting in votings)
                    {
                        this.InternalDelete(voting);
                    } 
            }
        }


        public IEnumerable<VotingEntity> CreateVoting(CreateVotingModel votingModel)
        {
            var staff = StaffExistsResult.Check(m_StaffManager, votingModel.StaffId).ThrowIfFailed().Staff;

            if (!votingModel.Votings.Any())
                throw new FineWorkException("请至少选择一个共识项！");

            var voteOption =
                VoteOptionExistsResult.Check(m_VoteOptionManager, votingModel.Votings.First().VoteOptionId)
                    .ThrowIfFailed()
                    .VoteOption;

            //删除原来的选项
            DeleteVotingByVoteId(voteOption.Vote, votingModel.StaffId);

            var votings = new List<VotingEntity>();
            votingModel.Votings.ToList().ForEach(p =>
            {
                var voting = this.InternalCreateVoting(staff, p.VoteOptionId, p.Reason);
                votings.Add(voting);
            });
            return votings;
        }

        public IEnumerable<VotingEntity> FetchVotingByVoteId(Guid voteId)
        {
            return this.InternalFetch(p => p.Where(x => x.Option.Vote.Id == voteId)
                .Include(x => x.Option));
        }

        public IEnumerable<VotingEntity> FetchVotingByVoteOptionId(Guid voteOptionId)
        {
            return this.InternalFetch(p => p.Where(x => x.Option.Id == voteOptionId)
                .Include(x => x.Option));
        }

        public IEnumerable<VotingEntity> FetchVotingByTaskIdAndStaffId(Guid taskId, Guid staffId)
        {
            var votings = m_TaskVoteManager.FetchVoteByTaskId(taskId).Select(p => p.Vote)
                .SelectMany(p => p.VoteOptions)
                .SelectMany(p => p.Votings).Where(p => p.Staff.Id == staffId);

            return votings;
        }

        public VotingEntity FindByOptionIdWithStaffId(Guid optionId, Guid staffId)
        {
            return this.InternalFetch(p => p.Option.Id == optionId && p.Staff.Id == staffId).FirstOrDefault();
        }

        private VotingEntity InternalCreateVoting(StaffEntity staff, Guid voteOptionId, string reason)
        {

            var voteOption =
                VoteOptionExistsResult.Check(m_VoteOptionManager, voteOptionId).ThrowIfFailed().VoteOption;
            var voting = new VotingEntity();


            voting.Id = Guid.NewGuid();
            voting.Reason = reason;
            voting.Staff = staff;
            voting.Option = voteOption;
            this.InternalInsert(voting);

            return voting;
        }
    }
}

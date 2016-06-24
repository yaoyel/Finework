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
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public class VotingManager : AefEntityManager<VotingEntity, Guid>, IVotingManager
    {
        public VotingManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IVoteOptionManager voteOptionManager)
            : base(sessionProvider)
        {
            m_StaffManager = staffManager;
            m_VoteOptionManager = voteOptionManager; 
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IVoteOptionManager m_VoteOptionManager;  


        public IEnumerable<VotingEntity> CreateVoting(CreateVotingModel votingModel)
        {
            var staff = StaffExistsResult.Check(m_StaffManager, votingModel.StaffId).ThrowIfFailed().Staff;

            if (!votingModel.Votings.Any())
                throw new FineWorkException("请至少选择一个共识项！");

            var votings = new List<VotingEntity>();
            votingModel.Votings.ToList().ForEach(p =>
            {
                var voting=this.InternalCreateVoting(staff, p.VoteOptionId, p.Reason);
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
            return this.InternalFetch(q => q.Where(x => x.Staff.Id == staffId && x.Option.Vote.Task.Id == taskId)
                .Include(x => x.Option).Include(x => x.Option.Vote));
        }

        private VotingEntity InternalCreateVoting(StaffEntity staff, Guid voteOptionId, string reason)
        {

            var voteOption =
                VoteOptionExistsResult.Check(m_VoteOptionManager, voteOptionId).ThrowIfFailed().VoteOption;

            var voting = new VotingEntity()
            {
                Id = Guid.NewGuid(),
                Reason = reason,
                Staff = staff,
                Option = voteOption
            };

            this.InternalInsert(voting);
             
            return voting;
        }
    }
}

using System;
using System.Linq;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class VotingExistsResult : FineWorkCheckResult
    {
        public VotingExistsResult(bool isSucceed, String message, VotingEntity voting)
            : base(isSucceed, message)
        {
            this.Voting = voting;
        }

        public VotingEntity Voting { get; private set; }

        public static VotingExistsResult Check(IVotingManager votingManager, Guid voteOptionId,Guid staffId)
        {
            var voting = votingManager.FetchVotingByVoteOptionId(voteOptionId)
                .FirstOrDefault(p => p.Staff.Id == staffId); 
            return Check(voting, "不存在对应投票信息.");
        }
         
        private static VotingExistsResult Check([CanBeNull] VotingEntity voting, String message)
        {
            if (voting == null)
            {
                return new VotingExistsResult(false, message, null);
            }
            return new VotingExistsResult(true, null, voting);
        }
    }
}

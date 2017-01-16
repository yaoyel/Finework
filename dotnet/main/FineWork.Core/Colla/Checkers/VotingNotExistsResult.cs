using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class VotingNotExistsResult : FineWorkCheckResult
    {
        public VotingNotExistsResult(bool isSucceed, String message, VotingEntity voting)
            : base(isSucceed, message)
        {
            this.Voting = voting;
        }

        public VotingEntity Voting { get; private set; }

        public static VotingNotExistsResult Check(IVotingManager votingManager, Guid voteOptionId,Guid staffId)
        {
            var voting = votingManager.FindByOptionIdWithStaffId(voteOptionId,staffId);
             
            return Check(voting, null);
        }

        private static VotingNotExistsResult Check([CanBeNull] VotingEntity voting, String message)
        {
            if (voting != null)
            {
                return new VotingNotExistsResult(false, $"已与{voting.CreatedAt.ToShortDateString()}投票", voting);
            }
            return new VotingNotExistsResult(true, null, null);
        }
    }
}

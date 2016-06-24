using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Votes")]
    [Authorize("Bearer")]
    public class VoteController:FwApiController
    {
        public VoteController(ISessionScopeFactory sessionScopeFactory,
            IVoteManager voteManager,
            IVotingManager votingManager,
            IVoteOptionManager voteOptionManager)
            : base(sessionScopeFactory)
        {
            if (voteManager == null) throw new ArgumentException(nameof(voteManager));
            if (votingManager == null) throw new ArgumentException(nameof(votingManager));
            if (voteOptionManager == null) throw new ArgumentException(nameof(voteOptionManager)); 

            m_VoteManager = voteManager;
            m_VotingManger = votingManager;
        }

        private readonly IVoteManager m_VoteManager;
        private readonly IVotingManager m_VotingManger;

        [HttpPost("CreateVote")]
        [DataScoped(true)]
        public VoteViewModel CreateVote([FromBody]CreateVoteModel voteModel)
        {
            var vote=this.m_VoteManager.CreateVote(voteModel);
            return vote.ToViewModel();
        }

        [HttpPost("CreateVoting")]
        [DataScoped(true)]
        public IEnumerable<VotingViewModel> CreateVoting([FromBody]CreateVotingModel votingModel)
        {
            var voting = this.m_VotingManger.CreateVoting(votingModel);
            return voting.Select(p=>p.ToViewModel());
        }

        [HttpGet("FetchApprovedVotesByTaskId")]
        public IActionResult FetchApprovedVotesByTaskId(Guid taskId)
        {
            var votes = this.m_VoteManager.FetchVotesByTaskId(taskId,true);
            return votes != null
                ?new ObjectResult( votes.Select(p => p.ToViewModel()))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchUnApprovedVotesByTaskId")]
        public IActionResult FetchUnApprovedVotesByTaskId(Guid taskId)
        {
            var votes = this.m_VoteManager.FetchVotesByTaskId(taskId, false);
            return votes != null
                ? new ObjectResult(votes.Select(p => p.ToViewModel()))
                : new HttpNotFoundObjectResult(taskId);
        }

        /// <summary>
        /// 为方便处理，将待共识的投票按照staff分类
        /// 分为创建的，已投票，未参与三类
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpGet("FetchUnApprovedVotesByTaskIdAndStaffId")]
        public IActionResult FetchUnApprovedVotesByTaskIdAndStaffId(Guid taskId, Guid staffId)
        {
            //所有待共识
            var voteEntities = this.m_VoteManager.FetchVotesByTaskId(taskId, false);

            if (voteEntities != null) {
                var votes = voteEntities.Select(p => p.ToViewModel()).ToList();

                //创建的共识
                var votesAsCreator = votes.Where(p => p.Createor.Id == staffId).ToList();

                //已投票的共识
                var votesAsVoter = m_VotingManger.FetchVotingByTaskIdAndStaffId(taskId, staffId)
                    .Select(p => p.Option.Vote).Select(p => p.ToViewModel()).ToList();

                //待参与的共识
                var votesAsPartaker = votes.Except(votesAsCreator.Union(votesAsVoter)).ToList();

                return new ObjectResult(new
                {
                    VotesAsCreator= votesAsCreator,
                    VotesAsVoter= votesAsVoter,
                    VotesAsPartaker= votesAsPartaker
                });
            }
            return new HttpNotFoundObjectResult($"TaskId:{taskId},StaffId:{staffId}");
        }

        [HttpGet("FindVoteByVoteId")]
        public VoteViewModel FindVoteByVoteId(Guid voteId)
        {
            return this.m_VoteManager.FindVoteByVoteId(voteId).ToViewModel();
        }

        [HttpPost("UpdateVoteApprovedStatus")]
        [DataScoped(true)]
        public void UpdateVoteApprovedStatus(Guid voteId)
        {
            var vote = VoteExistsResult.Check(this.m_VoteManager, voteId).ThrowIfFailed().Vote;

            this.m_VoteManager.UpdateVoteApprovedStatus(vote);
        } 
     
    }
}

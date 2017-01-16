using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Votes")]
    [Authorize("Bearer")]
    public class VoteController:FwApiController
    {
        public VoteController(ISessionProvider<AefSession> sessionProvider,
            IVoteManager voteManager,
            IVotingManager votingManager,
            IVoteOptionManager voteOptionManager,
            ITaskVoteManager taskVoteManager
            )
            : base(sessionProvider)
        {
            if (voteManager == null) throw new ArgumentException(nameof(voteManager));
            if (votingManager == null) throw new ArgumentException(nameof(votingManager));
            if (voteOptionManager == null) throw new ArgumentException(nameof(voteOptionManager));
            if (taskVoteManager == null) throw new ArgumentException(nameof(taskVoteManager));

            m_VoteManager = voteManager;
            m_VotingManger = votingManager;
            m_TaskVoteManager = taskVoteManager;
        }

        private readonly IVoteManager m_VoteManager;
        private readonly IVotingManager m_VotingManger;
        private readonly ITaskVoteManager m_TaskVoteManager;
        [HttpPost("CreateVote")]
        //[DataScoped(true)]
        public VoteViewModel CreateVote(Guid taskId,[FromBody]CreateVoteModel voteModel)
        {
            using (var tx = TxManager.Acquire())
            {
                var vote = this.m_VoteManager.CreateVote(voteModel);
                var result = vote.ToViewModel(this.AccountId);

                m_TaskVoteManager.CreateTaskVote(taskId, voteModel.CreatorStaffId, vote.Id);
                tx.Complete();
                return result;
            }
        }
         
        [HttpPost("CreateVoting")]
        //[DataScoped(true)]
        public IEnumerable<VotingViewModel> CreateVoting([FromBody]CreateVotingModel votingModel)
        {
            using (var tx = TxManager.Acquire())
            {
                var voting = this.m_VotingManger.CreateVoting(votingModel);


                var result= voting.Select(p => p.ToViewModel());
                tx.Complete();
                return result;
            }
        }

        [HttpGet("FetchApprovedVotesByTask")]
        public IActionResult FetchApprovedVotesByTask(Guid taskId)
        {
            var votes = this.m_VoteManager.FetchVotesByTaskId(taskId,true);
            return votes != null
                ?new ObjectResult( votes.Select(p => p.ToViewModel(this.AccountId)))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchUnApprovedVotesByTask")]
        public IActionResult FetchUnApprovedVotesByTask(Guid taskId)
        {
            var votes = this.m_VoteManager.FetchVotesByTaskId(taskId, false);
            return votes != null
                ? new ObjectResult(votes.Select(p => p.ToViewModel(this.AccountId)))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchVotesByTask")]
        public IActionResult FetchVotesByTask(Guid taskId)
        {
            var votes = this.m_VoteManager.FetchVotesByTaskId(taskId,null);
            return votes != null
               ? new ObjectResult(votes.Select(p => p.ToViewModel(this.AccountId)))
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
                var votes = voteEntities.Select(p => p.ToViewModel(this.AccountId)).ToList();

                //创建的共识
                var votesAsCreator = votes.Where(p => p.Createor.Id == staffId).ToList();

                //已投票的共识
                var votesAsVoter = m_VotingManger.FetchVotingByTaskIdAndStaffId(taskId, staffId)
                    .Select(p => p.Option.Vote).Select(p => p.ToViewModel(this.AccountId)).ToList();

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
            return this.m_VoteManager.FindVoteByVoteId(voteId).ToViewModel(this.AccountId);
        }

        [HttpPost("UpdateVoteApprovedStatus")]
        //[DataScoped(true)]
        public void UpdateVoteApprovedStatus(Guid voteId,bool newStatus)
        {
            using (var tx = TxManager.Acquire())
            {
                var vote = VoteExistsResult.Check(this.m_VoteManager, voteId).ThrowIfFailed().Vote;
               
                this.m_VoteManager.UpdateVoteApprovedStatus(vote, newStatus);
                tx.Complete();
            }
        }


        [HttpPost("UpdateVote")]
        public void UpdateVote([FromBody] UpdateVoteModel voteModel)
        {
            Args.NotNull(voteModel, nameof(voteModel));

            using (var tx=TxManager.Acquire())
            {
                this.m_VoteManager.UpdateVote(voteModel);
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using AVOSCloud.RealtimeMessageV2;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/forums")]
    [Authorize("Bearer")]
    public class ForumController:FwApiController
    {
        public ForumController(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IForumSectionManager forumSectionManager,
            IForumTopicManager forumTopicManager,
            IForumLikeManager forumLikeManager,
            IForumCommentManager forumCommentManager,
            IForumVoteManager forumVoteManager,
            IVoteManager voteManager,
            IForumCommentLikeManager forumCommentLikeManager) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(forumSectionManager, nameof(forumSectionManager));
            Args.NotNull(forumTopicManager, nameof(forumTopicManager));
            Args.NotNull(forumLikeManager, nameof(forumLikeManager));
            Args.NotNull(forumCommentManager, nameof(forumCommentManager));
            Args.NotNull(forumVoteManager, nameof(forumVoteManager));

            m_StaffManager = staffManager;
            m_ForumTopicManager = forumTopicManager;
            m_ForumSectionManager = forumSectionManager;
            m_ForumCommentManager = forumCommentManager;
            m_ForumVoteManager = forumVoteManager;
            m_ForumLikeManager = forumLikeManager;
            m_VoteManager = voteManager;
            m_ForumCommentLikeManager = forumCommentLikeManager;

        }

        private readonly IStaffManager m_StaffManager;
        private readonly IForumSectionManager m_ForumSectionManager;
        private readonly IForumTopicManager m_ForumTopicManager;
        private readonly IForumCommentManager m_ForumCommentManager;
        private readonly IForumLikeManager m_ForumLikeManager;
        private readonly IForumVoteManager m_ForumVoteManager;
        private readonly IVoteManager m_VoteManager;
        private readonly IForumCommentLikeManager m_ForumCommentLikeManager;

        [HttpGet("FetchForumSectionByStaffId")]
        public IActionResult FetchForumSectionByStaffId(Guid staffId,ForumSections section,string version)
        {
            var forumSections = m_ForumSectionManager.FetchForumSectionByStaffId(staffId, section, version);

            if (forumSections!=null)
            { 
                return new ObjectResult(forumSections.ToViewModel()); 
            } 
            return new HttpNotFoundObjectResult(staffId); 
        }

        [HttpGet("FetchTopicsBySectionId")]
        public IActionResult FetchTopicsBySectionId(Guid forumSectionId)
        {
            var result = m_ForumTopicManager.FetchForumTopicBySectionId(forumSectionId).ToList();

            if (result.Any())
                return new ObjectResult(result.Select(p => p.ToForumViewModel(true)));

            return new HttpNotFoundObjectResult(forumSectionId);
        }

        [HttpGet("FetchForumSectionVersions")]
        public IActionResult FetchForumSectionVersions(Guid orgId, ForumSections section)
        { 
            var forumSections = m_ForumSectionManager.FetchForumSectionByOrgId(orgId, section).ToList();
            var  versions = new List<Tuple<Guid, string>>();
            if (forumSections.Any())
            {
                var sectionGroups = forumSections.GroupBy(p => p.CreatedAt.Year);
                 
                foreach (var sectionGroup in sectionGroups)
                {
                    var itemIndex = 0;
                    foreach (var item in sectionGroup)
                    {
                        itemIndex++;
                        var version = $"{item.CreatedAt.Year}年{itemIndex.ToString().PadLeft(2, '0')}版";

                        versions.Add(new Tuple<Guid, string>(item.Id,version)); 
                    }
                }
            }

            return new ObjectResult(versions);
        }

        [HttpGet("FindForumSectionById")]
        public IActionResult FindForumSectionById(Guid forumSectionId)
        {
            var forumSection = ForumSectionExistsResult.Check(this.m_ForumSectionManager, forumSectionId).ForumSection;

            if(forumSection!=null) return new ObjectResult(forumSection.ToViewModel(true));
            return new HttpNotFoundObjectResult(forumSectionId);
        }

        [HttpGet("FindForumTopicById")]
        public IActionResult FindForumTopicById(Guid topicId)
        {
            var topic = ForumTopicExistsResult.Check(this.m_ForumTopicManager, topicId).ForumTopic;

            if (topic != null)
            {
                var result = topic.ToViewModel(this.AccountId,true);

                result.LikeFlag = topic.ForumLikes.Any(p => p.Staff.Account.Id == this.AccountId);


                return new ObjectResult(result);
            } 
            return new HttpNotFoundObjectResult(topicId);
        }

        [HttpPost("CreateForumSection")]
        public IActionResult CreateForumSection([FromBody]CreateForumSectionModel forumSectionModel)
        { 
            Args.NotNull(forumSectionModel, nameof(forumSectionModel)); 

            using (var tx = TxManager.Acquire())
            { 
               var forumSection= this.m_ForumSectionManager.CreateForumSetcion(forumSectionModel);

                var result = forumSection.ToViewModel();
                tx.Complete();

                return new ObjectResult(result);
            } 
        }

        [HttpPost("CreateForumTopic")]
        public IActionResult CreateForumTopic([FromBody] CreateForumTopicModel forumTopicModel)
        {
            Args.NotNull(forumTopicModel, nameof(forumTopicModel));

            using (var tx = TxManager.Acquire())
            { 
                var topic=this.m_ForumTopicManager.CreateForumTopic(forumTopicModel);
                var result = topic.ToViewModel(this.AccountId,true);
                tx.Complete(); 
                return new ObjectResult(result);
            }
        }

        [HttpPost("CreateForumVote")]
        public IActionResult CreateForumVote(Guid forumSectionId, [FromBody] CreateVoteModel voteModel)
        {
            Args.NotNull(voteModel, nameof(voteModel));

            var createTopicModel = new CreateForumTopicModel();
            createTopicModel.Content = voteModel.Subject;
            createTopicModel.ForumSectionId = forumSectionId;
            createTopicModel.StaffId = voteModel.CreatorStaffId;
            createTopicModel.TopicType = ForumPostTypes.Vote;

            using (var tx = TxManager.Acquire())
            {
                var topic = m_ForumTopicManager.CreateForumTopic(createTopicModel);
                var vote = m_VoteManager.CreateVote(voteModel);
                this.m_ForumVoteManager.CreateForumVote(vote, topic);
                var result = topic.ToViewModel(this.AccountId);
                tx.Complete();
                return new ObjectResult(result);
            }
        }

        [HttpPost("IncreaseForumTopicViews")]
        public IActionResult IncreaseForumTopicViews(Guid topicId)
        {
            using (var tx=TxManager.Acquire())
            {
                m_ForumTopicManager.IncreaseForumTopicViews(topicId);
                tx.Complete(); 
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost("CreateForumLike")]
        public IActionResult CreateForumLike(Guid topicId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_ForumLikeManager.CreateForumLike(staffId, topicId);
                tx.Complete();
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost("DeleteForumLike")]
        public IActionResult DeleteForumLike(Guid topicId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_ForumLikeManager.DeleteForumLike(staffId, topicId);
                tx.Complete();
            }
            return new HttpStatusCodeResult(204);
        }

        [HttpPost("CreateForumComment")]
        public IActionResult CreateForumComment([FromBody]CraeteForumCommentModel forumCommentModel)
        {
            Args.NotNull(forumCommentModel, nameof(forumCommentModel));

            using (var tx = TxManager.Acquire())
            {
                var comment=this.m_ForumCommentManager.CreateForumComment(forumCommentModel);
                var result = comment.ToViewModel(this.AccountId,true);
                tx.Complete();
                return new ObjectResult(result);
            }
        }

        [HttpPost("DeleteForumComment")]
        public IActionResult DeleteForumComment(Guid commentId)
        { 
            using (var tx = TxManager.Acquire())
            {
                this.m_ForumCommentManager.DeleteForumComment(commentId);
                tx.Complete();
            }
            return new HttpStatusCodeResult(204);
        }

        [HttpGet("FetchCommentsByTopicId")]
        public IActionResult FetchCommentsByTopicId(Guid topicId,int? pageSize,int? page)
        {
            var comments = m_ForumCommentManager.FetchForumCommentsByTopicId(topicId).ToList().AsQueryable().ToPagedList(page,pageSize);

            if(comments.Any()) return new ObjectResult(comments.Select(p=>p.ToViewModel(this.AccountId,true)));

            return new HttpNotFoundObjectResult(topicId);
        }

        [HttpPost("UpdateForumTopic")]
        public IActionResult UpdateForumTopic([FromBody]UpdateForumTopicModel updateForumTopicModel)
        {
            Args.NotNull(updateForumTopicModel, nameof(updateForumTopicModel));

            using (var tx = TxManager.Acquire())
            {
                this.m_ForumTopicManager.UpdateForumTopic(updateForumTopicModel);
                tx.Complete();
            }
          
            return new HttpStatusCodeResult(200);
        }

        [HttpPost("UpdateForumComment")]
        public IActionResult UpdateForumComment([FromBody]UpdateForumCommentModel updateForumCommentModel)
        {
            Args.NotNull(updateForumCommentModel, nameof(updateForumCommentModel));

            using (var tx = TxManager.Acquire())
            {
                this.m_ForumCommentManager.UpdateForumComment(updateForumCommentModel);
                tx.Complete();
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost("CreateCommentLike")]
        public IActionResult CreateCommentLike(Guid staffId, Guid commentId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_ForumCommentLikeManager.CreateForumComentLike(staffId, commentId);
                tx.Complete();
            }
            return new HttpStatusCodeResult(201);
        }

        [HttpPost("DeleteCommentLike")]
        public IActionResult DeleteCommentLike(Guid staffId, Guid commentId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_ForumCommentLikeManager.DeleteForumCommentLike(staffId, commentId);
                tx.Complete();
            }

            return new HttpStatusCodeResult(200);
        }

    }
}

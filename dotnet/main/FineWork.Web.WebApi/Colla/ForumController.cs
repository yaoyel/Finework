using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography.Xml;
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
using FineWork.Logging;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using FineWork.Message;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/forums")]
    [Authorize("Bearer")]
    public class ForumController : FwApiController
    {
        public ForumController(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IForumSectionManager forumSectionManager,
            IForumTopicManager forumTopicManager,
            IForumLikeManager forumLikeManager,
            IForumCommentManager forumCommentManager,
            IForumVoteManager forumVoteManager,
            IVoteManager voteManager,
            IForumCommentLikeManager forumCommentLikeManager,
            INotificationManager notificationManager,
            IConfiguration config,
            IAccessTimeManager accessTimeManager,
            IForumSectionViewTimeManager forumSectionViewTimeManager ) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(forumSectionManager, nameof(forumSectionManager));
            Args.NotNull(forumTopicManager, nameof(forumTopicManager));
            Args.NotNull(forumLikeManager, nameof(forumLikeManager));
            Args.NotNull(forumCommentManager, nameof(forumCommentManager));
            Args.NotNull(forumVoteManager, nameof(forumVoteManager));
            Args.NotNull(forumVoteManager, nameof(notificationManager));

            m_StaffManager = staffManager;
            m_ForumTopicManager = forumTopicManager;
            m_ForumSectionManager = forumSectionManager;
            m_ForumCommentManager = forumCommentManager;
            m_ForumVoteManager = forumVoteManager;
            m_ForumLikeManager = forumLikeManager;
            m_VoteManager = voteManager;
            m_ForumCommentLikeManager = forumCommentLikeManager;
            m_NotificationManager = notificationManager;
            m_Config = config;
            m_AccessTimeManager = accessTimeManager;
            m_ForumSectionViewTimeManager = forumSectionViewTimeManager; 
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IForumSectionManager m_ForumSectionManager;
        private readonly IForumTopicManager m_ForumTopicManager;
        private readonly IForumCommentManager m_ForumCommentManager;
        private readonly IForumLikeManager m_ForumLikeManager;
        private readonly IForumVoteManager m_ForumVoteManager;
        private readonly IVoteManager m_VoteManager;
        private readonly IForumCommentLikeManager m_ForumCommentLikeManager;
        private readonly INotificationManager m_NotificationManager;
        private readonly IConfiguration m_Config;
        private readonly IAccessTimeManager m_AccessTimeManager;
        private readonly IForumSectionViewTimeManager m_ForumSectionViewTimeManager;
        private readonly ILogger m_Logger=LogManager.GetLogger(typeof(ForumController));

        [HttpGet("FetchForumSectionByStaffId")]
        public IActionResult FetchForumSectionByStaffId(Guid staffId, ForumSections section, string version)
        {
            var forumSections = m_ForumSectionManager.FetchForumSectionByStaffId(staffId, section, version);

            if (forumSections != null)
            {
                return new ObjectResult(forumSections.ToViewModel());
            }

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("HasUnReadForum")]
        public IActionResult HasUnReadForum(Guid staffId)
        {
            var result = m_ForumSectionManager.HasUnReadForumByStaffId(staffId).ToList();

            if (result.Any())
                return new ObjectResult(result);

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FetchUnReadCommentByStaffId")]
        public IActionResult FetchUnReadCommentByStaffId(Guid staffId, bool isDetail = false)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).Staff;

            if (staff != null)
            {
                var result = FetchUnReadForumsByStaff(staff);
                if (result != null)
                {
                    if (isDetail)
                    {
                        m_AccessTimeManager.UpdateLastViewForumCommentTime(staffId, DateTime.Now);
                        return new ObjectResult(result);
                    }

                    return new ObjectResult(result.Select(p=>p.ForumId));
                }
            }
            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FetchTopicsBySectionId")]
        public IActionResult FetchTopicsBySectionId(Guid forumSectionId, Guid staffId)
        {
            var result = m_ForumTopicManager.FetchForumTopicBySectionId(forumSectionId).ToList();

            //修改最后访问时间
            if (staffId != default(Guid))
                using (var tx = TxManager.Acquire())
                {
                    m_AccessTimeManager.UpdateLastViewForumTime(staffId, DateTime.Now);
                    tx.Complete();
                }

            if (result.Any())
                return new ObjectResult(result.Select(p => p.ToForumViewModel(true)));

            return new HttpNotFoundObjectResult(forumSectionId);
        }

        [HttpGet("FetchForumSectionVersions")]
        public IActionResult FetchForumSectionVersions(Guid orgId, ForumSections section)
        {
            var forumSections = m_ForumSectionManager.FetchForumSectionByOrgIdWithSection(orgId, section).ToList();
            var versions = new List<Tuple<Guid, string,bool,DateTime>>();
            var staff = StaffExistsResult.Check(m_StaffManager, orgId, AccountId).ThrowIfFailed().Staff;

            if (forumSections.Any())
            {
                var sectionGroups = forumSections.GroupBy(p => p.CreatedAt.Year);

                var hasNewSectionIds = this.FetchIrrelevantForumsByStaff(staff.Id,section);
                foreach (var sectionGroup in sectionGroups)
                {
                    var itemIndex = 0;
                    foreach (var item in sectionGroup)
                    {
                        itemIndex++;
                        var version = $"{item.CreatedAt.Year}年{itemIndex.ToString().PadLeft(2, '0')}版";
                        var isNew = hasNewSectionIds.Contains(item.Id);
                        versions.Add(new Tuple<Guid, string, bool,DateTime>(item.Id, version, isNew,item.CreatedAt));
                    }
                }
            }
            UpdateLastViewForumSectionTime(section, staff.Id);
            return new ObjectResult(versions);
        }

        [HttpGet("FindForumSectionById")]
        public IActionResult FindForumSectionById(Guid forumSectionId)
        {
            var forumSection = ForumSectionExistsResult.Check(this.m_ForumSectionManager, forumSectionId).ForumSection;

            if (forumSection != null) return new ObjectResult(forumSection.ToViewModel(true));
            return new HttpNotFoundObjectResult(forumSectionId);
        }

        [HttpGet("FindForumTopicById")]
        public IActionResult FindForumTopicById(Guid topicId)
        {
            var topic = ForumTopicExistsResult.Check(this.m_ForumTopicManager, topicId).ForumTopic;

            if (topic != null)
            {
                var result = topic.ToViewModel(this.AccountId, true);

                result.LikeFlag = topic.ForumLikes.Any(p => p.Staff.Account.Id == this.AccountId);


                return new ObjectResult(result);
            }
            return new HttpNotFoundObjectResult(topicId);
        }

        [HttpPost("CreateForumSection")]
        public IActionResult CreateForumSection([FromBody] CreateForumSectionModel forumSectionModel)
        {
            Args.NotNull(forumSectionModel, nameof(forumSectionModel));

            using (var tx = TxManager.Acquire())
            {
                var forumSection = this.m_ForumSectionManager.CreateForumSetcion(forumSectionModel);

                var result = forumSection.ToViewModel();

                var phoneNumbers = forumSection.Staff.Org.Staffs.Where(p => p.Id != forumSectionModel.StaffId)
                    .Select(p => p.Account.PhoneNumber).ToArray();

                SendForumMessageAsync(forumSection.Staff.Org.Id, "forumTopic", forumSection.Id,
                  (int)forumSection.Section, forumSection.Id,
                  null, phoneNumbers);
                tx.Complete();
                return new ObjectResult(result);
            }
        }

        [HttpPost("CreateForumTopic")]
        public  IActionResult CreateForumTopic([FromBody] CreateForumTopicModel forumTopicModel)
        {
            Args.NotNull(forumTopicModel, nameof(forumTopicModel));

            var staff = new StaffEntity();

            var forumTopicEntity = new ForumTopicEntity();
            var forumTopicViewModel = new ForumTopicViewModel();
            using (var tx = TxManager.Acquire())
            {
                forumTopicEntity = this.m_ForumTopicManager.CreateForumTopic(forumTopicModel);

                forumTopicViewModel = forumTopicEntity.ToViewModel(this.AccountId, true);

                staff = forumTopicEntity.Staff;
                tx.Complete();
            }

            var phoneNumbers = staff.Org.Staffs.Where(p => p.Id != staff.Id)
                .Select(p => p.Account.PhoneNumber).ToArray();
 
              SendForumMessageAsync(staff.Org.Id, "forumTopic", forumTopicEntity.ForumSection.Id,
                    (int) forumTopicEntity.ForumSection.Section, forumTopicEntity.Id,
                    null, phoneNumbers);
         
        
            return new ObjectResult(forumTopicViewModel);
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

            var staff = new StaffEntity();
            var forumTopicViewModel = new ForumTopicViewModel();
            var forumTopicEntity = new ForumTopicEntity();
            using (var tx = TxManager.Acquire())
            {
                forumTopicEntity = m_ForumTopicManager.CreateForumTopic(createTopicModel);
                var vote = m_VoteManager.CreateVote(voteModel);
                this.m_ForumVoteManager.CreateForumVote(vote, forumTopicEntity);
                forumTopicViewModel = forumTopicEntity.ToViewModel(this.AccountId);

                staff = forumTopicEntity.Staff;
                tx.Complete();
            }

            var phoneNumbers = staff.Org.Staffs.Where(p => p.Id != staff.Id)
                .Select(p => p.Account.PhoneNumber).ToArray();


            SendForumMessageAsync(staff.Org.Id, "forumTopic", forumTopicEntity.ForumSection.Id,
                (int) forumTopicEntity.ForumSection.Section, forumTopicEntity.Id,
                null, phoneNumbers);

            return new ObjectResult(forumTopicViewModel);
        }

        [HttpPost("IncreaseForumTopicViews")]
        public IActionResult IncreaseForumTopicViews(Guid topicId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_ForumTopicManager.IncreaseForumTopicViews(topicId);
                tx.Complete();
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost("CreateForumLike")]
        public IActionResult CreateForumLike(Guid topicId, Guid staffId)
        {
            var forumLike = new ForumLikeEntity();
            using (var tx = TxManager.Acquire())
            {
                forumLike = this.m_ForumLikeManager.CreateForumLike(staffId, topicId);
                tx.Complete();
            }

            if (forumLike.Staff != forumLike.ForumTopic.Staff)
            {

                SendForumMessageAsync(forumLike.Staff.Org.Id, "forumLike", forumLike.ForumTopic.ForumSection.Id,
                    (int) forumLike.ForumTopic.ForumSection.Section, forumLike.ForumTopic.Id, null,
                    forumLike.ForumTopic.Staff.Account.PhoneNumber); 
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
        public IActionResult CreateForumComment([FromBody] CraeteForumCommentModel forumCommentModel)
        {
            Args.NotNull(forumCommentModel, nameof(forumCommentModel));

            var forumComment = new ForumCommentEntity();
            var forumCommentViewModel = new ForumCommentViewModel();
            using (var tx = TxManager.Acquire())
            {
                forumComment = this.m_ForumCommentManager.CreateForumComment(forumCommentModel);
                forumCommentViewModel = forumComment.ToViewModel(this.AccountId, true);
                tx.Complete();
            }

            if (forumCommentModel.TargetCommentId == default(Guid) &&
                forumComment.Staff != forumComment.ForumTopic.Staff)

                SendForumMessageAsync(forumComment.Staff.Org.Id, "forumComment",
                    forumComment.ForumTopic.ForumSection.Id,
                    (int) forumComment.ForumTopic.ForumSection.Section, forumComment.ForumTopic.Id,
                    forumComment.Id,
                    forumComment.ForumTopic.Staff.Account.PhoneNumber);

            return new ObjectResult(forumCommentViewModel);
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
        public IActionResult FetchCommentsByTopicId(Guid topicId, int? pageSize, int? page)
        {
            var comments =
                m_ForumCommentManager.FetchForumCommentsByTopicId(topicId)
                    .ToList()
                    .AsQueryable()
                    .ToPagedList(page, pageSize).Data.ToList();

            if (comments.Any()) return new ObjectResult(comments.Select(p => p.ToViewModel(this.AccountId, true)));

            return new HttpNotFoundObjectResult(topicId);
        }

        [HttpPost("UpdateForumTopic")]
        public IActionResult UpdateForumTopic([FromBody] UpdateForumTopicModel updateForumTopicModel)
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
        public IActionResult UpdateForumComment([FromBody] UpdateForumCommentModel updateForumCommentModel)
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

        [HttpPost("UpdateForumVerViewTime")]
        public IActionResult UpdateForumVerViewTime(Guid staffId, ForumSections section,Guid forumSectionId)
        {
            m_ForumSectionViewTimeManager.CreateFroumSectionViewTime(forumSectionId, staffId);

            return new HttpStatusCodeResult(200); 
        }

        [HttpGet("FetchForumTopicsByContent")]
        public IActionResult FetchForumTopicsByContent(Guid orgId,string content,int? page,int? pageSize)
        { 
            var topics = this.m_ForumTopicManager.FetchForumTopicByContent(orgId, content)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.ToSimpleViewModel(this.AccountId, true))
                .AsQueryable()
                .ToPagedList(page, pageSize);
                      
            if(page==null && pageSize==null)
                return new ObjectResult(topics.Data);
            if (topics.Data.Any())
                return new ObjectResult(topics);


            return new HttpNotFoundObjectResult(content);

        }

        private   Task SendForumMessageAsync(Guid orgId, string from, Guid? forumSectionId, int section, Guid? topicId,
            Guid? commentId, params string[] phoneNumber)
        {
            string message = string.Format(m_Config["PushMessage:Forum"]);
            var extra = new Dictionary<string, string>();
            extra.Add("PathTo", "forum");
            extra.Add("From", from);
            extra.Add("OrgId", orgId.ToString());
            extra.Add("Section", section.ToString());
            extra.Add("ForumSectionId", forumSectionId?.ToString() ?? "");
            extra.Add("TopicId", topicId?.ToString() ?? "");
            extra.Add("CommentId", commentId?.ToString() ?? "");

          return    m_NotificationManager.SendByAliasAsync("", message, extra, phoneNumber).ContinueWith(
                t => m_Logger.LogWarning(0, "forumpushwarring", t.Exception), TaskContinuationOptions.OnlyOnFaulted); 
        }

        private List<UnReadForumViewModel> FetchUnReadForumsByStaff(StaffEntity staff)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staff.Id).AccessTime;

            var unReadComment = m_ForumCommentManager.FetchCommentsByTopicOrCommentCreator(staff.Id)
                .Where(p => p.Staff.Id != staff.Id)
                .Select(p => new UnReadForumViewModel
                {
                    Version =this.m_ForumSectionManager.FetchVersionByForumSectionId(p.ForumTopic.ForumSection.Id),
                    Section = p.ForumTopic.ForumSection.Section, 
                    ForumId = p.ForumTopic.ForumSection.Id,
                    CommentId = p.Id,
                    Content = p.Content,
                    AccountId = p.Staff.Account.Id,
                    IsLike = false,
                    TopicId = p.ForumTopic.Id,
                    TopicCotent = p.ForumTopic.Content,
                    TargetCommentId = p.TargetComment?.Id,
                    TargetCommentContent = p.TargetComment?.Content,
                    CreatedAt = p.CreatedAt,
                    StaffName = p.Staff.Name,
                    SecurityStamp = p.Staff.Account.SecurityStamp
                });

            var unReadTopicLike = m_ForumLikeManager.FetchForumLikesByTopicCreatorId(staff.Id)
                .Where(p => p.Staff.Id != staff.Id)
                .Select(p => new UnReadForumViewModel
                {
                    Version = this.m_ForumSectionManager.FetchVersionByForumSectionId(p.ForumTopic.ForumSection.Id),
                    Section = p.ForumTopic.ForumSection.Section,
                    ForumId = p.ForumTopic.ForumSection.Id,
                    LikeId = p.Id,
                    AccountId = p.Staff.Account.Id,
                    IsLike = true,
                    TopicId = p.ForumTopic.Id,
                    TopicCotent = p.ForumTopic.Content, 
                    CreatedAt = p.CreatedAt,
                    StaffName = p.Staff.Name,
                    SecurityStamp = p.Staff.Account.SecurityStamp
                });

            var unReadCommentLike =
                m_ForumCommentLikeManager.FetchCommentLikesByCreator(staff.Id)
                    .Where(p => p.Staff.Id != staff.Id)
                    .Select(p => new UnReadForumViewModel
                    {
                        Version = this.m_ForumSectionManager.FetchVersionByForumSectionId(p.ForumComment.ForumTopic.ForumSection.Id),
                        Section = p.ForumComment.ForumTopic.ForumSection.Section,
                        ForumId = p.ForumComment.ForumTopic.ForumSection.Id,
                        LikeId = p.Id,
                        AccountId = p.Staff.Account.Id,
                        IsLike = true,
                        TargetCommentId = p.ForumComment.Id,
                        TargetCommentContent = p.ForumComment.Content,
                        TopicId = p.ForumComment.ForumTopic.Id,
                        CreatedAt = p.CreatedAt,
                        StaffName = p.Staff.Name,
                        SecurityStamp = p.Staff.Account.SecurityStamp
                    });

            var result = unReadComment.Union(unReadTopicLike).Union(unReadCommentLike).ToList();

            if (result.Any() && accessTime.LastViewForumCommentAt != null)
            {
                result = result.Where(p => p.CreatedAt > accessTime.LastViewForumCommentAt).ToList();
            }

            if (result.Any()) return result;
            return null;
        }

        private Guid[] FetchIrrelevantForumsByStaff(Guid staffId,ForumSections section)
        {
            return m_ForumSectionManager.FetchIrrelevantForumsByStaff(staffId,section).Item1.ToArray();
        }

        private void UpdateLastViewForumSectionTime(ForumSections forumSection, Guid staffId)
        {
            switch (forumSection)
            {
                case ForumSections.Mission:
                    m_AccessTimeManager.UpdateLastViewMissinTime(staffId, DateTime.Now);
                    break;
                case ForumSections.Vision:
                    m_AccessTimeManager.UpdateLastViewVisionTime(staffId, DateTime.Now);
                    break;
                case ForumSections.Values:
                    m_AccessTimeManager.UpdateLastViewValuesTime(staffId, DateTime.Now);
                    break;
                case ForumSections.Strategy:
                    m_AccessTimeManager.UpdateLastViewStrategyTime(staffId, DateTime.Now);
                    break;
                case ForumSections.OrgGovernance:
                    m_AccessTimeManager.UpdateLastViewOrgGovernanceTime(staffId, DateTime.Now);
                    break;

            }
        }
    }
}
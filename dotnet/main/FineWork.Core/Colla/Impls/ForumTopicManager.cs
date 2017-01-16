using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;

namespace FineWork.Colla.Impls
{
    public class ForumTopicManager : AefEntityManager<ForumTopicEntity, Guid>, IForumTopicManager
    {
        public ForumTopicManager(ISessionProvider<AefSession> sessionProvider,
            IForumSectionManager forumSectionManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(forumSectionManager, nameof(forumSectionManager));
            Args.NotNull(staffManager, nameof(staffManager));

            m_StaffManager = staffManager;
            m_ForumSectionManager = forumSectionManager;
        }

        private readonly IForumSectionManager m_ForumSectionManager;
        private readonly IStaffManager m_StaffManager;

        public ForumTopicEntity CreateForumTopic(CreateForumTopicModel forumTopicModel)
        {
            Args.NotNull(forumTopicModel, nameof(forumTopicModel));

            var forumSection =
                ForumSectionExistsResult.Check(this.m_ForumSectionManager, forumTopicModel.ForumSectionId)
                    .ThrowIfFailed()
                    .ForumSection;

            var staff = StaffExistsResult.Check(this.m_StaffManager, forumTopicModel.StaffId).ThrowIfFailed().Staff;
            var forumTopicEntity = new ForumTopicEntity();
            forumTopicEntity.Id = Guid.NewGuid();
            forumTopicEntity.Content = forumTopicModel.Content;
            forumTopicEntity.ForumSection = forumSection;
            forumTopicEntity.Type = forumTopicModel.TopicType;
            forumTopicEntity.Staff = staff;
            forumTopicEntity.Title = forumTopicModel.Title;

            this.InternalInsert(forumTopicEntity);
            return forumTopicEntity;
        }

        public IEnumerable<ForumTopicEntity> FetchForumTopicBySectionId(Guid sectionId)
        {
            return this.InternalFetch(p => p.ForumSection.Id == sectionId);
        }

        public ForumTopicEntity FindById(Guid topicId)
        {
            return this.InternalFetch(p => p.Id == topicId).FirstOrDefault();
        }

        public void IncreaseForumTopicViews(Guid topicId)
        {
            var forumTopic = ForumTopicExistsResult.Check(this, topicId).ThrowIfFailed().ForumTopic;

            forumTopic.ViewTotal++;
            this.InternalUpdate(forumTopic);
        }

        public ForumTopicEntity UpdateForumTopic(UpdateForumTopicModel updateForumTopicModel)
        {
            Args.NotNull(updateForumTopicModel, nameof(updateForumTopicModel));

            var topic = ForumTopicExistsResult.Check(this, updateForumTopicModel.TopicId).ThrowIfFailed().ForumTopic;
            var staff =
                StaffExistsResult.Check(this.m_StaffManager, updateForumTopicModel.StaffId).ThrowIfFailed().Staff;

            if (topic.Staff != staff) throw new FineWorkException("你不可修改此讨论内容.");

            topic.Content = updateForumTopicModel.Content;
            topic.Title = updateForumTopicModel.Title;
            topic.LastUpdatedAt = DateTime.Now;

            this.InternalUpdate(topic);
            return topic;
        }

        public IEnumerable<ForumTopicEntity> FetchForumTopicByOrgId(Guid orgId)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId);
        }

        public IEnumerable<ForumTopicEntity> FetchForumTopicByContent(Guid orgId,string content)
        {
            if (string.IsNullOrEmpty(content))
                return this.InternalFetch(p => p.Staff.Org.Id == orgId);

            return this.InternalFetch(p =>p.Staff.Org.Id==orgId && (p.Title==""?p.Content.Substring(0,content.Length>20?20:content.Length).Contains(content):p.Title.Contains(content)
            || p.ForumVote.Vote.Subject.Contains(content) ));
        }
 
    }
}
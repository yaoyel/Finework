using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;

namespace FineWork.Colla.Impls
{
    public class ForumLikeManager : AefEntityManager<ForumLikeEntity, Guid>, IForumLikeManager
    {
        public ForumLikeManager(ISessionProvider<AefSession> sessionProvider,
            IForumTopicManager forumTopicManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(forumTopicManager, nameof(forumTopicManager));
            Args.NotNull(staffManager, nameof(staffManager));

            m_ForumTopicManager = forumTopicManager;
            m_StaffManager = staffManager;
        }

        private readonly IForumTopicManager m_ForumTopicManager;
        private readonly IStaffManager m_StaffManager;
        public ForumLikeEntity CreateForumLike(Guid staffId, Guid topicId)
        {
            var topic = ForumTopicExistsResult.Check(this.m_ForumTopicManager, topicId).ThrowIfFailed().ForumTopic;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var forumLike=new ForumLikeEntity();
            forumLike.Staff = staff;
            forumLike.ForumTopic = topic;
            forumLike.Id = Guid.NewGuid(); 

            this.InternalInsert(forumLike);
            return forumLike;
        }

        public void DeleteForumLike(Guid staffId, Guid topicId)
        {
            var forumLike =
                this.InternalFetch(p => p.Staff.Id == staffId && p.ForumTopic.Id == topicId).FirstOrDefault();

            if (forumLike != null) this.InternalDelete(forumLike); 
        }

        public IEnumerable<ForumLikeEntity> FetchForumLikesByTopicCreatorId(Guid staffId)
        {
            return this.InternalFetch(p => p.ForumTopic.Staff.Id == staffId);
        }
        public IEnumerable<ForumLikeEntity> FetchForumLikesByTopicId(Guid topicId)
        {
            return this.InternalFetch(p => p.ForumTopic.Id == topicId);
        } 
    }
}
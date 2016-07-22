using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IForumTopicManager
    {
        ForumTopicEntity CreateForumTopic(CreateForumTopicModel forumTopicModel);

        IEnumerable<ForumTopicEntity> FetchForumTopicBySectionId(Guid sectionId);

        ForumTopicEntity FindById(Guid topicId);

        void IncreaseForumTopicViews(Guid topicId);

        ForumTopicEntity UpdateForumTopic(UpdateForumTopicModel updateForumTopicModel);
    }
}
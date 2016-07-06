using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IForumTopicManager
    {
        ForumTopicEntity CreateForumTopic(int type, string content);

        IEnumerable<ForumTopicEntity> FetchForumTopicBySectionId(Guid sectionId); 
    }
}
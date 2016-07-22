using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class ForumTopicExistsResult : FineWorkCheckResult
    {
        public ForumTopicExistsResult(bool isSucceed, String message, ForumTopicEntity forumTopic)
            : base(isSucceed, message)
        {
            this.ForumTopic = forumTopic;
        }

        public ForumTopicEntity ForumTopic { get; set; }
        public static ForumTopicExistsResult Check(IForumTopicManager forumTopicManager, Guid topicId)
        {
            var forumTopic = forumTopicManager.FindById(topicId);
            return Check(forumTopic, "不存在对用的讨论主题.");
        }

 
        private static ForumTopicExistsResult Check([CanBeNull]  ForumTopicEntity forumTopic, String message)
        {
            if (forumTopic == null)
            {
                return new ForumTopicExistsResult(false, message, null);
            }

            return new ForumTopicExistsResult(true, null, forumTopic);
        }
    }
}

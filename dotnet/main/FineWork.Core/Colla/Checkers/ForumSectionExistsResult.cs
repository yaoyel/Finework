using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class ForumSectionExistsResult : FineWorkCheckResult
    {
        public ForumSectionExistsResult(bool isSucceed, String message, ForumSectionEntity forumSection)
            : base(isSucceed, message)
        {
            this.ForumSection = forumSection;
        }

        public ForumSectionEntity ForumSection { get; set; }
        public static ForumSectionExistsResult Check(IForumSectionManager forumSectionManager, Guid forumSectionId)
        {
            var forumSection = forumSectionManager.FindById(forumSectionId);
            return Check(forumSection, "不存在对应的基本法版本");
        }


        private static ForumSectionExistsResult Check([CanBeNull]  ForumSectionEntity forumSection, String message)
        {
            if (forumSection == null)
            {
                return new ForumSectionExistsResult(false, message, null);
            }

            return new ForumSectionExistsResult(true, null, forumSection);
        }
    }
}

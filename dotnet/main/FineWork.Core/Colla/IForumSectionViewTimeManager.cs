using System;

namespace FineWork.Colla
{
    public interface IForumSectionViewTimeManager
    {
        ForumSectionViewEntity CreateFroumSectionViewTime(Guid forumSectionId, Guid staffId);
    }
}
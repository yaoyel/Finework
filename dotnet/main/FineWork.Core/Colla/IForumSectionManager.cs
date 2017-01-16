using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IForumSectionManager
    {
        ForumSectionEntity CreateForumSetcion(CreateForumSectionModel forumSectionModel);

        IEnumerable<ForumSectionEntity> FetchForumSectionByOrgIdWithSection(Guid orgId, ForumSections section);

        IEnumerable<ForumSectionEntity> FetchForumSectionByOrgId(Guid orgId);

        ForumSectionEntity FetchForumSectionByStaffId(Guid orgId, ForumSections section, string version);

        ForumSectionEntity FindById(Guid forumSectionId);

        ForumSections[] HasUnReadForumByStaffId(Guid staffId);

        string FetchVersionByForumSectionId(Guid forumSectionId);

        DateTime GetLastViewForumSectionTime(Guid staffId, ForumSections section);

        Tuple<Guid[],int[]> FetchIrrelevantForumsByStaff(Guid staffId,params ForumSections[] sections); 
    }
}

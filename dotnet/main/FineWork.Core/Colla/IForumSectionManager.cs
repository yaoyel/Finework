using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IForumSectionManager
    {
        ForumSectionEntity CreateForumSetcion(CreateForumSectionModel forumSectionModel);

        IEnumerable<ForumSectionEntity> FetchForumSectionByOrgId(Guid orgId, ForumSections section);

        ForumSectionEntity FetchForumSectionByStaffId(Guid orgId, ForumSections section,string version);

        ForumSectionEntity FindById(Guid forumSectionId);
    } 
}

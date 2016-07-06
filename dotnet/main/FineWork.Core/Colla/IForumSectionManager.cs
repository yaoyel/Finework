using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IForumSectionManager
    {
        ForumSectionEntity CreateForumSetcion(CreateForumSectionModel forumSectionModel);

        IEnumerable<ForumSectionEntity> FetchForumSectionByStaffId(Guid staffId, int sectionId);

        
    } 
}

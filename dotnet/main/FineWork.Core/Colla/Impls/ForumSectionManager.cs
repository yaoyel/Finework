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
    public class ForumSectionManager:AefEntityManager<ForumSectionEntity, Guid>,IForumSectionManager
    {
        public ForumSectionManager(ISessionProvider<AefSession> sessionProvider, 
            IStaffManager staffManager) : base(sessionProvider)
        { 
            Args.NotNull(staffManager, nameof(staffManager));
             
            m_StaffManager = staffManager;
        }
         
        private readonly IStaffManager m_StaffManager;

        public ForumSectionEntity CreateForumSetcion(CreateForumSectionModel forumSectionModel)
        {
            Args.NotNull(forumSectionModel, nameof(forumSectionModel));

            var staff = StaffExistsResult.Check(this.m_StaffManager, forumSectionModel.StaffId).ThrowIfFailed().Staff;
            var forumSectionEntity=new ForumSectionEntity();
            forumSectionEntity.Content = forumSectionModel.Content;
            forumSectionEntity.Staff = staff;
            forumSectionEntity.Section = forumSectionModel.SectionId;
            forumSectionEntity.Id = Guid.NewGuid();

            this.InternalInsert(forumSectionEntity);
            return forumSectionEntity; 
        }

        public ForumSectionEntity  FetchForumSectionByStaffId(Guid staffId, ForumSections section,
            string version)
        {
            var forumSections = this.InternalFetch(
                    p => p.Staff.Id == staffId && p.Section == section)
                    .OrderBy(p => p.CreatedAt)
                    .ToList();

            if (string.IsNullOrEmpty(version))
                return forumSections.LastOrDefault();


            var versionSplit = version.Substring(0, version.Length - 1).Split('年');
            var yearOfVersion = int.Parse( versionSplit[0]);
            var orderOfVersion = int.Parse(versionSplit[1]);

            var forumSectionsByVersion = forumSections.Where(p => p.CreatedAt.Year == yearOfVersion).ToList();

            if (orderOfVersion > forumSectionsByVersion.Count) return null;

            return forumSectionsByVersion[orderOfVersion - 1];
        }

        public IEnumerable<ForumSectionEntity> FetchForumSectionByOrgId(Guid orgId, ForumSections section)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId && p.Section==section).OrderBy(p=>p.CreatedAt);
        }

        public ForumSectionEntity FindById(Guid forumSectionId)
        {
            return this.InternalFind(forumSectionId);
        }
    }
}
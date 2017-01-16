using System;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class ForumSectionViewTimeManager : AefEntityManager<ForumSectionViewEntity, Guid>,
        IForumSectionViewTimeManager
    {
        public ForumSectionViewTimeManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IForumSectionManager forumSectionManager) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(forumSectionManager, nameof(forumSectionManager));

            m_StaffManager = staffManager;
            m_ForumSectionManager = forumSectionManager;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IForumSectionManager m_ForumSectionManager;

        public ForumSectionViewEntity CreateFroumSectionViewTime(Guid forumSectionId, Guid staffId)
        {
            var forumSection =
                ForumSectionExistsResult.Check(m_ForumSectionManager, forumSectionId).ThrowIfFailed().ForumSection;
            var staff = StaffExistsResult.Check(m_StaffManager, staffId).ThrowIfFailed().Staff;

            var viewTime =
                this.InternalFetch(p => p.Staff.Id == staffId && p.ForumSection.Id == forumSectionId).FirstOrDefault();

            if (viewTime == null)
            {
                viewTime = new ForumSectionViewEntity();
                viewTime.Id = Guid.NewGuid(); 
                viewTime.Staff = staff;
                viewTime.ForumSection = forumSection;
                this.InternalInsert(viewTime);
                return viewTime;
            }
            viewTime.CreatedAt = DateTime.Now;
            this.InternalUpdate(viewTime);

            return viewTime; 
        }
    }
}
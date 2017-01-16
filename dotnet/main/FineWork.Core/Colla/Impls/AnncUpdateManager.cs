using System;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class AnncUpdateManager : AefEntityManager<AnncUpdateEntity, Guid>, IAnncUpdateManager
    {
        public AnncUpdateManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IAnnouncementManager anncManager) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(anncManager, nameof(anncManager));

            m_StaffManager = staffManager;
            m_AnncManager = anncManager;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IAnnouncementManager m_AnncManager;
        public void CreateAnncUpdate(Guid anncId, Guid staffId)
        {
            var annc = AnncExistsResult.Check(this.m_AnncManager, anncId).ThrowIfFailed().Annc;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var anncUpdate=new AnncUpdateEntity();
            anncUpdate.Id = Guid.NewGuid();
            anncUpdate.Annc = annc;
            anncUpdate.Staff = staff;
            this.InternalInsert(anncUpdate);
        }

        public void DeleteAnncUpdatesByAnncId(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this.m_AnncManager, anncId).Annc;
            if(annc==null) return;

            var updates = annc.Updates.ToList();

            if(updates.Any())
                updates.ForEach(InternalDelete);
        }
    }
}
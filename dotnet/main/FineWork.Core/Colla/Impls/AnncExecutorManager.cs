using System;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class AnncExecutorManager : AefEntityManager<AnncExecutorEntity, Guid>, IAnncExecutorManager
    {
        public AnncExecutorManager(ISessionProvider<AefSession> sessionProvider,
            IAnnouncementManager anncoumentManager,
            IStaffManager staffManager,
            IPartakerManager partakerManager) : base(sessionProvider)
        {
            Args.NotNull(anncoumentManager, nameof(anncoumentManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(partakerManager, nameof(partakerManager));

            m_StaffManager = staffManager;
            m_AnnouncementManager = anncoumentManager;
            m_PartakerManager = partakerManager; 
        }

        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IPartakerManager m_PartakerManager;
        
        public AnncExecutorEntity CreateAnncExecutor(Guid anncId, Guid staffId, bool checkPartaker = true)
        {
            StaffEntity staff;
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;  
            var anncExecutor = AnncExecutorExistsResult.Check(this, anncId, staffId).AnncExecutor;

            if (checkPartaker)
                staff = PartakerExistsResult.CheckForStaff(annc.Task, staffId).ThrowIfFailed().Partaker.Staff;
            else
            {
                var partaker = PartakerExistsResult.CheckForStaff(annc.Task, staffId).Partaker;
                if (partaker != null)
                    staff = partaker.Staff;
                else
                {
                    staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                    m_PartakerManager.CreateCollabrator(annc.Task.Id, staff.Id);
                }
            }

            if (anncExecutor == null)
            {
                anncExecutor = new AnncExecutorEntity();
                anncExecutor.Annc = annc;
                anncExecutor.Staff =staff;
                anncExecutor.Id = Guid.NewGuid();
                this.InternalInsert(anncExecutor);
            }
             
            return anncExecutor;
        }

        public void DeleteAnncExecutor(Guid anncId, Guid staffId)
        {
            var anncExecutor = AnncExecutorExistsResult.Check(this, anncId, staffId).AnncExecutor;
            if(anncExecutor==null) return;

            this.InternalDelete(anncExecutor);
        }

        public void DeleteAnncExecutorByAnncId(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).Annc;
            if (annc != null)
            {
                foreach (var executor in annc.Executors.ToList())
                {
                    this.InternalDelete(executor);
                }
            }
        }

        public AnncExecutorEntity FindByAnncIdWithStaffId(Guid anncId, Guid staffId)
        {
            return this.InternalFetch(p => p.Annc.Id == anncId && p.Staff.Id == staffId).FirstOrDefault();
        }
    }
}
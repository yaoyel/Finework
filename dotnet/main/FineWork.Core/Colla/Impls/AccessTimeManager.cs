using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public  class AccessTimeManager : AefEntityManager<AccessTimeEntity, Guid>, IAccessTimeManager
    {
        public AccessTimeManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager) : base(sessionProvider)
        {
            m_StaffManager = staffManager;
        }

        private readonly IStaffManager m_StaffManager;

        public AccessTimeEntity FindAccessTimeByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId).FirstOrDefault();
        }

        public AccessTimeEntity FindAccessTimeById(Guid accessTimeId)
        {
            return this.InternalFind(accessTimeId);
        }

        public AccessTimeEntity CreateAccessTimeEntity(Guid staffId, DateTime? lastEntryOrgAt, DateTime? lastViewMomentAt,
            DateTime? lastViewCommentAt,DateTime? lastViewNewsAt)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var result = new AccessTimeEntity();
            result.Id = Guid.NewGuid();
            result.Staff = staff;
            result.LastEnterOrgAt = lastEntryOrgAt;
            result.LastViewCommentAt = lastViewCommentAt;
            result.LastViewMomentAt = lastViewMomentAt;
            result.LastViewNewsAt = lastViewNewsAt;
            this.InternalInsert(result);

            return result; 
        }

        public void UpdateLastVoewMomentTime( Guid staffId, DateTime lastViewMomentAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, lastViewMomentAt, null,null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewMomentAt = lastViewMomentAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastEnterOrgTime(Guid staffId, DateTime lastEntryOrgAt)
        {  
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, lastEntryOrgAt, null, null,null);
            else
            { 
                this.Session.DbContext.Entry(accessTime).State= EntityState.Modified;
                accessTime.LastEnterOrgAt = lastEntryOrgAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewCommentTime(Guid staffId,DateTime lastViewCommentAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, lastViewCommentAt,null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewCommentAt = lastViewCommentAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewNewsTime(Guid staffId, DateTime lastViewNewsAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null,lastViewNewsAt);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewNewsAt = lastViewNewsAt;
                this.InternalUpdate(accessTime);
            }
        }
    }
}

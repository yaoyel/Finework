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
    public class AccessTimeManager : AefEntityManager<AccessTimeEntity, Guid>, IAccessTimeManager
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

        public AccessTimeEntity CreateAccessTimeEntity(Guid staffId, DateTime? lastEntryOrgAt,
            DateTime? lastViewMomentAt,
            DateTime? lastViewCommentAt, DateTime? lastViewNewsAt, DateTime? lastViewForumAt,
            DateTime? lastViewForumCommentAt,
            DateTime? lastViewMissinAt, DateTime? lastViewVisionAt, DateTime? lastViewValuesAt,
            DateTime? lastViewStrategyAt, DateTime? lastViewOrgGovernanceAt)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var result = new AccessTimeEntity();
            result.Id = Guid.NewGuid();
            result.Staff = staff;
            result.LastEnterOrgAt = lastEntryOrgAt;
            result.LastViewCommentAt = lastViewCommentAt;
            result.LastViewMomentAt = lastViewMomentAt;
            result.LastViewNewsAt = lastViewNewsAt;
            result.LastViewForumAt = lastViewForumAt;
            result.LastViewForumCommentAt = lastViewForumCommentAt;
            result.LastViewMissinAt = lastViewMissinAt;
            result.LastViewVisionAt = lastViewVisionAt;
            result.LastViewValuesAt = lastViewValuesAt;
            result.LastViewStrategyAt = lastViewStrategyAt;
            result.LastViewOrgGovernanceAt = lastViewOrgGovernanceAt; 
            this.InternalInsert(result);

            return result;
        }

        public void UpdateLastVoewMomentTime(Guid staffId, DateTime lastViewMomentAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, lastViewMomentAt, null, null, null, null, null, null, null,
                    null, null);
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
                this.CreateAccessTimeEntity(staffId, lastEntryOrgAt, null, null, null, null, null, null, null, null,
                    null, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastEnterOrgAt = lastEntryOrgAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewCommentTime(Guid staffId, DateTime lastViewCommentAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, lastViewCommentAt, null, null, null, null, null, null,
                    null, null);
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
                this.CreateAccessTimeEntity(staffId, null, null, null, lastViewNewsAt, null, null, null, null, null,
                    null, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewNewsAt = lastViewNewsAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewForumTime(Guid staffId, DateTime lastViewForumAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, lastViewForumAt, null, null, null, null,
                    null, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewForumAt = lastViewForumAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewMissinTime(Guid staffId, DateTime lastViewMissinAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, null, null, lastViewMissinAt, null, null,
                    null, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewMissinAt = lastViewMissinAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewVisionTime(Guid staffId, DateTime lastViewVisionAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, null, null, null, lastViewVisionAt, null,
                    null,null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewVisionAt = lastViewVisionAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewValuesTime(Guid staffId, DateTime lastViewValuesAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, null, null, null, null, lastViewValuesAt,
                    null, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewValuesAt = lastViewValuesAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewStrategyTime(Guid staffId, DateTime lastViewStrategyAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, null, null, null, null, null,
                    lastViewStrategyAt, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewStrategyAt = lastViewStrategyAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewOrgGovernanceTime(Guid staffId, DateTime lastViewOrgGovernanceAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, null, null, null, null, null, null,
                    lastViewOrgGovernanceAt);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewOrgGovernanceAt = lastViewOrgGovernanceAt;
                this.InternalUpdate(accessTime);
            }
        }

        public void UpdateLastViewForumCommentTime(Guid staffId, DateTime lastViewForumCommentAt)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this, staffId).AccessTime;
            if (accessTime == null)
                this.CreateAccessTimeEntity(staffId, null, null, null, null, null, lastViewForumCommentAt, null, null,
                    null, null, null);
            else
            {
                this.Session.DbContext.Entry(accessTime).State = EntityState.Modified;
                accessTime.LastViewForumCommentAt = lastViewForumCommentAt;
                this.InternalUpdate(accessTime);
            }
        }
 
    }
}

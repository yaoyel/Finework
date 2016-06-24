using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Security.Checkers;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Web;
using System.IO;
using FineWork.Avatar;
using FineWork.Common;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class OrgManager : AefEntityManager<OrgEntity, Guid>, IOrgManager
    {
        public OrgManager(ISessionProvider<AefSession> dbContextProvider, ILazyResolver<IStaffManager> staffManagerResolver, IAvatarManager avatarManager)
            :base(dbContextProvider)
        {
            if (staffManagerResolver == null) throw new ArgumentNullException(nameof(staffManagerResolver));
            m_StaffManagerResolver = staffManagerResolver;
            this.AvatarManager = avatarManager;
        }

        private readonly ILazyResolver<IStaffManager> m_StaffManagerResolver;

        private IStaffManager StaffManager { get { return m_StaffManagerResolver.Required; } }

        private IAvatarManager AvatarManager { get; }
         
        public OrgEntity CreateOrg(IAccount account, String orgName, string avatarUrl)
        {
            if (String.IsNullOrEmpty(orgName)) throw new ArgumentException("name is null or empty.", nameof(orgName));

            OrgNotExistsResult.Check(this, orgName).ThrowIfFailed(); 

            OrgEntity result = new OrgEntity();
            result.Id = Guid.NewGuid();
            result.Name = orgName;
            result.IsInvEnabled = true;
            result.CreatedAt = DateTime.Now; 
            this.InternalInsert(result);  
            return result;
        }

        public void ChangeOrgName(OrgEntity org, String newName)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));
            if (String.IsNullOrEmpty(newName)) throw new ArgumentException("newName is null or empty.", nameof(newName));
            OrgNotExistsResult.Check(this, newName).ThrowIfFailed(); 

            org.Name = newName;
            this.InternalUpdate(org);
        }

        public void ChangeOrgInvEnabled(OrgEntity org, bool newInvStatus)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));
            OrgExistsResult.Check(this, org.Id).ThrowIfFailed();

            org.IsInvEnabled = newInvStatus;
            this.InternalUpdate(org); 
        }

        public void ChangeOrgAdmin(OrgEntity org, Guid newAdminStaffId)
        {
            if (org == null) throw new ArgumentNullException(nameof(org));
            OrgExistsResult.Check(this, org.Id).ThrowIfFailed();
            var staff = StaffExistsResult.Check(this.StaffManager, newAdminStaffId).ThrowIfFailed().Staff;
            if (staff.Org.Id != org.Id)
            {
                throw new FineWorkException($"The staffId [{newAdminStaffId}] does not belong to org [{org.Id}]");
            }
            org.AdminStaff = staff;
            this.InternalUpdate(org);
        }

        public OrgEntity FindOrg(Guid orgId)
        {
            return this.InternalFind(orgId);
        }

        public OrgEntity FindOrgByName(String orgName)
        {
            if (String.IsNullOrEmpty(orgName)) throw new ArgumentException("name is null or empty.", nameof(orgName));
            return this.InternalFetch(x => x.Name == orgName).SingleOrDefault();
        }

        public ICollection<OrgEntity> FetchOrgs()
        {
            return this.InternalFetch(q => q);
        }

        public IEnumerable<OrgEntity> FetchOrgsByAccount(Guid accountId)
        {
            var staffs = StaffManager.FetchStaffsByAccount(accountId).Where(p=>p.IsEnabled);
            var orgs = staffs.Select(staff => staff.Org);
            return orgs;
        }

        public IEnumerable<OrgEntity> FetchOrgsByStaff(Guid staffId)
        {
            var staffs = this.StaffManager.FetchAllStaff();
            var orgs = staffs.Where(staff => staff.Id == staffId
             &&　staff.IsEnabled).Select(staff => staff.Org);
            return orgs;
        }

        public void UploadOrgAvatar(Stream stream, Guid orgId, string contentType)
        {
            AvatarManager.CreateAvatars(KnownAvatarOwnerTypes.Orgs, orgId, stream);
            var org = OrgExistsResult.Check(this, orgId).ThrowIfFailed().Org;
            this.UpdateOrgWithNewStamp(org);
        }

        protected void UpdateOrgWithNewStamp(OrgEntity org)
        {
            if (org == null) throw new ArgumentNullException("org");
            org.SecurityStamp = Guid.NewGuid().ToString("D");
            this.InternalUpdate(org);
        }

    }
}

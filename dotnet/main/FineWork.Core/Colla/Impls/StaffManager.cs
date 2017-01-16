using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Security.Checkers;
using FineWork.Security.Repos.Aef;

namespace FineWork.Colla.Impls
{
    public class StaffManager : AefEntityManager<StaffEntity, Guid>, IStaffManager
    {
        public StaffManager(ISessionProvider<AefSession> dbContextProvider, IAccountManager accountManager, IOrgManager orgManager)
            :base(dbContextProvider)
        {
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            this.AccountManager = accountManager;
            this.OrgManager = orgManager;
        }

        private IAccountManager AccountManager { get; set; }

        private IOrgManager OrgManager { get; set; }

        public StaffEntity CreateStaff(Guid orgId, Guid accountId, String staffName)
        {
            var org = OrgExistsResult.Check(this.OrgManager, orgId).ThrowIfFailed().Org;
            var account = AccountExistsResult.Check(this.AccountManager, accountId).ThrowIfFailed().Account;

            StaffNotExistsResult.Check(this, org.Id, account.Id).ThrowIfFailed();
            StaffNotExistsResult.Check(this, org.Id, staffName).ThrowIfFailed();

            StaffEntity staff = new StaffEntity();
            staff.Id = Guid.NewGuid();
            staff.Org = org;
            staff.Account = (AccountEntity)account;
            staff.Name = staffName;
            staff.IsEnabled = true;
            this.InternalInsert(staff);

            return staff;
        }

        public StaffEntity FindStaff(Guid staffId)
        {
            return this.InternalFind(staffId);
        }

        public IEnumerable<StaffEntity> FetchStaffsByName(Guid orgId,string name)
        {
            return this.InternalFetch(p =>p.Org.Id==orgId &&  p.Name.Contains(name));
        }

        public IList<StaffEntity> FetchStaffsByIds(params Guid[] staffIds)
        {
            return this.InternalFetch(p => staffIds.Contains(p.Id));  
        }

        public StaffEntity FindStaffByNameInOrg(Guid orgId, string staffName)
        {
            return this.InternalFetch(x => x.Org.Id == orgId && x.Name == staffName).SingleOrDefault();
        }

        public StaffEntity FindStaffByOrgAccount(Guid orgId, Guid accountId)
        {
            return this.InternalFetch(x => x.Org.Id == orgId && x.Account.Id == accountId).SingleOrDefault();
        }
        
        public IList<StaffEntity> FetchStaffsByOrg(Guid orgId,bool? isEnabled)
        {
           var staffs= this.InternalFetch(x =>x.Org.Id == orgId).ToList();
            if (isEnabled != null)
                staffs = staffs.Where(p => p.IsEnabled == isEnabled.Value).ToList();
            return staffs; 
        }

        public IList<StaffEntity> FetchStaffsByAccount(Guid accountId)
        {
            return this.InternalFetch(x => x.Account.Id == accountId).ToList();
        }  
        public IEnumerable<StaffEntity> FetchAllStaff()
        {
            return this.InternalFetchAll();
        }

        public void UpdateStaff(StaffEntity staff)
        {
            Args.NotNull(staff, nameof(staff));
            this.InternalUpdate(staff);
        }
    }
}
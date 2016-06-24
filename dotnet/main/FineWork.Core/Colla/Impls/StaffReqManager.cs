using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Security.Checkers;
using FineWork.Security.Repos.Aef;
namespace FineWork.Colla.Impls
{
    public class StaffReqManager : AefEntityManager<StaffReqEntity, Guid>, IStaffReqManager
    {
        public StaffReqManager(ISessionProvider<AefSession> dbContextProvider,
            IAccountManager accountManager, IOrgManager orgManager, IStaffManager staffManager)
            : base(dbContextProvider)
        {
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));

            this.AccountManager = accountManager;
            this.OrgManager = orgManager;
            this.StaffManager = staffManager;
        }

        private IAccountManager AccountManager { get; set; }

        private IOrgManager OrgManager { get; set; }

        private IStaffManager StaffManager { get; set; }

        public StaffReqEntity FindStaffReqById(Guid staffReqId)
        {
            return this.InternalFind(staffReqId);
        }

        public StaffReqEntity CreateStaffReq(Guid accountId, Guid orgId, string message)
        {  
            var org = OrgExistsResult.Check(this.OrgManager, orgId).ThrowIfFailed().Org;
            var account = AccountExistsResult.Check(this.AccountManager, accountId).ThrowIfFailed().Account;

            StaffNotExistsResult.Check(this.StaffManager, org.Id, account.Id).ThrowIfFailed();
            var staffReq = StaffReqExistsResult.Check(this, orgId, accountId).StaffReq;

            if (staffReq != null)
            {
                staffReq.ReviewStatus = ReviewStatuses.Unspecified;
                staffReq.CreatedAt = DateTime.Now;
                this.InternalUpdate(staffReq);
            }
            else
            {
                staffReq = new StaffReqEntity()
                {
                    Id = Guid.NewGuid(),
                    Org = org,
                    Account = (AccountEntity) account,
                    Message = message,
                    CreatedAt = DateTime.Now,
                    ReviewStatus = ReviewStatuses.Unspecified
                };
                this.InternalInsert(staffReq);
            }
            return staffReq;

        }

        public IList<StaffReqEntity> FetchStaffReqsByOrg(Guid orgId)
        {
            var result = this.InternalFetch(p => p.Org.Id == orgId
                                                 && p.ReviewStatus == ReviewStatuses.Unspecified); 
            return result;
        }

        public IList<StaffReqEntity> FetchStaffReqsByAccount(Guid accountId)
        {
            return this.InternalFetch(p => p.Account.Id == accountId);
        }

        public StaffEntity ChangeReviewStatus(StaffReqEntity staffReq, ReviewStatuses newRevStatus)
        {
            StaffNotExistsResult.Check(this.StaffManager, staffReq.Org.Id, staffReq.Account.Id)
                .ThrowIfFailed();

            staffReq.ReviewStatus = newRevStatus;
            staffReq.ReviewAt = DateTime.Now;
            this.InternalUpdate(staffReq);
            if (newRevStatus == ReviewStatuses.Approved)
               return StaffManager.CreateStaff(staffReq.Org.Id, staffReq.Account.Id,staffReq.Account.Name);
            return null;
        }
    }
}

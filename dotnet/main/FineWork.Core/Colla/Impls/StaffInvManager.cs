using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Security.Checkers;
using FineWork.Security.Repos.Aef;
using FineWork.Core.Colla.Models;
using FineWork.Net.Sms;

namespace FineWork.Colla.Impls
{
    public class StaffInvManager : AefEntityManager<StaffInvEntity, Guid>, IStaffInvManager
    {
        public StaffInvManager(ISessionProvider<AefSession> dbContextProvider,
            IAccountManager accountManager, IOrgManager orgManager, IStaffManager staffManager,
            ISmsService smsService)
            : base(dbContextProvider)
        {
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));

            this.AccountManager = accountManager;
            this.OrgManager = orgManager;
            this.StaffManager = staffManager;
            this.SmsService = smsService;
        }

        private IAccountManager AccountManager { get; set; }

        private IOrgManager OrgManager { get; set; }

        private IStaffManager StaffManager { get; set; }

        private ISmsService SmsService { get; set; }

        public StaffInvEntity FindStaffInvById(Guid staffInvId)
        {
            return this.InternalFind(staffInvId);
        }


        public void ChangeReviewStatus(StaffInvEntity staffInv, ReviewStatuses newRevStatus)
        {
            var account =
                AccountExistsResult.CheckByPhoneNumber(this.AccountManager, staffInv.PhoneNumber)
                    .ThrowIfFailed()
                    .Account;

            StaffNotExistsResult.Check(this.StaffManager, staffInv.Org.Id, account.Id)
                .ThrowIfFailed();

            staffInv.ReviewStatus = newRevStatus;
            staffInv.ReviewAt = DateTime.Now;

            this.InternalUpdate(staffInv);

            if (newRevStatus == ReviewStatuses.Approved &&
                StaffNotExistsResult.Check(this.StaffManager, staffInv.Org.Id, account.Id).IsSucceed)
                StaffManager.CreateStaff(staffInv.Org.Id, account.Id,
                    string.IsNullOrEmpty(staffInv.StaffName) ? account.Name : staffInv.StaffName);
        }

        public void BulkCreateStuffInv(CreateStaffInvModel invStaffs)
        {
            if (!PermissionIsAdminResult.Check(this.OrgManager, invStaffs.OrgId, invStaffs.StaffId).IsSucceed)
                InvitationIsEnabledResult.Check(this.OrgManager, invStaffs.OrgId).ThrowIfFailed();

            //var org = OrgExistsResult.Check(this.OrgManager, invStaffs.OrgId).ThrowIfFailed().Org;

            invStaffs.Invitees.ToList().ForEach(p =>
            {
                this.CreateStaffInv(p.Item2, p.Item1, invStaffs.InviterName, invStaffs.OrgId);

                var account = AccountExistsResult.CheckByPhoneNumber(this.AccountManager, p.Item2).Account;
                if (account == null)
                {
                    //发送短信通知
                    var attrs = new Dictionary<string, object>()
                    {
                        ["AccountName"] = p.Item2,

                        ["inviter"] = "下载地址:app.chinahrd.net/Download"
                    };
                    SmsService.SendMessage(p.Item2, "invitestaff ", attrs);
                }
            });
        }


        public StaffInvEntity CreateStaffInv(string phoneNumber,string staffName,string inviterName, Guid orgId)
        {
            var account = AccountExistsResult.CheckByPhoneNumber(this.AccountManager, phoneNumber).Account;
            var org = OrgExistsResult.Check(this.OrgManager, orgId).ThrowIfFailed().Org;

            if (!string.IsNullOrEmpty(staffName))
            {
                Args.MaxLength(staffName, 18, nameof(staffName), "成员名称");
                var regString = @"^[\u4e00-\u9fa5a-zA-Z]{1,18}$";
                if (!Regex.IsMatch(staffName, regString))
                {
                    throw new ArgumentException($"成员名称不允许有标点符号、数字及特殊字符");
                }
            }
            StaffInvEntity staffInv =  StaffInvExistsResult.CheckByPhoneNumber(this, orgId, phoneNumber).StaffInv;

            if (account!=null)
            { 
                StaffNotExistsResult.Check(this.StaffManager, orgId, account.Id).ThrowIfFailed();
            }

            //已存在邀请，直接将邀请人姓名添加至InvitersName字段后
            if (staffInv != null)
            {
                if (!staffInv.InviterNames.Contains(inviterName))
                    staffInv.InviterNames = string.Concat(inviterName, ",", staffInv.InviterNames);

                staffInv.CreatedAt = DateTime.Now;
                this.InternalUpdate(staffInv);
            }
            else
            {
                staffInv = new StaffInvEntity()
                {
                    Id = Guid.NewGuid(),
                    PhoneNumber=phoneNumber,
                    Org = org,
                    Message = "",
                    StaffName=staffName,
                    CreatedAt = DateTime.Now,
                    ReviewStatus = ReviewStatuses.Unspecified,
                    InviterNames = inviterName
                };
                this.InternalInsert(staffInv);
            }
            return staffInv;
        }

        public  StaffInvEntity  FindStaffInvByOrgAccount(Guid orgId, Guid accountId)
        {
            var account = AccountExistsResult.Check(this.AccountManager, accountId).ThrowIfFailed().Account;
            return this.InternalFetch(p => p.PhoneNumber == account.PhoneNumber && p.ReviewStatus == ReviewStatuses.Unspecified)
                     .FirstOrDefault(p => p.Org.Id == orgId);  
        }


        public StaffInvEntity FindStaffInvByOrgWithPhoneNumber(Guid orgId, string phoneNumber)
        {
            return this.InternalFetch(p => p.PhoneNumber == phoneNumber && p.Org.Id == orgId).FirstOrDefault();
        }

        public ICollection<StaffInvEntity> FetchStaffInvsByAccount(Guid accountId)
        {
            //查找已经加入的组织
            var account = AccountExistsResult.Check(this.AccountManager, accountId).ThrowIfFailed().Account;
            var joinedOrgs = this.StaffManager.FetchStaffsByAccount(accountId).Select(p => p.Org.Id);
            return this.InternalFetch(p => p.PhoneNumber== account.PhoneNumber
            && p.ReviewStatus == ReviewStatuses.Unspecified 
            && !joinedOrgs.Contains(p.Org.Id)); 
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Mvc;
using FineWork.Core.Colla.Models;
using FineWork.Colla.Checkers;
using Microsoft.AspNet.Authorization;
using AppBoot.Checks;
using Microsoft.AspNet.Http;
using System.IO;
using AppBoot.Transactions;
using FineWork.Web.WebApi.Core;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Orgs")]
    [Authorize("Bearer")]
    public class OrgController : FwApiController
    {
        public OrgController(ISessionScopeFactory sessionScopeFactory,
            IStaffManager staffManager,
            IOrgManager orgManager,
            IAccountManager accountManager
            ) : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            m_SessionScopeFactory = sessionScopeFactory;
            m_OrgManager = orgManager;
            m_StaffManager = staffManager;
            m_AccountManager = accountManager;
        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;

        private readonly IOrgManager m_OrgManager;

        private readonly IStaffManager m_StaffManager;

        private readonly IAccountManager m_AccountManager;

        /// <summary>
        /// 获取所有组织机构信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchOrgs")]
        public IEnumerable<OrgViewModel> FetchOrgs()
        {
            var orgs = m_OrgManager.FetchOrgs();
            return orgs.Select(org => org.ToViewModel());
        }


        [HttpGet("FetchOrgsByAccountId")]
        public IEnumerable<OrgViewModel> FetchOrgsByAccountId()
        {
            var orgs = m_OrgManager.FetchOrgsByAccount(this.AccountId);
            return orgs.Select(org => org.ToViewModel());
        }

        [HttpGet("FindOrgById")]
        public OrgViewModel FindOrgById(Guid orgId)
        {
            return m_OrgManager.FindOrg(orgId).ToViewModel();
        }

        [HttpGet("FetchOrgsByStaffId")]
        public IEnumerable<OrgViewModel> FetchOrgsByStaffId(Guid staffId)
        {
            return m_OrgManager.FetchOrgsByStaff(staffId)
                .Select(s => s.ToViewModel());
        }

        [HttpGet("FetchOrgsByName")]
        public IEnumerable<OrgViewModel> FetchOrgsByName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name));

            return m_OrgManager
                .FetchOrgs()
                .Where(p => p.Name.Contains(name.Trim()))
                .Select(p => p.ToViewModel());
        }

        [HttpPost("CreateOrg")]
        [DataScoped(true)]
        public OrgViewModel CreateOrg(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name");

            //创建组织
            var org = m_OrgManager.CreateOrg(null, name, null);

            //创建员工
            var account = m_AccountManager.FindAccount(this.AccountId);
            var staff = this.m_StaffManager.CreateStaff(org.Id, account.Id, account.Name);

            //设为管理员
            this.m_OrgManager.ChangeOrgAdmin(org, staff.Id);

            return org.ToViewModel();
        }

        [HttpPost("UpdateOrgName")]
        [DataScoped(true)]
        public void ChangeOrgName(Guid orgId, string newName)
        {
            if (string.IsNullOrEmpty(newName)) throw new ArgumentException(nameof(newName));

            PermissionIsAdminResult.Check(m_StaffManager, orgId, this.AccountId).ThrowIfFailed();
            m_OrgManager.ChangeOrgName(m_OrgManager.FindOrg(orgId), newName);
        }

        [HttpPost("UpdateOrgInvStatus")]
        [DataScoped(true)]
        public IActionResult ChangeOrgInvStatus(Guid orgId, bool newInvStatus)
        {
            PermissionIsAdminResult.Check(m_StaffManager, orgId, this.AccountId).ThrowIfFailed();
            m_OrgManager.ChangeOrgInvEnabled(m_OrgManager.FindOrg(orgId), newInvStatus);
            return new HttpStatusCodeResult((int)System.Net.HttpStatusCode.NoContent);
        }

        [HttpPost("UpdateOrgAdmin")]
        [DataScoped(true)]
        public void ChangeOrgAdmin(Guid orgId, Guid newAdminStaffId)
        {
            PermissionIsAdminResult.Check(m_StaffManager, orgId, this.AccountId).ThrowIfFailed();

            m_OrgManager.ChangeOrgAdmin(m_OrgManager.FindOrg(orgId), newAdminStaffId);
        }

        [HttpPost("UploadOrgAvatar")]
        public void UploadOrgAvatar(IFormFile file, Guid orgId)
        {
            if (file == null) throw new ArgumentException(nameof(file));

            if (!file.ContentType.StartsWith("image"))
                throw new ArgumentException("Unsupported file type!");

            OrgExistsResult.Check(this.m_OrgManager, orgId).ThrowIfFailed();

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                m_OrgManager.UploadOrgAvatar(reader.BaseStream, orgId, file.ContentType);
            }
        }
    }
}

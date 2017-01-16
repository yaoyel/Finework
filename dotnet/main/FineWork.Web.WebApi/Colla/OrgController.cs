using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Orgs")]
    [Authorize("Bearer")]
    public class OrgController : FwApiController
    {
        public OrgController(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IOrgManager orgManager,
            IAccountManager accountManager,
            IInvCodeManager invCodeManager,
            IHostingEnvironment hostEvn
            ) : base(sessionProvider)
        {
            if (orgManager == null) throw new ArgumentNullException(nameof(orgManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (invCodeManager == null) throw new ArgumentNullException(nameof(invCodeManager));
            m_OrgManager = orgManager;
            m_StaffManager = staffManager;
            m_AccountManager = accountManager;
            m_InvCodeManager = invCodeManager;
            m_HostEvn = hostEvn;
        }

        private readonly IOrgManager m_OrgManager;

        private readonly IStaffManager m_StaffManager;

        private readonly IAccountManager m_AccountManager;

        private readonly IInvCodeManager m_InvCodeManager;

        private readonly IHostingEnvironment m_HostEvn;
        
        /// <summary>
        /// 获取所有组织机构信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchOrgs")]
        [AllowAnonymous]
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
        public IActionResult FindOrgById(Guid orgId)
        {
            var org = m_OrgManager.FindOrg(orgId);

            if (org == null)
                return new HttpNotFoundObjectResult(orgId);

            return new ObjectResult(org.ToViewModel());
        }

        [HttpGet("FetchOrgsByStaffId")]
        public IActionResult FetchOrgsByStaffId(Guid staffId)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).Staff;
            if (staff != null) return new ObjectResult( staff.Org.ToViewModel());
            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FetchOrgsByName")]
        public IActionResult FetchOrgsByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new BadRequestObjectResult("组织名称不可以为空."); 

            var result= m_OrgManager
                .FetchOrgs()
                .Where(p => p.Name.Contains(name.Trim()))
                .Select(p => p.ToViewModel());

            return new ObjectResult(result);

        }

        [HttpPost("CreateOrg")]
        //[DataScoped(true)]
        public IActionResult CreateOrg(string name,string invCode)
        {
            OrgViewModel orgViewModel;
            using (var tx = TxManager.Acquire())
            {
                if (string.IsNullOrEmpty(name)) return new BadRequestObjectResult("组织名称不可以为空。");
                Args.MaxLength(name, 64, nameof(name), "组织名称");
                if (string.IsNullOrEmpty(invCode)) return new BadRequestObjectResult("组织邀请码不可以为空。");

                var code = new InvCodeEntity();
                if (invCode != "chinahrd")
                    //验证邀请码是否正确
                    code = InvCodeExistResult.Check(this.m_InvCodeManager, invCode).ThrowIfFailed().Code;

                //创建组织
                var org = m_OrgManager.CreateOrg(null, name, null);

                //创建员工
                var account = m_AccountManager.FindAccount(this.AccountId);
                var staff = this.m_StaffManager.CreateStaff(org.Id, account.Id, account.Name);

                //设为管理员
                this.m_OrgManager.ChangeOrgAdmin(org, staff.Id);

                if (invCode != "chinahrd")
                    //更新邀请码状态
                {
                    code.Org = org;
                    code.ExpiredAt = DateTime.Now;
                    m_InvCodeManager.UpdateInvCode(code);
                }


                orgViewModel = org.ToViewModel();
                tx.Complete();
               
            }

            //上传默认图像
            Task.Factory.StartNew(() =>
            {
                var orgId = orgViewModel.Id;
                var path = Path.Combine(m_HostEvn.WebRootPath, "Avatar", "default", "org.png");

                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    m_OrgManager.UploadOrgAvatar(fileStream, orgId, "Image/jpeg");
                }
               
            });
            return new ObjectResult(orgViewModel);
        }

        [HttpPost("UpdateOrgName")]
        //[DataScoped(true)]
        public IActionResult ChangeOrgName(Guid orgId, string newName)
        {
            using (var tx = TxManager.Acquire())
            {
                if (string.IsNullOrEmpty(newName)) return new BadRequestObjectResult("组织名称不可以为空。");
                Args.MaxLength(newName, 64, nameof(newName),"组织名称");

                PermissionIsAdminResult.Check(m_StaffManager, orgId, this.AccountId).ThrowIfFailed();
                m_OrgManager.ChangeOrgName(m_OrgManager.FindOrg(orgId), newName);
                tx.Complete();
                return new HttpStatusCodeResult(200);
            }
        }

        [HttpPost("UpdateOrgInvStatus")]
        //[DataScoped(true)]
        public IActionResult ChangeOrgInvStatus(Guid orgId, bool newInvStatus)
        {
            using (var tx = TxManager.Acquire())
            {
                PermissionIsAdminResult.Check(m_StaffManager, orgId, this.AccountId).ThrowIfFailed();
                m_OrgManager.ChangeOrgInvEnabled(m_OrgManager.FindOrg(orgId), newInvStatus);
                tx.Complete();
                return new HttpStatusCodeResult((int) System.Net.HttpStatusCode.NoContent);
            }
        }

        [HttpPost("UpdateOrgAdmin")]
        //[DataScoped(true)]
        public void ChangeOrgAdmin(Guid orgId, Guid newAdminStaffId)
        {
            using (var tx = TxManager.Acquire())
            {
                PermissionIsAdminResult.Check(m_StaffManager, orgId, this.AccountId).ThrowIfFailed();

                m_OrgManager.ChangeOrgAdmin(m_OrgManager.FindOrg(orgId), newAdminStaffId);
                tx.Complete();
            }
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

        [HttpGet("CreateInvCodes")]
        [AllowAnonymous]
        public IList<string> CreateInvCodes(int len=6, int count = 100)
        {
            var codes = m_InvCodeManager.CreateInvCodes(len,count);
            return codes.Select(p => p.Id).ToList();
        }
         
    }
}

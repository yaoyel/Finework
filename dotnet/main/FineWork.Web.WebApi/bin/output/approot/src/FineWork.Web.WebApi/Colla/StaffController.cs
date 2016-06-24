using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using Microsoft.AspNet.Mvc;
using System.Net;
using FineWork.Web.WebApi.Core;
using AppBoot.Transactions;
using FineWork.Colla.Checkers;
using Microsoft.AspNet.Authorization;
using  AppBoot.Checks;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Staffs")]
    [Authorize("Bearer")]
    public class StaffController : FwApiController
    {
        public StaffController(ISessionScopeFactory sessionScopeFactory, IStaffManager staffManager,
            IPartakerManager partakerManager)
            : base(sessionScopeFactory)
        {
            Args.NotNull(sessionScopeFactory, nameof(sessionScopeFactory));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(partakerManager, nameof(partakerManager));

            this.m_SessionScopeFactory = sessionScopeFactory;
            this.StaffManager = staffManager;

        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;

        private IStaffManager StaffManager { get; }

        private IPartakerManager PartakerManager { get; set; }

        //Match: http://localhost:41969/api/Staff/FindById?staffId=85A1ABCC-4B30-4258-87B3-F37601A97185
        [HttpGet("FindById")]
        public IActionResult FindById(Guid staffId)
        {
            var staff = this.StaffManager.FindStaff(staffId);

            return staff != null
                ? new ObjectResult(staff.ToViewModel())
                : new HttpNotFoundObjectResult(staffId);
        }

        //Match: http://localhost:41969/api/Staff/FindByOrgAndAccount?orgId=47A65F3E-301B-4680-8575-C87477E2D01B&accountId=5A6EF736-567C-45F1-8459-2C381BB3BCFA
        [HttpGet("FindByOrgAndAccount")]
        public StaffViewModel FindByOrgAndAccount(Guid orgId, Guid accountId)
        {
            var staff = this.StaffManager.FindStaffByOrgAccount(orgId, accountId);
            return staff?.ToViewModel();
        }

        /* RESTful style:
        //Match: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B/ByName/Tom
        //Match: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B/ByName/
        //NotMatch: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B
        [HttpGet("ByOrg/{orgId}/ByName/{staffName?}")] 
        */
        //Match: http://localhost:41969/api/Staff/FetchByOrgAndName?orgId=47A65F3E-301B-4680-8575-C87477E2D01B&staffName=Tom
        [HttpGet("FetchByOrgAndStaffName")]
        public IEnumerable<StaffViewModel> FetchByOrgAndStaffName(Guid orgId, String staffName = null)
        {
            IEnumerable<StaffEntity> staffs = this.StaffManager.FetchStaffsByOrg(orgId);
            if (!String.IsNullOrEmpty(staffName))
                staffs = staffs.Where(x => x.Name == staffName);
            return staffs.Select(x => x.ToViewModel()).ToList();
        }

        //Match: http://localhost:41969/api/Staffs/FetchByOrgId?orgId=47A65F3E-301B-4680-8575-C87477E2D01B
        [HttpGet("FetchByOrgId")]
        public IEnumerable<StaffViewModel> FetchByOrgId(Guid orgId, bool isEnabled = true)
        {
            return this.StaffManager.FetchStaffsByOrg(orgId, isEnabled)
                .Select(p => p.ToViewModel());
        }

        //Match: http://localhost:41969/api/Staffs/FetchByAccountId?accountId=5A6EF736-567C-45F1-8459-2C381BB3BCFA
        [HttpGet("FetchByAccountId")]
        public IEnumerable<StaffViewModel> FetchByAccountId(Guid accountId)
        {
            var staffs = this.StaffManager.FetchStaffsByAccount(accountId);
            return staffs.Select(x => x.ToViewModel()).ToList();
        }


        /// <summary>
        /// 员工退出组织，管理员删除员工通用此接口 
        /// </summary>
        /// <param name="staffids"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        [HttpPost("UpdateStaffStatus")]
        [DataScoped(true)]
        public IActionResult ChangeStaffStatus([FromBody] IList<Guid> staffids, bool newStatus)
        {
            StaffManager.ChangeStaffStatus(staffids, newStatus);
            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
        }

        [HttpPost("ChangeStaffName")]
        [DataScoped(true)]
        public void ChangeStaffName(Guid staffId, string newStaffName)
        {
            Args.NotEmpty(newStaffName, nameof(newStaffName));

            var staff = StaffExistsResult.Check(this.StaffManager, staffId).ThrowIfFailed().Staff;

            StaffNotExistsResult.Check(this.StaffManager, staff.Org.Id, newStaffName).ThrowIfFailed();

            staff.Name = newStaffName;
            this.StaffManager.UpdateStaff(staff);
        }

        [HttpPost("ChangeStaffDepartment")]
        [DataScoped(true)]
        public void ChangeStaffDepartment(Guid staffId, string newDepartment)
        {
            Args.NotEmpty(newDepartment, nameof(newDepartment));

            var staff = StaffExistsResult.Check(this.StaffManager, staffId).ThrowIfFailed().Staff; 

            staff.Department = newDepartment;
            this.StaffManager.UpdateStaff(staff);
        } 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Web.WebApp.Models;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApp.ApiControllers
{
    [Route("api/Staff/")]
    public class StaffController  : FwApiController
    {
        public StaffController(ISessionScopeFactory sessionScopeFactory, IStaffManager staffManager)
        {
            Args.NotNull(sessionScopeFactory, nameof(sessionScopeFactory));
            Args.NotNull(staffManager, nameof(staffManager));

            this.SessionScopeFactory = sessionScopeFactory;
            this.StaffManager = staffManager;
        }

        private ISessionScopeFactory SessionScopeFactory { get; }

        private IStaffManager StaffManager { get; }

        //Match: http://localhost:41969/api/Staff/FindById?staffId=85A1ABCC-4B30-4258-87B3-F37601A97185
        [HttpGet("FindById")]
        public IActionResult FindById(Guid staffId)
        {
            using (var ss = this.SessionScopeFactory.CreateScope())
            {
                var staff = this.StaffManager.FindStaff(staffId);

                return staff != null 
                    ? new ObjectResult(staff.ToStaffViewModel()) 
                    : new HttpNotFoundObjectResult(staffId);
            }
        }

        //Match: http://localhost:41969/api/Staff/FindByOrgAndAccount?orgId=47A65F3E-301B-4680-8575-C87477E2D01B&accountId=5A6EF736-567C-45F1-8459-2C381BB3BCFA
        [HttpGet("FindByOrgAndAccount")]
        public IActionResult FindByOrgAndAccount(Guid orgId, Guid accountId)
        {
            using (var ss = this.SessionScopeFactory.CreateScope())
            {
                var staff = this.StaffManager.FindStaffByOrgAccount(orgId, accountId);

                return staff != null
                    ? new ObjectResult(staff.ToStaffViewModel())
                    : new HttpNotFoundObjectResult(new {orgId, accountId});
            }
        }

        /* RESTful style:
        //Match: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B/ByName/Tom
        //Match: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B/ByName/
        //NotMatch: http://localhost:41969/api/Staff/ByOrg/47A65F3E-301B-4680-8575-C87477E2D01B
        [HttpGet("ByOrg/{orgId}/ByName/{staffName?}")] 
        */
        //Match: http://localhost:41969/api/Staff/FetchByOrgAndName?orgId=47A65F3E-301B-4680-8575-C87477E2D01B&staffName=Tom
        [HttpGet("FetchByOrgAndName")] 
        public IEnumerable<StaffViewModel> FetchByOrgAndName(Guid orgId, String staffName = null)
        {
            using (var ss = this.SessionScopeFactory.CreateScope())
            {
                IEnumerable<StaffEntity> staffs = this.StaffManager.FetchStaffsByOrg(orgId);
                if (!String.IsNullOrEmpty(staffName))
                    staffs = staffs.Where(x => x.Name == staffName);
                return staffs.Select(x => x.ToStaffViewModel());
            }
        }

        //Match: http://localhost:41969/api/Staff/FetchByOrg?orgId=47A65F3E-301B-4680-8575-C87477E2D01B
        [HttpGet("FetchByOrg")]
        public IEnumerable<StaffViewModel> FetchByOrg(Guid orgId)
        {
            using (var ss = this.SessionScopeFactory.CreateScope())
            {
                var staffs = this.StaffManager.FetchStaffsByOrg(orgId);
                return staffs.Select(x => x.ToStaffViewModel());
            }
        }

        //Match: http://localhost:41969/api/Staff/FetchByAccount?accountId=5A6EF736-567C-45F1-8459-2C381BB3BCFA
        [HttpGet("FetchByAccount")]
        public IEnumerable<StaffViewModel> FetchByAccount(Guid accountId)
        {
            using (var ss = this.SessionScopeFactory.CreateScope())
            {
                var staffs = this.StaffManager.FetchStaffsByAccount(accountId);
                return staffs.Select(x => x.ToStaffViewModel());
            }
        }
    }
}

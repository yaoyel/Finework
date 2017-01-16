using System;
using System.Linq;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla;
using FineWork.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/pushlogs")]
    [Authorize("Bearer")]
    public class PushLogController : FwApiController
    {
        public PushLogController(ISessionProvider<AefSession> sessionProvider,
            IPushLogManager pushLogManager 
            ) : base(sessionProvider)
        {
            Args.NotNull(pushLogManager, nameof(pushLogManager));

            m_PushLogManager = pushLogManager;
        }

        private readonly IPushLogManager m_PushLogManager;

        [HttpGet("FetchPushLogsByStaffId")]
        public IActionResult FetchPushLogsByStaffId(Guid staffId,PushKinds[] pushKinds)
        {
            var pushLogs = m_PushLogManager.FetchPushLogsByStaffId(staffId).ToList();

            if (pushLogs.Any())
                return new ObjectResult(pushLogs.Select(p => p.ToViewModel()));

            return  new HttpNotFoundObjectResult(staffId);
        }

        [HttpPost("DeletePushLogs")]
        public IActionResult DeletePushLogs([FromBody]params Guid[] pushLogId)
        {
            if (!pushLogId.Any())
                throw new FineWorkException("请传入要删除的通知id");
            m_PushLogManager.DeletePushLogs(pushLogId);

            return new NoContentResult();
        }

        [HttpPost("DeletePushLogsByStaffId")]
        public IActionResult DeletePushLogsByStaffId(Guid staffId,PushKinds[] pushKinds)
        {
            m_PushLogManager.DeletePushLogsByStaffId(staffId,pushKinds);

            return new NoContentResult();
        }

        [HttpPost("ChangeViewedStatus")]
        public IActionResult ChangeViewedStatus(Guid pushLogId)
        {
            m_PushLogManager.ChanageViewedStatus(pushLogId);

            return new HttpOkResult();
        }

        [HttpGet("FetchUnReadPushLogs")]
        public IActionResult FetchUnReadPushLogs(Guid staffId)
        {
            var pushLogGroups = this.m_PushLogManager.FetchPushLogsByStaffId(staffId)
                .Where(p=>!p.IsViewed )
                .GroupBy(p => p.TargetType)
                .Select(p => new {kind = p.Key, count = p.Count()});

            if(pushLogGroups.Any())
                return new ObjectResult(pushLogGroups);

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FetchPushLogByKinds")]
        public IActionResult FetchPushLogByKinds(Guid staffId,PushKinds[] pushKinds,int? pageSize,int? page)
        {
            var pushlogs = this.m_PushLogManager.FetchPushLogsByKinds(staffId,pushKinds)
                .OrderByDescending(p=>p.CreatedAt)
                .Select(p=>p.ToViewModel())
                .AsQueryable().ToPagedList(page,pageSize);

            if(pushlogs.Data.Any())
                return new ObjectResult(pushlogs);

            return new HttpNotFoundObjectResult(pushKinds);
        }
    }
}
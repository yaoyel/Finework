using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Message;
using FineWork.Net.IM;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Configuration;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/PartakerInvs")]
    [Authorize("Bearer")]
    public class PartakerInvController : FwApiController
    {
        public PartakerInvController(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IStaffManager staffManager,
            IPartakerInvManager partakerInvManager,
            INotificationManager notificationManager, 
            IConfiguration config,
            IIMService imService)
            : base(sessionProvider)
        {
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (partakerInvManager == null) throw new ArgumentNullException(nameof(partakerInvManager));
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));
            if (config == null) throw new ArgumentException(nameof(config));
            if (imService == null) throw new ArgumentException(nameof(imService));

            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_PartakerInvManager = partakerInvManager;
            m_Configuration = config;
            m_NotificationManager = notificationManager;
            m_IIMService = imService;
        } 

        private readonly ITaskManager m_TaskManager; 
        private readonly IStaffManager m_StaffManager; 
        private readonly IPartakerInvManager m_PartakerInvManager; 
        private readonly INotificationManager m_NotificationManager;
        private readonly IConfiguration m_Configuration;
        private readonly IIMService m_IIMService;
         
        /// <summary>
        /// 直接将员工加入任务，不需要审核
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("QuickAdd")]
        //[DataScoped(true)]
        public List<PartakerInvViewModel> QuickAdd([FromBody] CreatePartakerInvModel model)
        {

            if (!model.InviteeStaffIds.Any())
                throw new ArgumentException("请添加被邀请的人");

            var task = TaskExistsResult.Check(m_TaskManager, model.TaskId).ThrowIfFailed().Task;
            var inviterPartaker =
                AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

            var inviteeStaffs = new List<StaffEntity>();
            var result = new List<PartakerInvViewModel>();
            using (var tx = TxManager.Acquire())
            {
                model.InviteeStaffIds.ToList().ForEach(p =>
                {

                    var inviteeStaff = StaffExistsResult.Check(m_StaffManager, p).ThrowIfFailed().Staff;
                    inviteeStaffs.Add(inviteeStaff);

                    PartakerInvIsEnabledResult.Check(task, inviterPartaker, model.PartakerKind).ThrowIfFailed();

                    var inviterStaff = inviterPartaker.Staff;
                    var partakerInv = m_PartakerInvManager.QuickAdd(task, inviterStaff, inviteeStaff, model.PartakerKind);
                    result.Add(partakerInv.ToViewModel());

                });
                tx.Complete();
            }
            //统一发送消息 
            Task.Factory.StartNew(async () =>
            {
               var message = string.Format(m_Configuration["LeanCloud:Messages:Task:Join"],string.Join(",",inviteeStaffs.Select(p=>p.Name)));
                await m_IIMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.Conversation.Id, task.Name, message);
            });

            return result;
        } 

        [HttpPost("ReviewPartakerInv")]
        public IActionResult ReviewPartakerInv(Guid partakerInvId, ReviewStatuses newRevStatus)
        {
            var partakerInv =
                PartakerInvExistsResult.CheckPending(this.m_PartakerInvManager, partakerInvId)
                    .ThrowIfFailed()
                    .PartakerInv;
            var partakerInvEntity= this.m_PartakerInvManager.ReviewPartakerInv(partakerInv, newRevStatus);


            //发送群通知
            if (newRevStatus == ReviewStatuses.Approved)
            {
                var message = string.Format(m_Configuration["LeanCloud:Messages:Task:Join"], partakerInvEntity.Staff.Name);
                m_IIMService.SendTextMessageByConversationAsync(partakerInvEntity.Task.Id, this.AccountId, partakerInvEntity.Task.Conversation.Id, partakerInvEntity.Task.Name, message);
            }

            return new HttpStatusCodeResult(200);
        }

        [HttpGet("FetchPartakerInvsByStaff")]
        public IActionResult FetchPartakerInvsByStaff(Guid staffId)
        {
            var partakerInvs = this.m_PartakerInvManager.FetchPendingPartakerInvsByStaff(staffId).ToList();
            if (partakerInvs.Any())
                return new ObjectResult( partakerInvs.Select(p => p.ToViewModel()));
            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FetchSentPartakertInvsByStaff")]
        public IActionResult FetchSentPartakertInvsByStaff(Guid staffId)
        {
            var partakerInvs = this.m_PartakerInvManager.FetchSentPartakerInvsByStaff(staffId).ToList();
            if (partakerInvs.Any())
                return new ObjectResult(partakerInvs.Select(p => p.ToViewModel()));
            return new HttpNotFoundObjectResult(staffId);
        }
    }
}

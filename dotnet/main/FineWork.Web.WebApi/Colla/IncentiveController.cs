using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using System.Text.RegularExpressions;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Common; 
using AppBoot.Checks;
using AppBoot.Transactions;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Incentives")]
    [Authorize("Bearer")]
    public class IncentiveController : FwApiController
    {
        public IncentiveController(ISessionProvider<AefSession> sessionProvider,
            ITaskIncentiveManager taskIncentiveManager, 
            IIncentiveKindManager incentiveKindManager,
            IIncentiveManager incentiveManager,
            ITaskManager taskManager,
            IIMService imService,
            IConfiguration config
            ) : base(sessionProvider) 
        {
            if (taskIncentiveManager == null) throw new ArgumentNullException(nameof(taskIncentiveManager));
            if (incentiveKindManager == null) throw new ArgumentNullException(nameof(incentiveKindManager));
            if (incentiveManager == null) throw new ArgumentNullException(nameof(incentiveManager));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));

            m_TaskIncentiveManager = taskIncentiveManager;
            m_IncentiveKindManager = incentiveKindManager;
            m_IncentiveManager = incentiveManager;
            m_TaskManager = taskManager;
            m_IMService = imService;
            m_Config = config;
        }

        private readonly ITaskIncentiveManager m_TaskIncentiveManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;
        private readonly IIncentiveManager m_IncentiveManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;

        [HttpGet("FetchTaskIncentivesByTaskId")]
        public IActionResult FetchTaskIncentivesByTaskId(Guid taskId)
        {
            var taskIncentives = this.m_TaskIncentiveManager.FetchTaskIncentiveByTaskId(taskId);


            return taskIncentives != null? new ObjectResult(taskIncentives
                .Select(p => p.ToViewModel())
                .OrderBy(p=>p.IncentiveKind.Id))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpPost("UpdateTaskIncentive")]
        //[DataScoped(true)]
        public ActionResult UpdateTaskIncentive(Guid taskId, int incentiveKindId, decimal amount)
        {
            using (var tx = TxManager.Acquire())
            {
                var parseAmount = new int();
                if (!Int32.TryParse(amount.ToString(CultureInfo.InvariantCulture), out parseAmount))
                    return new BadRequestObjectResult("请输入正确的值！");

                var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                var incentiveKind =
                    IncentiveKindExistsResult.Check(m_IncentiveKindManager, incentiveKindId)
                        .ThrowIfFailed()
                        .IncentiveKind;
                var taskIncentive =
                    this.m_TaskIncentiveManager.UpdateTaskIncentive(taskId, incentiveKindId, amount).ToViewModel();

                //发送群消息
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Incentive"], partaker.Staff.Name,
                    incentiveKind.Name, amount);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                var result = new ObjectResult(taskIncentive);
                tx.Complete();
                return result;
            }
        }

        [HttpGet("FetchIncentiveKind")]
        public IEnumerable<IncentiveKindViewModel> FetchIncentiveKind()
        {
            return
                this.m_IncentiveKindManager.FetchIncentiveKind()
                    .Select(p => p.ToViewModel());
        }

        [HttpPost("CreateIncentive")]
        //[DataScoped(true)]
        public ActionResult CreateIncentive(Guid taskId, int incentiveKindId, Guid senderStaffId,
            Guid receiverStaffId, decimal quantity)
        {
            var parse = new decimal();
            if (!decimal.TryParse(quantity.ToString(CultureInfo.InvariantCulture), out parse))
               return new BadRequestObjectResult("请输入正确的值！");
            using (var tx = TxManager.Acquire())
            {
                this.m_IncentiveManager.CreateIncentive(taskId, incentiveKindId, senderStaffId, receiverStaffId,
                    quantity);
                tx.Complete();
            }
            return new HttpStatusCodeResult(201); 
        }

        [HttpGet("FetchIncentiveByTask")]
        public ActionResult FetchIncentiveByTask(Guid taskId)
        {
            var incentives = this.m_IncentiveManager.FetchIncentiveByTaskId(taskId).ToList();
            if (incentives.Any())
                return new ObjectResult(incentives.Select(p => p.ToViewModel()));
            return new HttpNotFoundObjectResult(taskId);
        }


    }
}

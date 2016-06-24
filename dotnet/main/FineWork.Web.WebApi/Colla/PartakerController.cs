using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Security;
using FineWork.Web.WebApi.Common;
using System.Data.Entity;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Common;
using FineWork.Net.IM;
using FineWork.Colla.Impls;
using Microsoft.Extensions.Configuration;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Partakers")]
    [Authorize("Bearer")]
    public class PartakerController : FwApiController
    {
        public PartakerController(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            ITaskAlarmManager taskAlarmManager,
            IStaffManager staffManager,
            IPartakerManager partakerManager,
            IPartakerReqManager partakerReqManager,
            IIMService imService,
            IConfiguration config)
            : base(sessionProvider)
        {
            if (partakerManager == null) throw new ArgumentNullException(nameof(partakerManager));
            if (partakerManager == null) throw new ArgumentNullException(nameof(partakerManager));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (taskAlarmManager == null) throw new ArgumentNullException(nameof(taskAlarmManager));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));
            if (imService == null) throw new ArgumentException(nameof(imService));

            m_PartakerManager = partakerManager;
            m_PartakerReqManager = partakerReqManager;
            m_TaskAlarmManager = taskAlarmManager;
            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_IMService = imService;
            m_Config = config;
            m_SessionProvider = sessionProvider;
        }

        private readonly IPartakerManager m_PartakerManager;
        private readonly IPartakerReqManager m_PartakerReqManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly ISessionProvider<AefSession> m_SessionProvider;

        [HttpGet("FetchPartakersWithReqsByStaff")]
        public IActionResult FetchPartakersWithReqsByStaff(Guid staffId)
        {

            using (var sp = m_SessionProvider.GetSession())
            { 
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var partakerReqSet = sp.DbContext.Set<PartakerReqEntity>()
                    .Where(p=>p.ReviewStatus== ReviewStatuses.Unspecified)
                    .Include(p=>p.Task.Creator)
                    .Include(p => p.Staff.Account).AsParallel().ToList(); 

                var partakers = m_PartakerManager.FetchPartakersByStaff(staffId).AsParallel().ToList();

                var partakersWithReqs = partakers
                    .OrderByDescending(p => p.Task.CreatedAt)
                    .GroupJoin(partakerReqSet, u => u.Task.Id, c => c.Task.Id, (u, c) => new
                    {
                        Task = u.Task.ToViewModel(true),
                        Partakers = u.Task.Partakers.ToList().Select(m => new
                        {
                            Id = m.Id,
                            Staff = m.Staff.ToViewModel(true),
                            Kind = m.Kind
                        }),
                        PartakerReqs = c?.Select(m => new
                        {
                            Id = m.Id,
                            Staff = m.Staff.ToViewModel(true),
                            PartakerKind = m.PartakerKind
                        })
                    }).ToList();
                if (!partakersWithReqs.Any())
                    return new HttpNotFoundObjectResult(staffId);


                return new ObjectResult(partakersWithReqs);
            }
        }


        [HttpGet("FetchPartakersByStaff")]
        public IActionResult FetchPartakersByStaff(Guid staffId)
        {
            var partakers = m_PartakerManager.FetchPartakersByStaff(staffId);
            return partakers == null
                ? new HttpNotFoundObjectResult(staffId)
                : new ObjectResult(partakers.Select(p => p.ToViewModel()).ToList());
        }

        /// <summary>
        /// 创建协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("CreateCollabrator")]
        //[DataScoped(true)]
        public PartakerViewModel CreateCollabrator(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                var collabrator = this.m_PartakerManager.CreateCollabrator(taskId, staffId);
                var result= collabrator.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        /// <summary>
        /// 创建指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("CreateMentor")]
        //[DataScoped(true)]
        public PartakerViewModel CreateMentor(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                var collabrator = this.m_PartakerManager.CreateMentor(taskId, staffId);
                var result = collabrator.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        /// <summary>
        /// 创建接受者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public PartakerViewModel CreateRecipient(Guid taskId, Guid staffId)
        {
            var recipient = this.m_PartakerManager.CreateRecipient(taskId, staffId);
            return recipient.ToViewModel();
        }

        /// <summary>
        /// 移除协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("RemoveCollabrator")]
        //[DataScoped(true)]
        public IActionResult RemoveCollabrator(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                if (staff.Account.Id == this.AccountId)
                    throw new FineWorkException("用户无法删除自己的角色");

                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).Partaker;

                if (partaker == null || partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您无权进行此操作.");

                this.m_PartakerManager.RemoveCollabrator(taskId, staffId);

                //发送群消息
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Remove"], partaker.Staff.Name, staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                tx.Complete();
                return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// 移除指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("RemoveMentor")]
        //[DataScoped(true)]
        public IActionResult RemoveMentor(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                if (staff.Account.Id == this.AccountId)
                    throw new FineWorkException("用户无法删除自己的角色");

                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).Partaker;

                if (partaker == null || partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您无权进行此操作.");


                this.m_PartakerManager.RemoveMentor(taskId, staffId);

                //发送群消息
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Remove"], partaker.Staff.Name, staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                tx.Complete();
                return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
            }
        }


        /// <summary>
        /// 移除接受者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("RemoveRecipient")]
        //[DataScoped(true)]
        public IActionResult RemoveRecipient(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                if (staff.Account.Id == this.AccountId)
                    throw new FineWorkException("用户无法删除自己的角色");

                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;


                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).Partaker;

                if (partaker == null || partaker.Kind != PartakerKinds.Leader)
                    throw new FineWorkException("您无权进行此操作.");


                this.m_PartakerManager.RemoveRecipient(taskId, staffId);

                //发送群消息
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Remove"], partaker.Staff.Name, staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                tx.Complete();
                return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// 移交任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("ChangeLeader")]
        //[DataScoped(true)]
        public PartakerViewModel ChangeLeader(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                if (partaker.Kind == PartakerKinds.Collaborator)
                    throw new FineWorkException("任务协同者不可用进行此操作。");

                //判断是否有预警或共识为处理 
                AlarmOrVoteExistsResult.Check(partaker, taskId, m_TaskAlarmManager).ThrowIfFailed();


                var leader = this.m_PartakerManager.ChangeLeader(taskId, staffId);

                var message = string.Format(m_Config["LeanCloud:Messages:Task:Transfer"], partaker.Staff.Name,
                    staff.Name);

                m_IMService.ChangeTaskLeader(partaker.Staff.Id.ToString(), task.ConversationId, staffId.ToString())
                    .Wait();
                m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId,task.ConversationId, task.Name, message);
                var result = leader.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        /// <summary>
        /// 退出任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpPost("ExitTask")]
        public IActionResult ExitTask(Guid taskId, Guid staffId)
        {

            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                //发送群消息
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Exit"], partaker.Staff.Name);
                this.m_PartakerManager.ExitTask(taskId, staffId);
                tx.Complete(); 
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                return new HttpStatusCodeResult(204);
            }
        }

        /// <summary>
        ///  获取战友信息
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [HttpGet("FetchPartakersByOrg")]
        public IActionResult FetchPartakersByOrg(Guid orgId, Guid staffId)
        {
            //获取组织下所有与我相关的任务
            var tasks = m_TaskManager.FetchTasksByOrgId(orgId).ToList();
            var partakers = new List<PartakerEntity>();
            if (tasks.Any())
            {
                partakers = tasks.Where(p => p.Partakers.Any(s => s.Staff.Id == staffId))
                    .SelectMany(p => p.Partakers).ToList();
            }
            var result = partakers.Any()
                ? new ObjectResult(partakers.Select(p => p.ToViewModel()))
                : new HttpNotFoundObjectResult($"orgid:{orgId},staffId:{staffId}");
            return result;
        }

        [HttpPost("ChangePartakerKind")]
        public PartakerViewModel ChangePartakerKind(Guid taskId, Guid staffId, PartakerKinds partakerKind)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            if (staff.Account.Id == this.AccountId)
                throw new FineWorkException("用户无法更新自己的角色");

            var partaker = AccountIsPartakerResult.Check(task,this.AccountId).ThrowIfFailed().Partaker;

            var pendingPartaker = PartakerExistsResult.CheckForStaff(task, staffId).ThrowIfFailed().Partaker;
            //只有负责人才可以修改角色
            if (partaker.Kind != PartakerKinds.Leader)
                throw new FineWorkException("您无权进行此操作.");


            //判断是否有预警或共识为处理 
            AlarmOrVoteExistsResult.Check(pendingPartaker, taskId, m_TaskAlarmManager).ThrowIfFailed();

            var result = this.m_PartakerManager.ChangePartakerKind(task, staff, partakerKind);

            //发送群消息
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangePartakerKind"], partaker.Staff.Name,
                staff.Name, partakerKind.GetLabel());
            m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);

            return result.ToViewModel();
        }
    }
}
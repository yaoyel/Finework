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
using FineWork.Message;
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
            IConfiguration config,
            INotificationManager notificationManager)
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
            m_NotificationManager = notificationManager;
        }

        private readonly IPartakerManager m_PartakerManager;
        private readonly IPartakerReqManager m_PartakerReqManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly ISessionProvider<AefSession> m_SessionProvider;
        private readonly INotificationManager m_NotificationManager;

        [HttpGet("FetchPartakersWithReqsByStaff")]
        public IActionResult FetchPartakersWithReqsByStaff(Guid staffId)
        {

            using (var sp = m_SessionProvider.GetSession())
            { 
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var partakerReqSet = sp.DbContext.Set<PartakerReqEntity>()
                    .Where(p=>p.ReviewStatus== ReviewStatuses.Unspecified)
                    .Include(p=>p.Task.Creator)
                    .Include(p => p.Staff.Account)
                    .Include(p=>p.Staff.Org).AsParallel().ToList(); 

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
        /// <param name="staffIds"></param>
        /// <returns></returns>
        [HttpPost("RemoveCollabrator")]
        //[DataScoped(true)]
        public IActionResult RemoveCollabrator(Guid taskId, Guid[] staffIds)
        {
            if (!staffIds.Any()) throw new FineWorkException("请选择要删除的成员.");

            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).Partaker;

            if (partaker == null || partaker.Kind != PartakerKinds.Leader)
                throw new FineWorkException("您无权进行此操作.");

            var staffNames = new List<string>();
            var phoneNumbers = new List<string>();

            using (var tx = TxManager.Acquire())
            {
                foreach (var staffId in staffIds)
                {
                    var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                    if (staff.Account.Id == this.AccountId)
                        throw new FineWorkException("用户无法删除自己的角色");

                    staffNames.Add(staff.Name);
                    phoneNumbers.Add(staff.Account.PhoneNumber);
                    this.m_PartakerManager.RemoveCollabrator(taskId, staffId);
                }
                tx.Complete();
            }
            //发送群消息
            var imMessage = string.Format(m_Config["LeanCloud:Messages:Task:Remove"], partaker.Staff.Name,
                string.Join(",", staffNames));
            m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                imMessage);

          
            //推送消息给申请人员
            var message = string.Format(m_Config["PushMessage:Task:remove"], task.Name);

            m_NotificationManager.SendByAliasAsync(null, message, null, phoneNumbers.ToArray());
            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);

        }

        /// <summary>
        /// 移除指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffIds"></param>
        /// <returns></returns>
        [HttpPost("RemoveMentor")]
        //[DataScoped(true)]
        public IActionResult RemoveMentor(Guid taskId, Guid[] staffIds)
        {
            if (!staffIds.Any()) throw new FineWorkException("请选择要删除的成员.");

            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).Partaker;

            if (partaker == null || partaker.Kind != PartakerKinds.Leader)
                throw new FineWorkException("您无权进行此操作.");

            var staffNames = new List<string>();
            var phoneNumbers = new List<string>();

            using (var tx = TxManager.Acquire())
            {
                foreach (var staffId in staffIds)
                {
                    var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                    if (staff.Account.Id == this.AccountId)
                        throw new FineWorkException("用户无法删除自己的角色");
                    staffNames.Add(staff.Name);
                    phoneNumbers.Add(staff.Account.PhoneNumber);
                    this.m_PartakerManager.RemoveMentor(taskId, staffId);
                }
                tx.Complete();
            }
            //发送群消息
            var imMessage = string.Format(m_Config["LeanCloud:Messages:Task:Remove"], partaker.Staff.Name,
                string.Join(",", staffNames));
            m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                imMessage);
            //推送消息给申请人员
            var pushMessage = string.Format(m_Config["PushMessage:Task:remove"], task.Name);

            m_NotificationManager.SendByAliasAsync(null, pushMessage, null, phoneNumbers.ToArray());

            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);

        }

        /// <summary>
        /// 移除接受者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffIds"></param>
        /// <returns></returns>
        [HttpPost("RemoveRecipient")]
        //[DataScoped(true)]
        public IActionResult RemoveRecipient(Guid taskId, Guid[] staffIds)
        {
            if (!staffIds.Any()) throw new FineWorkException("请选择要删除的成员.");

            var staffNames = new List<string>();
            var phoneNumbers = new List<string>();
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).Partaker;

            if (partaker == null || partaker.Kind != PartakerKinds.Leader)
                throw new FineWorkException("您无权进行此操作.");

            using (var tx = TxManager.Acquire())
            {
                foreach (var staffId in staffIds)
                {
                    var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                    if (staff.Account.Id == this.AccountId)
                        throw new FineWorkException("用户无法删除自己的角色");

                    staffNames.Add(staff.Name);
                    phoneNumbers.Add(staff.Account.PhoneNumber);
                    this.m_PartakerManager.RemoveRecipient(taskId, staffId);
                }

                tx.Complete();
            }
            //发送群消息
            var message = string.Format(m_Config["LeanCloud:Messages:Task:Remove"], partaker.Staff.Name,
                string.Join(",", staffNames));
            m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                message);
            //推送消息给申请人员
            var pushMessage = string.Format(m_Config["PushMessage:Task:remove"], task.Name);

            m_NotificationManager.SendByAliasAsync(null, pushMessage, null, phoneNumbers.ToArray());
            return new HttpStatusCodeResult((int) HttpStatusCode.NoContent);
        }

        /// <summary>
        /// 移交任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="staffId"></param>
        /// <param name="newKind"></param>
        /// <returns></returns>
        [HttpPost("ChangeLeader")]
        //[DataScoped(true)]
        public PartakerViewModel ChangeLeader(Guid taskId, Guid staffId,PartakerKinds newKind)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                if (partaker.Kind == PartakerKinds.Collaborator)
                    throw new FineWorkException("任务协同者不可用进行此操作。");

                //判断是否有预警或共识为处理 
                //AlarmOrVoteExistsResult.Check(partaker, taskId, m_TaskAlarmManager).ThrowIfFailed();


                var leader = this.m_PartakerManager.ChangeLeader(taskId, staffId,newKind);

                var message = string.Format(m_Config["LeanCloud:Messages:Task:Transfer"], partaker.Staff.Name,
                    staff.Name);

                m_IMService.ChangeTaskLeader(partaker.Staff.Id.ToString(), task.Conversation.Id, staffId.ToString())
                    .Wait();
                m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId,task.Conversation.Id, task.Name, message);
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
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.Conversation.Id, task.Name, message);
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
            //AlarmOrVoteExistsResult.Check(pendingPartaker, taskId, m_TaskAlarmManager).ThrowIfFailed();

            var result = this.m_PartakerManager.ChangePartakerKind(task, staff, partakerKind);

            //发送群消息
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangePartakerKind"], partaker.Staff.Name,
                staff.Name, partakerKind.GetLabel());
            m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.Conversation.Id, task.Name, message);

            return result.ToViewModel();
        }

        [AllowAnonymous]
        [HttpGet("FecthConversationsByStaffId")]
        public IActionResult FecthConversationsByStaffId(Guid staffId)
        { 
            var task = m_PartakerManager.FetchPartakersByStaff(staffId).Select(p => p.Task).Where(p=>p.IsDeserted==null).ToList();

            var convrs = m_IMService.FecthConversationsByStaffIdAsync(staffId).Result.ToList();

            var alarms =
                m_TaskAlarmManager.FetchTaskAlarmsByStaffIdWithTaskId(staffId,null).ToList();

            var taskResult = convrs.Join(task, u => u.ConversationId, c => c.ConversationId, (u, c) =>
              {
                  if (u.Attributes.ContainsKey("IsDeserted"))
                      u.Attributes["IsDeserted"] = c.IsDeserted;
                  if (u.Attributes.ContainsKey("Progress"))
                      u.Attributes["IsDeserted"] = c.Progress;
                  if (u.Attributes.ContainsKey("IsEnd"))
                      u.Attributes["IsDeserted"] = c.Report != null;
                  if (u.Attributes.ContainsKey("IsDeserted"))
                      u.Attributes["IsDeserted"] = c.IsDeserted;
                  if (u.Attributes.ContainsKey("ResolvedCount"))
                      u.Attributes["ResolvedCount"] = alarms.Count(p => p.ResolveStatus == ResolveStatus.Closed && p.Conversation.Id == u.ConversationId);
                  if (u.Attributes.ContainsKey("AlarmsCount"))
                      u.Attributes["AlarmsCount"] = alarms.Count(p => p.Conversation.Id == u.ConversationId);

                  return u;
              }).ToList();

            var alarmResult = convrs.Join(alarms, u => u.ConversationId, c => c.Conversation.Id, (u, c) =>
            {
                if (u.Attributes.ContainsKey("ResolvedCount"))
                    u.Attributes["ResolvedCount"] = alarms.Count(p => p.ResolveStatus == ResolveStatus.Closed && p.Conversation.Id == u.ConversationId);

                if (u.Attributes.ContainsKey("AlarmsCount"))
                    u.Attributes["AlarmsCount"] = alarms.Count(p => p.Conversation.Id == u.ConversationId);

                return u;
            }).ToList();


            if (convrs.Any()) return new ObjectResult(taskResult.Union(alarmResult));
            return new HttpNotFoundObjectResult(staffId);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla.Models;
using FineWork.Message;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/TaskAlarms")]
    [Authorize("Bearer")]
    public class TaskAlarmController : FwApiController
    {
        public TaskAlarmController(ISessionProvider<AefSession> sessionProvider,
            IAlarmManager alarmPeriodManager,
            ITaskAlarmManager taskAlarmManager,
            IStaffManager staffManager,
            ITaskManager taskManager,
            IIMService imService,
            IConfiguration config,
            INotificationManager notificationManager)
            : base(sessionProvider)
        {
            if (alarmPeriodManager == null) throw new ArgumentNullException(nameof(alarmPeriodManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));
            if (taskManager == null) throw new ArgumentNullException(nameof(taskManager));
            if (imService == null) throw new ArgumentNullException(nameof(imService));
            m_AlarmPeriodManager = alarmPeriodManager;
            m_TaskAlarmManager = taskAlarmManager;
            m_StaffManager = staffManager;
            m_TaskManager = taskManager;
            m_IMService = imService;
            m_Config = config;
            m_NotificationManager = notificationManager;
        }

        private readonly IAlarmManager m_AlarmPeriodManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly IStaffManager m_StaffManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly INotificationManager m_NotificationManager;

        [HttpPost("CreateAlarmPeriod")]
        //[DataScoped(true)]
        public AlarmViewModel CreateAlarmPeriod([FromBody]CreateAlarmPeriodModel createAlarmPeriodModel)
        {
            if (string.IsNullOrEmpty(createAlarmPeriodModel.ShortTime)) throw new ArgumentException(nameof(createAlarmPeriodModel.ShortTime));

            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, createAlarmPeriodModel.TaskId.Value).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                var alarmPeriod = this.m_AlarmPeriodManager.CreateAlarmPeriod(createAlarmPeriodModel);

                //发送群消息  
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Alarm:Create"], partaker.Staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                var result = alarmPeriod.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpPost("DeleteAlarmPeriod")]
        //[DataScoped(true)]
        public void DeleteAlarmPeriod(Guid alarmPeriodId)
        {
            using (var tx = TxManager.Acquire())
            {
                var alarmPeriod = this.m_AlarmPeriodManager.FindAlarmPeriod(alarmPeriodId);
                var partaker = AccountIsPartakerResult.Check(alarmPeriod.Task, this.AccountId).ThrowIfFailed().Partaker;
                var task = alarmPeriod.Task;

                //发送群消息  
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Alarm:Delete"], partaker.Staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                this.m_AlarmPeriodManager.DeleteAlarmPeriod(alarmPeriod);
                tx.Complete(); 
            }
        }

        [HttpPost("UpdateAlarmPeriod")]
        //[DataScoped(true)]
        public AlarmViewModel UpdateAlarmPeriod([FromBody]UpdateAlarmPeriodModel updateAlarmPeriodModel)
        {

            if (string.IsNullOrEmpty(updateAlarmPeriodModel.ShortTime)) throw new ArgumentException(nameof(updateAlarmPeriodModel.ShortTime));
            if (updateAlarmPeriodModel.Weekdays <= 0) throw new ArgumentException(nameof(updateAlarmPeriodModel.Weekdays));
            using (var tx = TxManager.Acquire())
            {
                var alarmPeriod = this.m_AlarmPeriodManager.UpdateAlarmPeriodTime(updateAlarmPeriodModel);

                var partaker = AccountIsPartakerResult.Check(alarmPeriod.Task, this.AccountId).ThrowIfFailed().Partaker;

                var task = alarmPeriod.Task;
                //发送群消息  
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Alarm:Update"], partaker.Staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                var result = alarmPeriod.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpPost("UpdateAlarmPeriodEnabled")]
        //[DataScoped(true)]
        public AlarmViewModel UpdateAlarmPeriodEnabled(Guid alarmPeriodId, bool isEnabled)
        {
            using (var tx = TxManager.Acquire())
            {
                var alarmPeriod = this.m_AlarmPeriodManager.UpdateAlarmPeriodEnabled(alarmPeriodId, isEnabled);

                var partaker = AccountIsPartakerResult.Check(alarmPeriod.Task, this.AccountId).ThrowIfFailed().Partaker;
                //发送群消息
                var alarmStatus = isEnabled ? "开启" : "关闭";
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Alarm:Enable"], partaker.Staff.Name,
                    alarmStatus);

                var task = alarmPeriod.Task;
                m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message);
                var result = alarmPeriod.ToViewModel();
                tx.Complete();
                return result;
            }
        }


        [HttpGet("FetchAlarmPeriodByTaskId")]
        public IActionResult FetchAlarmPeriodByTaskId(Guid taskId)
        {
            var alarmPeriods = this.m_AlarmPeriodManager.FetchAlarmPeriodsByTaskId(taskId);

            return alarmPeriods != null
                ? new ObjectResult(alarmPeriods.Select(p => p.ToViewModel()))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchUntreatedAlarmPeriodByStaff")]
        public IActionResult FetchUntreatedAlarmPeriodByStaff(Guid staffId)
        {
            var result = this.m_AlarmPeriodManager.FetchUntreatedAlarmPeriodByStaff(staffId).ToList();
            if (result.Any()) return new ObjectResult(result.Select(p => p.ToViewModel()));
            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FindAlarmPeriod")]
        public IActionResult FindAlarmPeriod(Guid alarmPeriodId)
        {
            var alarmPeriod = this.m_AlarmPeriodManager.FindAlarmPeriod(alarmPeriodId);

            return alarmPeriod != null
                ? new ObjectResult(alarmPeriod.ToViewModel())
                : new HttpNotFoundObjectResult(alarmPeriodId);
        }

        [HttpPost("CreateTaskAlarm")]
        //[DataScoped(true)]
        public TaskAlarmViewModel CreateTaskAlarm([FromBody]CreateTaskAlarmModel taskAlarmModel )
        {
            Args.NotNull(taskAlarmModel, nameof(taskAlarmModel));

            using (var tx = TxManager.Acquire())
            {
                var alarm = m_TaskAlarmManager.CreateTaskAlarm(taskAlarmModel);
                var result = alarm.ToViewModel();
                tx.Complete();
                return result; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskAlarmId"></param>
        /// <param name="newStatus"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost("ChangeResolvedStatus")]
        //[DataScoped(true)]
        public TaskAlarmViewModel ChangeResolvedStatus(Guid taskAlarmId, ResolveStatus newStatus, string comment)
        {
            if(newStatus==ResolveStatus.Resolved)
            Args.MaxLength(comment, 400, nameof(comment), "会议总结");

            using (var tx = TxManager.Acquire())
            {
                var alarm = m_TaskAlarmManager.ChangeResolvedStatus(taskAlarmId, this.AccountId, newStatus, comment);
             
                var result = alarm.ToViewModel(); 
                tx.Complete();

                return result;
            }
        }

        [HttpGet("FetchAlarmsByResolvedStatus")]
        public IActionResult FetchAlarmsByResolvedStatus(Guid taskId, Guid staffId, bool isResolved)
        {
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task; 
            var partaker = PartakerExistsResult.CheckForStaff(task, staffId).ThrowIfFailed().Partaker;

            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByResolvedStatus(taskId, staffId, isResolved).ToList();

            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId);
            //自己发出的预警
            var sends = alarms.Where(p => p.Staff.Id == staffId).ToList();
            var receiveds = alarms.Where(p => p.Staff.Id!=staffId && p.ReceiversArray!=null && p.ReceiversArray.Contains((int) partaker.Kind)).ToList();
            var others = alarms.Except(sends).Except(receiveds).ToList();
            var result = new StaffAlarmsViewModel
            {
                SendOuts=sends?.Select(p=> p.ToViewModel()).ToList(),
                Receiveds=receiveds?.Select(p => p.ToViewModel()).ToList(),
                Others =others?.Select(p => p.ToViewModel()).ToList()
            }; 

            if (isResolved)
                return new ObjectResult(result); 

            return new ObjectResult(ChangeCommunicationStatus(taskId,staffId,result)); 
        } 

        [HttpGet("FetchAlarmsByStaff")]
        public IActionResult FetchAlarmsByStaff(Guid staffId)
        {
            var alarms =
                m_TaskAlarmManager.FetchTaskAlarmsByStaffId(staffId)
                    .Where(p => p.ResolveStatus == ResolveStatus.UnResolved)
                    .ToList();
            if (alarms.Any())
                return new ObjectResult(alarms.Select(p => p.ToViewModel()));

            return new HttpNotFoundObjectResult(staffId); 
        }

        [HttpGet("FetchAlarmsByTaskGroupByKind")]
        [AllowAnonymous]
        public IActionResult FetchAlarmsByTaskGroupByKind(Guid taskId)
        {
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByTaskId(taskId).ToList();
            if (alarms.Any()) return new ObjectResult(alarms.ToViewModelGroupByKind());
            return new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchAlarmsByStaffGroupByTask")]
        public IActionResult FetchAlarmsByStaffGroupByTask(Guid staffId)
        {
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByStaffId(staffId,includeAllTask:true)
                .Where(p=>p.ResolveStatus!=ResolveStatus.Closed).ToList();

            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId);

            return new ObjectResult(alarms.ToViewModelGroupByTask());
        }


        [HttpGet("FetchAlarmsByStaffGroupByKind")]
        public IActionResult FetchAlarmsByStaffGroupByKind(Guid staffId)
        {
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByStaffId(staffId, includeAllTask: true)
                .Where(p => p.ResolveStatus != ResolveStatus.Closed).ToList();

            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId);

            return new ObjectResult(alarms.ToViewModelGroupByKind());
        }


        [HttpGet("FetchAlarmsWithPartakerByStaffAndKind")]
        public IActionResult FetchAlarmsWithPartakerByStaffAndKind(Guid staffId, TaskAlarmKinds? alarmKind)
        {
            var alarms = m_TaskAlarmManager.FetchAlarmsWithPartakerByStaffAndKind(staffId, alarmKind)
                .Where(p => p.ResolveStatus != ResolveStatus.Closed).ToList();

            if (!alarms.Any()) return new HttpNotFoundObjectResult($"{staffId},{alarmKind}");

            return new ObjectResult(alarms.ToViewModelWithPartakers());
        }

        [HttpPost("UpdateTaskAlarmContent")]
        public IActionResult UpdateTaskAlarmContent(Guid taskAlarmId, string content)
        {
            var taskAlarm = TaskAlarmExistsResult.Check(this.m_TaskAlarmManager, taskAlarmId).ThrowIfFailed().TaskAlarm;
            taskAlarm.Content = content;
            this.m_TaskAlarmManager.UpdateTaskAlarm(taskAlarm);
            return new HttpStatusCodeResult(200);
        }

        /// <summary>
        /// 根据聊天室类型返回未处理的预警
        /// </summary> 
        /// <param name="taskId"></param>
        /// <param name="chatRoomKind"></param>
        /// <param name="creatorStaffId"></param>
        /// <returns></returns>
        [HttpGet("FetchAlarmsByChatRoomKind")] 
        public IActionResult FetchAlarmsByChatRoomKind( Guid taskId, ChatRoomKinds chatRoomKind,Guid creatorStaffId)
        {
            var alarm = this.m_TaskAlarmManager.FetchAlarmsByChatRoomKind(taskId,chatRoomKind,creatorStaffId).ToList();
            if (!alarm.Any()) return new HttpNotFoundObjectResult(chatRoomKind);

            return new ObjectResult(alarm.Select(p=>p.ToViewModel()).ToList());
        }

        [HttpGet("FetchAlarmsByConversationId")]
        public IActionResult FetchAlarmsByConversationId(string conversationId)
        {
            var alarm = this.m_TaskAlarmManager.FetchAlarmsByConversationId(conversationId).ToList();
            if (!alarm.Any()) return new HttpNotFoundObjectResult(conversationId);

            return new ObjectResult(alarm.Select(p => p.ToViewModel()).ToList());
        }

        private StaffAlarmsViewModel ChangeCommunicationStatus(Guid taskId,Guid staffId,StaffAlarmsViewModel staffAlarms)
        {

            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

            var mentors = task.Partakers.Where(p => p.Kind == PartakerKinds.Mentor).ToList()
                .Select(p => p.Staff.Id).ToArray();

            var otherPartakers = task.Partakers.Where(p => p.Kind != PartakerKinds.Mentor).ToList()
                .Select(p => p.Staff.Id).ToArray();

            var alarmsInCommunication = new List<TaskAlarmEntity>();
            var allAlarms = this.m_TaskAlarmManager.FetchTaskAlarmsByTaskId(taskId)
                .ToList();

            //获取当前tong会议室正在处理的预警7
            var alarmInTong = allAlarms.Where(p => p.ResolveStatus == ResolveStatus.UnResolved && p.Receivers.Split(',').Count() == 4)
                .OrderBy(p=>p.CreatedAt).ToList();

            //获取shine会议室正在处理的预警
            var alarmsInShine = allAlarms.Where(p => p.ResolveStatus == ResolveStatus.UnResolved && p.Receivers.Split(',').Count() < 4)
                .OrderBy(p => p.CreatedAt).ToList();


            alarmsInShine = alarmsInShine.GroupBy(p => p.ConversationId)
                .Select(p => p.FirstOrDefault()).ToList();

            if (alarmInTong.Any())
                alarmsInCommunication.Add(alarmInTong.FirstOrDefault());

            if (alarmsInShine.Any())
                alarmsInCommunication.AddRange(alarmsInShine);

            var alarmIds = alarmsInCommunication.Select(p => p.Id).ToArray();

            if (!alarmsInCommunication.Any())
                return staffAlarms;

            if (staffAlarms.Receiveds.Any())
            {
                for (int i = 0; i < staffAlarms.Receiveds.Count(); i++)
                {
                    if (alarmIds.Contains(staffAlarms.Receiveds.ElementAt(i).Id))
                        staffAlarms.Receiveds[i].IsCommunicating = true;
                }
            }

            if (staffAlarms.SendOuts.Any())
            {

                for (int i = 0; i < staffAlarms.SendOuts.Count(); i++)
                {
                    if (alarmIds.Contains(staffAlarms.SendOuts.ElementAt(i).Id))
                        staffAlarms.SendOuts[i].IsCommunicating = true;
                }
            }

            if (staffAlarms.Others.Any())
            {
                for (int i = 0; i < staffAlarms.Others.Count(); i++)
                {
                    if (alarmIds.Contains(staffAlarms.Others.ElementAt(i).Id))
                        staffAlarms.Others[i].IsCommunicating = true;
                }
            }

            return staffAlarms;
        }

        [HttpGet("UpdateRemoteServer")]
        [AllowAnonymous]
        public void UpdateRemoteServer()
        {
            m_TaskAlarmManager.UpdateRemoteServer();
        }
    }
}

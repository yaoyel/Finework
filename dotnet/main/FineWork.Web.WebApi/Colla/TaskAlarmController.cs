using System;
using System.Collections.Generic;
using System.IO;
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
using FineWork.Colla.Impls;
using FineWork.Colla.Models;
using FineWork.Message;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;
using FineWork.Common;
using FineWork.Files;
using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;

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
            INotificationManager notificationManager,
            IConversationManager conversationManager,
            IMemberManager memberManager,
            IPartakerManager partakerManager,
            IFileManager fileManager,
            IAlarmTempManager alarmTempManager)
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
            m_ConversationManager = conversationManager;
            m_SessionProvider = sessionProvider;
            m_MemberManager = memberManager;
            m_PartakerManager = partakerManager;
            m_FileManager = fileManager;
            m_AlarmTempManager = alarmTempManager;
        }

        private readonly IAlarmManager m_AlarmPeriodManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly IStaffManager m_StaffManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly INotificationManager m_NotificationManager;
        private readonly IConversationManager m_ConversationManager;
        private readonly ISessionProvider<AefSession> m_SessionProvider;
        private readonly IMemberManager m_MemberManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IFileManager m_FileManager;
        private readonly IAlarmTempManager m_AlarmTempManager;

        [HttpPost("UploadAlarmAtt")]
        public void UploadAlarmAtt(Guid alarmId, IFormFile file, bool isRollBak = false)
        {
            var alarm = AlarmExistsResult.Check(this.m_AlarmPeriodManager, alarmId).ThrowIfFailed().AlarmPeriod;

            try
            {
                if (file == null || file.Length == 0)
                    throw new FineWorkException("上传文件不能为空.");
                UploadTaskAlarmAtt(alarm, file);
            }
            catch (Exception)
            {

                if (isRollBak)
                    this.DeleteAlarmPeriod(alarmId);
                throw;
            }

        }

        [HttpPost("CreateAlarmPeriod")]
        //[DataScoped(true)]
        public AlarmViewModel CreateAlarmPeriod([FromBody] CreateAlarmPeriodModel createAlarmPeriodModel)
        {
            if (string.IsNullOrEmpty(createAlarmPeriodModel.ShortTime))
                throw new ArgumentException(nameof(createAlarmPeriodModel.ShortTime));
            AlarmViewModel result;
            AlarmEntity alarmPeriod;

            using (var tx = TxManager.Acquire())
            {
                var task =
                    TaskExistsResult.Check(m_TaskManager, createAlarmPeriodModel.TaskId.Value).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                alarmPeriod = this.m_AlarmPeriodManager.CreateAlarmPeriod(createAlarmPeriodModel);

                //发送群消息  
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Alarm:Create"], partaker.Staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                    message);
                result = alarmPeriod.ToViewModel();
                tx.Complete();
            }

            return result;
        }

        [HttpPost("CreateAlarmTemp")]
        public AlarmViewModel CreateAlarmTemp([FromBody] CreateAlarmPeriodModel model)
        {
            if (string.IsNullOrEmpty(model.ShortTime))
                throw new ArgumentException(nameof(model.ShortTime));
            AlarmViewModel result;

            using (var tx = TxManager.Acquire())
            {
                var alarmPeriod = this.m_AlarmTempManager.CreateAlarmTemp(model);
                result = alarmPeriod.ToViewModel();
                tx.Complete();
            }

            return result;
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
                m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                    message);
                this.m_AlarmPeriodManager.DeleteAlarmPeriod(alarmPeriod);
                tx.Complete();
            }
        }

        public void DeleteAlarmTemp(Guid alarmTempId)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_AlarmTempManager.DeleteAlarmTemps(alarmTempId);
                tx.Complete();
            }
        }

        [HttpPost("UpdateAlarmPeriod")]
        //[DataScoped(true)]
        public AlarmViewModel UpdateAlarmPeriod([FromBody] UpdateAlarmPeriodModel updateAlarmPeriodModel)
        {
            if (string.IsNullOrEmpty(updateAlarmPeriodModel.ShortTime))
                throw new ArgumentException(nameof(updateAlarmPeriodModel.ShortTime));
            if (updateAlarmPeriodModel.Weekdays <= 0 && string.IsNullOrEmpty(updateAlarmPeriodModel.DaysInMonth) &&
                updateAlarmPeriodModel.IsRepeat)
                throw new FineWorkException("请设置预警时间.");
            if (!updateAlarmPeriodModel.IsRepeat && updateAlarmPeriodModel.NoRepeatTime == default(DateTime))
                throw new FineWorkException("请设置预警时间.");

            using (var tx = TxManager.Acquire())
            {
                var alarmPeriod = this.m_AlarmPeriodManager.UpdateAlarmPeriodTime(updateAlarmPeriodModel);

                var partaker = AccountIsPartakerResult.Check(alarmPeriod.Task, this.AccountId).ThrowIfFailed().Partaker;

                var task = alarmPeriod.Task;
                //发送群消息  
                var message = string.Format(m_Config["LeanCloud:Messages:Task:Alarm:Update"], partaker.Staff.Name);
                m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                    message);
                var result = alarmPeriod.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpPost("UpdateAlarmTemp")]
        public IActionResult UpdateAlarmTemp([FromBody] UpdateAlarmPeriodModel model)
        {
            if (string.IsNullOrEmpty(model.ShortTime))
                throw new ArgumentException(nameof(model.ShortTime));
            if (model.Weekdays <= 0 && string.IsNullOrEmpty(model.DaysInMonth) && model.IsRepeat)
                throw new FineWorkException("请设置预警时间.");
            if (!model.IsRepeat && model.NoRepeatTime == default(DateTime))
                throw new FineWorkException("请设置预警时间.");
            using (var tx = TxManager.Acquire())
            {
                this.m_AlarmTempManager.UpdateAlarmTemp(model);
                tx.Complete();
            }

            return new HttpOkResult();
        }

        [HttpPost("UpdateAlarmTempEnabled")]
        //[DataScoped(true)]
        public IActionResult UpdateAlarmTempEnabled(Guid alarmTempId, bool isEnabled)
        {
            using (var tx = TxManager.Acquire())
            {
                this.m_AlarmTempManager.UpdateAlarmTempEnabled(alarmTempId, isEnabled);

                tx.Complete();
            }

            return new HttpOkResult();
        }

        [HttpPost("UpdateAlarmPeriodEnabled")]
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
                m_IMService.SendTextMessageByConversationAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name,
                    message);
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
        public TaskAlarmViewModel CreateTaskAlarm([FromBody] CreateTaskAlarmModel taskAlarmModel)
        {
            Args.NotNull(taskAlarmModel, nameof(taskAlarmModel));

            //老版本 0.9.0
            if (taskAlarmModel.Receivers != null && taskAlarmModel.Receivers.Any() &&
                taskAlarmModel.PartakerKinds == null)
                taskAlarmModel.PartakerKinds = taskAlarmModel.Receivers;

            if (taskAlarmModel.CreatorId == default(Guid) && taskAlarmModel.StaffId != default(Guid))
                taskAlarmModel.CreatorId = taskAlarmModel.StaffId;

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
            if (newStatus == ResolveStatus.Resolved)
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
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByResolvedStatus(taskId, staffId, isResolved).ToList();

            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId);
            //自己发出的预警
            var sends = alarms.Where(p => p.Staff.Id == staffId).ToList();
            var receiveds =
                alarms.Where(
                    p =>
                        p.Staff.Id != staffId && p.Conversation != null &&
                        p.Conversation.Members.Any(a => a.Staff.Id == staffId)).ToList();
            var others = alarms.Except(sends).Except(receiveds).ToList();
            var result = new StaffAlarmsViewModel
            {
                SendOuts = sends?.Select(p => p.ToViewModel(false, false)).ToList(),
                Receiveds = receiveds?.Select(p => p.ToViewModel(false, false)).ToList(),
                Others = others?.Select(p => p.ToViewModel(false, false)).ToList()
            };

            if (isResolved)
                return new ObjectResult(result);

            return new ObjectResult(ChangeCommunicationStatus(taskId, result));
        }

        [HttpGet("FetchAlarmsByStaff")]
        public IActionResult FetchAlarmsByStaff(Guid staffId)
        {
            var alarms =
                m_TaskAlarmManager.FetchTaskAlarmsByCreatorId(staffId)
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
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByCreatorId(staffId, includeAllTask: true)
                .Where(p => p.ResolveStatus != ResolveStatus.Closed).ToList();

            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId);

            return new ObjectResult(alarms.ToViewModelGroupByTask());
        }

        [HttpGet("FetchAlarmsByStaffGroupByKind")]
        public IActionResult FetchAlarmsByStaffGroupByKind(Guid staffId)
        {
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByCreatorId(staffId, includeAllTask: true)
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

        [HttpGet("FetchAlarmsByConversationId")]
        public IActionResult FetchAlarmsByConversationId(string conversationId)
        {
            var alarm = this.m_TaskAlarmManager.FetchAlarmsByConversationId(conversationId).ToList();
            if (!alarm.Any()) return new HttpNotFoundObjectResult(conversationId);

            return new ObjectResult(alarm.Select(p => p.ToViewModel()).ToList());
        }

        [HttpPost("ChangeConversationName")]
        public IActionResult ChangeConversationName(Guid staffId, string conversationId, string newName)
        {
            Args.NotNull(newName, nameof(newName));
            m_IMService.ChangeConversationNameAsync(staffId.ToString(), conversationId, newName).Wait();
            return new HttpOkResult();
        }

        [HttpPost("AddMember")]
        public IActionResult AddMember(Guid taskId, Guid creatorId, Guid[] staffIds, string conversationId)
        {
            if (!staffIds.Any()) throw new FineWorkException("请选择要添加的成员.");
            Args.NotNull(conversationId, nameof(conversationId));

            var staffNames = new List<string>();
            var creator = StaffExistsResult.Check(m_StaffManager, creatorId).ThrowIfFailed().Staff;
            using (var tx = TxManager.Acquire())
            {
                foreach (var staffId in staffIds)
                {
                    var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                    m_MemberManager.CreateMember(conversationId, staffId);
                    staffNames.Add(staff.Name);
                }

                tx.Complete();
            }
            m_IMService.AddMemberAsync(creatorId.ToString(), conversationId,
                staffIds.Select(p => p.ToString()).ToArray()).Wait();
            m_IMService.SendTextMessageByConversationAsync(taskId, this.AccountId, conversationId, "",
                $" {creator.Name} 邀请 {string.Join(",", staffNames)} 加入聊天室.");
            return new HttpOkResult();
        }

        [HttpPost("DeleteMember")]
        public IActionResult DeleteMember(Guid taskId, Guid creatorId, Guid[] staffIds, string conversationId)
        {
            if (!staffIds.Any()) throw new FineWorkException("请选择要删除的成员.");

            Args.NotNull(conversationId, nameof(conversationId));
            var convMembers = m_MemberManager.FetchMembersByConversationId(conversationId)
                .Select(p => p.Staff).ToList();
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            var staffName = new List<string>();
            var creator = StaffExistsResult.Check(m_StaffManager, creatorId).ThrowIfFailed().Staff;
            using (var tx = TxManager.Acquire())
            {
                foreach (var staffId in staffIds)
                {
                    var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
                    m_MemberManager.DeleteMember(conversationId, staffId);
                    staffName.Add(staff.Name);
                    convMembers.Remove(staff);
                }
                tx.Complete();
            }
            m_IMService.RemoveMemberAsync(creatorId.ToString(), conversationId,
                staffIds.Select(p => p.ToString()).ToArray()).Wait();

            m_IMService.ChangeConversationNameAsync(conversationId, creator, convMembers, task);
            m_IMService.SendTextMessageByConversationAsync(taskId, this.AccountId, conversationId, "",
                $" {creator.Name} 将 {string.Join(",", staffName)} 移出聊天室.");
            return new HttpOkResult();
        }

        [HttpGet("FetchConversationsByStaffId")]
        [AllowAnonymous]
        public IActionResult FetchConversationsByStaffId(Guid staffId)
        {
            var taskConvrs = m_PartakerManager.FetchPartakersByStaff(staffId).Select(p => p.Task)
                .Where(p => p.IsDeserted == null && p.Conversation != null)
                .Select(p => p.ToConversationViewModel()).ToList();

            var taskConversationIds = taskConvrs.Select(p => p.ConversationId).ToArray();

            var alarmConvrs = m_TaskAlarmManager.FetchTaskAlarmsByStaffIdWithTaskId(staffId, null)
                .Where(
                    p =>
                        !taskConversationIds.Contains(p.Conversation.Id) && p.Task.IsDeserted == null &&
                        p.Conversation != null)
                .Select(p => p.ToConversationViewModel()).ToList();

            return new ObjectResult(taskConvrs.Union(alarmConvrs).ToList());
        }

        private StaffAlarmsViewModel ChangeCommunicationStatus(Guid taskId, StaffAlarmsViewModel staffAlarms)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

            var alarmsInCommunication = new List<TaskAlarmEntity>();
            var allAlarms = this.m_TaskAlarmManager.FetchTaskAlarmsByTaskId(taskId)
                .ToList();

            //获取当前tong会议室正在处理的预警7
            var alarmInTong =
                allAlarms.Where(
                    p => p.ResolveStatus == ResolveStatus.UnResolved && p.Conversation.Id == task.Conversation.Id)
                    .OrderBy(p => p.CreatedAt).ToList();

            //获取shine会议室正在处理的预警
            var alarmsInShine =
                allAlarms.Where(
                    p => p.ResolveStatus == ResolveStatus.UnResolved && p.Conversation.Id != task.Conversation.Id)
                    .OrderBy(p => p.CreatedAt).ToList();


            alarmsInShine = alarmsInShine.GroupBy(p => p.Conversation)
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

        private string GetAlarmAttDirectory(AlarmEntity alarm)
        {
            return $"tasks/alarms/{alarm.Id}";
        }

        private void UploadTaskAlarmAtt(AlarmEntity alarm, IFormFile file)
        {
            if (alarm==null) return;
            var path = GetAlarmAttDirectory(alarm); 
            try
            { 
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var fileStream = reader.BaseStream;
                    if (alarm.TempletKind == AlarmTempKinds.Image)
                        fileStream = ImageUtil.CutFromCenter(fileStream, 800, null);

                    this.m_FileManager.CreateFile(path, file.ContentType, fileStream);
                }
            }
            catch
            {
                throw new FineWorkException("文件上传失败.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Security;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using FineWork.Web.WebApi.Core;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Common;
using FineWork.Net.IM;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Colla.Impls;
using FineWork.Logging;
using Microsoft.Extensions.Configuration;
using FineWork.Message;
using Microsoft.Extensions.Logging;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Tasks")]
    [Authorize("Bearer")]
    public class TaskController : FwApiController
    {
        public TaskController(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IIMService imservice,
            IConfiguration config,
            ITaskLogManager taskLogManager,
            INotificationManager notificationManager,
            ITaskAlarmManager taskAlarmManager,
            IVoteManager voteManager,
            IAnnouncementManager announcementManager,
            IAnncAlarmManager anncAlarmManager,
            ITaskVoteManager taskVoteManager,
            IConversationManager conversationManager,
            ITaskTempManager taskTempManager)
            : base(sessionProvider)
        {
            if (taskManager == null) throw new ArgumentException("TaskManager");
            if (imservice == null) throw new ArgumentException("imservice");
            if (config == null) throw new ArgumentException("config");
            if (taskLogManager == null) throw new ArgumentException("taskLogManager");
            if (notificationManager == null) throw new ArgumentException("notificationManager");

            m_TaskManager = taskManager;
            m_Config = config;
            m_IMService = imservice;
            m_TaskLogManager = taskLogManager;
            m_SessionProvider = sessionProvider;
            m_NotificationManager = notificationManager;
            m_TaskAlarmManager = taskAlarmManager;
            m_VoteManager = voteManager;
            m_AnnouncementManager = announcementManager;
            m_AnncAlarmManager = anncAlarmManager;
            m_TaskVoteManager = taskVoteManager;
            m_ConversationManager = conversationManager;
            m_TaskTempManager = taskTempManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly ISessionProvider<AefSession> m_SessionProvider;
        private readonly INotificationManager m_NotificationManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly IVoteManager m_VoteManager;
        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly IAnncAlarmManager m_AnncAlarmManager;
        private readonly ITaskVoteManager m_TaskVoteManager;
        private readonly ILogger m_Logger = LogManager.GetLogger(typeof(TaskController));
        private readonly IConversationManager m_ConversationManager;
        private readonly ITaskTempManager m_TaskTempManager;

        [HttpPost("CreateTask")]
        //[DataScoped(true)]
        public TaskViewModel CreateTask([FromBody] CreateTaskModel taskModel)
        {
            if (taskModel == null) throw new ArgumentNullException(nameof(taskModel));

            using (var tx = TxManager.Acquire())
            {
                var task = m_TaskManager.CreateTask(taskModel);

                var result = task.ToViewModel();
                tx.Complete();
                return result;
            }
        }

        [HttpGet("FindTaskById")]
        public IActionResult FindTaskById(Guid taskId)
        {
            var task = m_TaskManager.FindTask(taskId);

            return task != null
                ? new ObjectResult(task.ToDetailViewModel(false, false, true))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchTaskCurtInfoByOrgId")]
        public IActionResult FetchTaskCurtInfoByOrgId(Guid orgId)
        {
            using (var sp = m_SessionProvider.GetSession())
            {
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var tasks =
                    m_TaskManager.FetchTasksByOrgId(orgId, includeAll: false)
                        .Select(p => p.ToSimpleViewModel())
                        .ToList();
                if (tasks.Any())
                    return new ObjectResult(tasks);
                return new HttpNotFoundObjectResult(orgId);
            }
        }

        [HttpGet("FetchTaskDetailInfosByStaffId")]
        [IgnoreDataScoped]
        public IActionResult FetchTaskDetailInfosByStaffId(Guid staffId)
        {
            using (var sp = m_SessionProvider.GetSession())
            {
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var tasks = this.m_TaskManager.FetchTasksByStaffId(staffId).AsParallel().ToList();

                if (tasks.Any())
                {
                    var result = tasks.Select(t => t.ToDetailViewModel(true, false)).ToList<TaskViewModel>();
                    return new ObjectResult(result);
                }

                return new HttpNotFoundObjectResult(staffId);
            }
        }

        [HttpGet("FetchTasksByStaffId")]
        [IgnoreDataScoped]
        public IActionResult FetchTasksByStaffId(Guid staffId)
        {
            var tasks = this.m_TaskManager.FetchTasksByStaff(staffId).ToList();
            if (tasks.Any())
                return new ObjectResult(tasks.Select(p => p.ToViewModel()));

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FetchSharedTasksByOrgId")]
        [IgnoreDataScoped]
        public IActionResult FetchSharedTasksByOrgId(Guid orgId)
        {
            var sharedTasks = m_TaskTempManager.FecthTaskTempsByOrgId(orgId).ToList();
            if (sharedTasks.Any())
                return
                    new ObjectResult(sharedTasks.Select(p => p.ToViewModel()).OrderByDescending(p => p.Copys).ToList());

            return new HttpNotFoundObjectResult(orgId);
        }

        [HttpGet("FetchTasksByName")]
        public IActionResult FetchTasksByName(Guid orgId, string name, int? page, int? pageSize)
        {
            var pageResult =
                this.m_TaskManager.FetchTaskByName(orgId, name)
                    .OrderByDescending(p => p.CreatedAt).Select(p => p.ToSimpleViewModel())
                    .AsQueryable()
                    .ToPagedList(page, pageSize);

            if (page == null && pageSize == null)
                return new ObjectResult(pageResult.Data);

            if (pageResult.Data.Any())
                return new ObjectResult(pageResult);

            return new HttpNotFoundObjectResult(name);
        }

        [HttpGet("FetchTaskDetailInfosByorgId")]
        public IActionResult FetchTaskDetailInfosByOrgId(Guid orgId, bool includeEnd = false)
        {
            using (var sp = m_SessionProvider.GetSession())
            {
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var task = this.m_TaskManager.FetchTasksByOrgId(orgId, includeEnd).ToList();
                if (task.Any())
                    return new ObjectResult(task.Select(p => p.ToDetailViewModel(true, false)));
                return new HttpNotFoundObjectResult(orgId);
            }
        }

        /// <summary> 获取员工的所有相关任务. </summary>
        [HttpGet("FindTaskGroupedListsByStaffId")]
        public TaskGroupedListsViewModel FindTaskGroupedListsByStaffId(Guid staffId)
        {
            Func<IEnumerable<TaskEntity>, PartakerKinds, IEnumerable<TaskViewModel>> filter =
                (coll, kind) =>
                    coll.Where(t => t.Partakers.Any(p => p.Staff.Id == staffId && p.Kind == kind))
                        .Select(t => t.ToViewModel());

            var tasks = this.m_TaskManager.FetchTasksByStaffId(staffId).ToList();
            return new TaskGroupedListsViewModel()
            {
                AsCollabrator = filter(tasks, PartakerKinds.Collaborator).ToList(),
                AsLeader = filter(tasks, PartakerKinds.Leader).ToList(),
                AsMentor = filter(tasks, PartakerKinds.Mentor).ToList(),
                AsRecipient = filter(tasks, PartakerKinds.Recipient).ToList(),
            };

        }

        /// <summary> 获取某机构下所有任务 </summary>
        [HttpGet("FetchTasksByOrgAndStaff")]
        public IEnumerable<TaskViewModel> FetchTasksByOrgAndStaff(Guid orgId, Guid staffId)
        {
            using (var sp = m_SessionProvider.GetSession())
            {
                var tasks =
                    sp.DbContext.Set<TaskEntity>()
                        .Include("Creator.Account")
                        .Include("Creator.Org")
                        .Include("ParentTask")
                        .Include("Partakers.Staff.Account")
                        .Include("Report")
                        .Where(p => p.Creator.Org.Id == orgId && p.Partakers.Any(s => s.Staff.Id == staffId)).ToList()
                        .Select(p => p.ToViewModel(true)).ToList();

                return tasks;
            }

        }

        [HttpGet("FetchTasksWithAlarmByOrgId")]
        [HttpGet("FetchTasksDetailInfoByOrgId")]
        [IgnoreDataScoped]
        public IActionResult FetchTasksWithAlarmByOrgId(Guid orgId)
        {
            using (var sp = m_SessionProvider.GetSession())
            {
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var tasks = m_TaskManager.FetchTasksByOrgId(orgId).ToList();
                if (!tasks.Any())
                    return new HttpNotFoundObjectResult(orgId);

                var result = tasks.Select(s => s.ToDetailViewModel(true)).ToList();

                return new ObjectResult(result.OrderBy(p => p.Level).ToList());
            }
        }

        [HttpGet("FetchTaskNumByStaffId")]
        public int FetchTaskNumByStaffId(Guid staffId)
        {
            return this.m_TaskManager.FetchTaskNumByStaffId(staffId);
        }

        [HttpPost("ChangeParentTask")]
        public IActionResult ChangeParentTask(Guid taskId, Guid? parentTaskId)
        {
            if (parentTaskId.HasValue && parentTaskId == taskId)
            {
                throw new FineWorkException("父任务不可与当前任务相同.");
            }
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            var parentTask = new TaskEntity();
            if (parentTaskId.HasValue)
            {
                parentTask = TaskExistsResult.Check(m_TaskManager, parentTaskId.Value).ThrowIfFailed().Task;
            }
            using (var tx = TxManager.Acquire())
            {
                m_TaskManager.ChangeParentTask(this.AccountId, taskId, parentTaskId);
                tx.Complete();
            }

            if (parentTaskId.HasValue)
            {

                var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeParentTask"],
                    partaker.Staff.Name,
                    parentTask.Name);

                SendMessageWhenTaskPropertyChangeAsync(task.Id, this.AccountId, task.Conversation.Id, task.Name, message);
            }
            return new HttpOkResult();
        }

        //[HttpPost("CreateTaskOnSharedTask")]
        //public IActionResult CreateTaskBySharedTaskId([FromBody] CreateTaskOnSharedModel model)
        //{
        //    Args.NotNull(model, nameof(model));
        //    using (var tx = TxManager.Acquire())
        //    {
        //        var task = m_TaskManager.CreateTask(model);

        //        var result = task.ToViewModel();
        //        tx.Complete();
        //        return new ObjectResult(result);
        //    } 
        //}

        [HttpPost("ShareTask")]
        public IActionResult ShareTask(Guid taskId, Guid staffId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_TaskManager.ShareTask(taskId, staffId);

                tx.Complete();
            }
            return new HttpOkResult();
        }

        [HttpPost("CancelSharedTask")]
        public IActionResult CancelSharedTask(Guid taskId)
        {
            using (var tx = TxManager.Acquire())
            {
                var taskTemp = TaskTempExistsResult.CheckForTask(this.m_TaskTempManager, taskId).TaskTemp;
                m_TaskTempManager.DeleteTaskTemp(taskTemp);

                tx.Complete();
            }
            return new HttpOkResult();
        }
         
        public IActionResult DeleteTask(Guid taskId)
        {
            using (var tx = TxManager.Acquire())
            {
                m_TaskManager.DeleteTask(taskId);
                tx.Complete();
            }
            return new NoContentResult();
        }

        #region 修改task属性

        [HttpPost("ChangeTaskGoal")]
        //[DataScoped(true)]
        public void ChangeTaskGoal(Guid taskId, string newGoal)
        {
            if (!string.IsNullOrEmpty(newGoal))
                Args.MaxLength(newGoal, 128, nameof(newGoal), "任务目标");

            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.Goal == newGoal) return;
            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

            string message;
            if (string.IsNullOrEmpty(newGoal))
                message = $"[任务提示] {partaker.Staff.Name} 清除了任务目标";
            else
                message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskGoal"], partaker.Staff.Name,
                    newGoal);

            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Id, task.Conversation.Id, task.Name,
                message);
            using (var tx = TxManager.Acquire())
            {
                task.Goal = newGoal;
                m_TaskManager.UpdateTask(task);
                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务目标为{newGoal}", "Goal");

                tx.Complete();
            }
        }

        [HttpPost("UpdateTaskLevel")]
        //[DataScoped(true)]
        public void ChangeTaskLevel(Guid taskId, int newLevel)
        {
            if (newLevel <= 0) throw new ArgumentException(nameof(newLevel));
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.Level == newLevel) return;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskLevel"], partaker.Staff.Name,
                this.TransferTaskLevel(newLevel));

            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id, task.Conversation.Id, task.Name,
                message);

            using (var tx = TxManager.Acquire())
            {
                task.Level = newLevel;
                m_TaskManager.UpdateTask(task);
                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务等级为{this.TransferTaskLevel(newLevel)}", "Level");
                tx.Complete();
            }
        }

        [HttpPost("ChangeTaskName")]
        //[DataScoped(true)]
        public async Task ChangeTaskName(Guid taskId, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("任务名称不可为空.", nameof(newName));

            Args.MaxLength(newName, 64, nameof(newName), "任务名称");

            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.Name == newName) return;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            TaskNotExistsResult.CheckInOrg(this.m_TaskManager, task.Creator.Org.Id, newName, taskId).ThrowIfFailed();
            var originalTask = new TaskEntity()
            {
                Id = task.Id,
                Name = task.Name,
                Conversation = task.Conversation
            };

            var changeTaskName = m_IMService.ChangeTaskNameAsync(partaker.Staff.Id.ToString(), originalTask, newName);
            using (var tx = TxManager.Acquire())
            {
                task.Name = newName;
                //更新数据库
                m_TaskManager.UpdateTask(task);

                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务名称为{newName}", "Name");
                await changeTaskName;
                tx.Complete();
            }
            //发送群消息
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskName"], partaker.Staff.Name,
                newName);
            await SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id,
                task.Conversation.Id, task.Name, message);
        }

        [HttpPost("ChangeTaskProgress")]
        //[DataScoped(true)]
        public async Task ChangeTaskProgress(Guid taskId, int newProgress)
        {
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.Progress == newProgress) return;

            if (newProgress == 100 && task.ChildTasks.Any(p => p.Progress != 100))
                throw new FineWorkException("存在未完成的子任务，不可进行此操作。");

            //只有任务负责人可以设置进度条
            var partaker =
                AccountIsPartakerResult.Check(task, this.AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;

            //更新聊天室名称 
            var changeTaskProgress = m_IMService.ChangeTaskProgressAsync(partaker.Staff.Id.ToString(),
                task.Conversation.Id, newProgress);
            using (var tx = TxManager.Acquire())
            {
                task.Progress = newProgress;

                m_TaskManager.UpdateTask(task);
                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务进度为{newProgress}", "Progress");
                await changeTaskProgress;
                tx.Complete();
            }

            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskProgress"], partaker.Staff.Name,
                $"{newProgress}%");
            await SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id,
                task.Conversation.Id, task.Name, message);
        }

        /// <summary>
        /// 是否允许邀请指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newMentorInvStatus"></param>
        [HttpPost("ChangeTaskMentorInvStatus")]
        //[DataScoped(true)]
        public void ChangeTaskMentorInvStatus(Guid taskId, bool newMentorInvStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.IsMentorInvEnabled == newMentorInvStatus) return;
            //必须是Leader才可以修改
            var partaker =
                AccountIsPartakerResult.Check(task, AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;
            var transferStatus = newMentorInvStatus ? "允许" : "禁止";

            using (var tx = TxManager.Acquire())
            {
                task.IsMentorInvEnabled = newMentorInvStatus;
                m_TaskManager.UpdateTask(task);

                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"{transferStatus}邀请指导者", "IsMentorInvEnabled");
                tx.Complete();
            }

            //发送群消息 
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskMentorInvStatus"],
                partaker.Staff.Name, transferStatus);
            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id,
                task.Conversation.Id, task.Name, message);
        }

        /// <summary>
        /// 是否允许要求协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newCollabratorInvStatus"></param>
        [HttpPost("ChangeTaskCollabratorInvStatus")]
        //[DataScoped(true)]
        public void ChangeTaskCollabratorInvStatus(Guid taskId, bool newCollabratorInvStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.IsCollabratorInvEnabled == newCollabratorInvStatus) return;

            //必须是Leader才可以修改
            var partaker =
                AccountIsPartakerResult.Check(task, AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;
            var transferStatus = newCollabratorInvStatus ? "允许" : "禁止";
            using (var tx = TxManager.Acquire())
            {

                task.IsCollabratorInvEnabled = newCollabratorInvStatus;
                m_TaskManager.UpdateTask(task);

                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"{transferStatus}邀请协同者", "IsCollabratorInvEnabled");
                tx.Complete();
            }
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskCollabratorInvStatus"],
                partaker.Staff.Name, transferStatus);


            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id, task.Conversation.Id, task.Name,
                message);

        }


        /// <summary>
        ///是否允许招募
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentStatus"></param>
        [HttpPost("ChangeTaskRecruitmentStatus")]
        //[DataScoped(true)]
        public void ChangeTaskRecruitmentStatus(Guid taskId, bool newRecuitmentStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.IsRecruitEnabled == newRecuitmentStatus) return;
            var statusText = newRecuitmentStatus ? "开启" : "关闭";
            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

            using (var tx = TxManager.Acquire())
            {
                task.IsRecruitEnabled = newRecuitmentStatus;

                //默认招募协同者
                if (String.IsNullOrEmpty(task.RecruitmentRoles))
                    task.RecruitmentRoles = ((int) PartakerKinds.Collaborator).ToString();
                m_TaskManager.UpdateTask(task);


                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"{statusText}任务招募", "RecruitmentRoles");
                tx.Complete();
            }
            //发送群消息
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskRecruitmentStatus"],
                partaker.Staff.Name, statusText);

            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id, task.Conversation.Id, task.Name,
                message);
        }

        /// <summary>
        /// 更新招募的角色
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentRoles"></param>
        [HttpPost("ChangeTaskRecruitmentRoles")]
        //[DataScoped(true)]
        public void ChangeTaskRecruitmentRoles(Guid taskId, int[] newRecuitmentRoles)
        {
            if (!newRecuitmentRoles.Any())
                throw new FineWorkException("请选择招募的角色。");
            var newRecuitmentRoleString = string.Join(",", newRecuitmentRoles.OrderBy(p => p));
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.RecruitmentRoles == newRecuitmentRoleString) return;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            var transferRoles = this.TransferTaskRecruimentRole(newRecuitmentRoleString);
            using (var tx = TxManager.Acquire())
            {
                task.RecruitmentRoles = newRecuitmentRoleString;
                m_TaskManager.UpdateTask(task);

                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务招募角色为{transferRoles}", "RecruitmentRoles");
                tx.Complete();
            }
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskRecruitmentRoles"],
                partaker.Staff.Name, transferRoles);

            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id, task.Conversation.Id,
                task.Name, message);
        }

        /// <summary>
        /// 更新招募的信息
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentDesc"></param>
        [HttpPost("ChangeTaskRecruitmentDesc")]
        //[DataScoped(true)]
        public void ChangeTaskRecruitmentDesc(Guid taskId, string newRecuitmentDesc)
        {
            if (!string.IsNullOrEmpty(newRecuitmentDesc))
                Args.MaxLength(newRecuitmentDesc, 400, nameof(newRecuitmentDesc), "招募说明");
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.RecruitmentDesc == newRecuitmentDesc) return;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            using (var tx = TxManager.Acquire())
            {
                task.RecruitmentDesc = newRecuitmentDesc;
                m_TaskManager.UpdateTask(task);
                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务招募信息为{newRecuitmentDesc}", "RecruitmentDesc");
                tx.Complete();
            }
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskRecruitmentDesc"],
                partaker.Staff.Name);

            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id, task.Conversation.Id, task.Name,
                message);
        }

        [HttpPost("ChangeTaskEndAt")]
        public void ChangeTaskEndAt(Guid taskId, DateTime endAt)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            if (task.EndAt == endAt) return;

            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            using (var tx = TxManager.Acquire())
            {
                task.EndAt = endAt;
                m_TaskManager.UpdateTask(task);
                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, $"修改任务结束时间为{endAt.ToString("yyyy-MM-dd")}", "EndAt");
                tx.Complete();


            }
            var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskEndAt"],
                partaker.Staff.Name, $"{endAt.ToString("yyyy-MM-dd")}");

            SendMessageWhenTaskPropertyChangeAsync(task.Id, partaker.Staff.Account.Id, task.Conversation.Id, task.Name,
                message);
        }

        [HttpPost("AbandonTask")]
        public void AbandonTask(Guid taskId)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

                //必须是Leader才可以修改
                var partaker =
                    AccountIsPartakerResult.Check(task, AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;

                if (task.ChildTasks.Any()) throw new FineWorkException("当前任务存在子任务,不可进行此操作.");
                if (task.Progress == 100) throw new FineWorkException("当前任务已完成,不可进行此操作.");

                task.IsDeserted = true;
                this.m_TaskManager.UpdateTask(task);

                m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                    ActionKinds.UpdateColumn, "放弃了任务", "IsDeserted");

                var taskConv = task.Conversation;
                var alarmConvs = task.Alarms.Select(p => p.Conversation).ToList();
                alarmConvs.Add(taskConv);
                alarmConvs.ForEach(p => { m_ConversationManager.UpdateConversation(p); });

                ManageMessageWhenAbandonTaskAsync(task, partaker).Wait();

                tx.Complete();
            }
        }

        private Task ManageMessageWhenAbandonTaskAsync(TaskEntity task, PartakerEntity leader)
        {
            return Task.Run(async () =>
            {
                var message = string.Format(m_Config["PushMessage:AbandonTask"],
                    leader.Staff.Name, task.Name);

                var aliases =
                    task.Partakers.Where(p => p.Kind != PartakerKinds.Leader)
                        .Select(p => p.Staff.Account.PhoneNumber)
                        .ToArray();
                await m_NotificationManager.SendByAliasAsync("", message, null, aliases);

                await
                    m_IMService.ChangeConAttrAsync(leader.Staff.Id.ToString(), task.Conversation.Id, "IsDeserted", true);

                var alarms = m_TaskAlarmManager.FetchTaskAlarmsByTaskId(task.Id).ToList();

                if (alarms.Any())
                    alarms.ForEach(async p =>
                    {
                        await
                            m_IMService.ChangeConAttrAsync(p.Staff.Id.ToString(), p.Conversation.Id, "IsDeserted", true);
                    });
            });
        }

        /// <summary>
        /// 获取个人中心任务相关的提醒，包括共识，里程碑，共筹。。
        /// </summary>
        /// <returns></returns>
        [HttpGet("FetchTaskNoticesByStaffId")]
        public IActionResult FetchTaskNoticesByStaffId(Guid staffId)
        {
            //共识
            var votes = this.m_VoteManager.FetchVotesByStaffId(staffId)
                .Where(p => p.EndAt <= DateTime.Now && !p.IsApproved.HasValue)
                .Join(m_TaskVoteManager.FetchAllVotes(), u => u.Id, c => c.Vote.Id, (u, c) => new TaskNoticeViewModel
                {
                    TaskId = c.Task.Id,
                    TaskName = c.Task.Name,
                    TargetId = u.Id,
                    Content = u.Subject,
                    NoticeFr = "vote",
                    CreatedAt = u.EndAt
                }).ToList();

            var anncs = this.m_AnncAlarmManager
                .FetchAnncAlarmsByStaffId(staffId)
                .Where(
                    w =>
                        w.Annc.EndAt <= DateTime.Now &&
                        (!w.Annc.Reviews.Any() ||
                         w.Annc.Reviews.All(p => p.Reviewstatus == AnncStatus.Delay && p.DelayAt <= DateTime.Now)))
                .Select(p => new TaskNoticeViewModel
                {
                    TaskId = p.Annc.Task.Id,
                    TaskName = p.Annc.Task.Name,
                    TargetId = p.Annc.Id,
                    Content = p.Annc.Content,
                    NoticeFr = "annc",
                   // CreatedAt =p.Annc.StartAt.HasValue?p.Annc.StartAt.Value.AddMinutes(-(p.BeforeStart ?? 0)):null
                }).ToList();

            if (votes.Any() || anncs.Any())
                return new ObjectResult(votes.Union(anncs));

            return new HttpNotFoundObjectResult(staffId);
        }

        private string TransferTaskRecruimentRole(string roleIds)
        {
            var roleIdArray = roleIds.Split(',');
            List<string> result;

            result = roleIdArray.ToList().ConvertAll(p => ((PartakerKinds) int.Parse(p)).GetLabel());

            return String.Join(",", result);
        }

        private string TransferTaskLevel(int levelId)
        {
            switch (levelId)
            {
                case 1:
                    return "特别重要";
                case 2:
                    return "非常重要";
                case 3:
                    return "比较重要";
                case 4:
                    return "一般重要";
                case 5:
                    return "不重要";
                default:
                    return levelId.ToString();
            }
        }


        private Task SendMessageWhenTaskPropertyChangeAsync(Guid taskId, Guid accountId, string convrId, string title,
            string content)
        {
            return
                Task.Factory.StartNew(
                    () =>
                        m_IMService.SendTextMessageByConversationAsync(taskId, accountId, convrId, title, content)
                            .ContinueWith(
                                t => m_Logger.LogWarning(0, "taskpropertyimwarring", t.Exception),
                                TaskContinuationOptions.OnlyOnFaulted
                            ));
        }

        #endregion
    }
}

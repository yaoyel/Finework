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
using Microsoft.Extensions.Configuration;

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
            ITaskLogManager taskLogManager)
            : base(sessionProvider)
        {
            if (taskManager == null) throw new ArgumentException("TaskManager");
            if (imservice == null) throw new ArgumentException("imservice");
            if (config == null) throw new ArgumentException("config");
            if (taskLogManager == null) throw new ArgumentException("taskLogManager");

            m_TaskManager = taskManager;
            m_Config = config;
            m_IMService = imservice;
            m_TaskLogManager = taskLogManager;
            m_SessionProvider = sessionProvider;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly ISessionProvider<AefSession> m_SessionProvider;

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

            return task != null ? new ObjectResult(task.ToDetailViewModel()) : new HttpNotFoundObjectResult(taskId);
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
                    var result = tasks.Select(t => t.ToDetailViewModel(true)).ToList<TaskViewModel>();
                    return new ObjectResult(result);
                }

                return new HttpNotFoundObjectResult(staffId);
            }

          
        }

        [HttpGet("FetchTaskDetailInfosByorgId")]
        public IActionResult FetchTaskDetailInfosByOrgId(Guid orgId)
        {
            using (var sp = m_SessionProvider.GetSession())
            {
                sp.DbContext.Configuration.LazyLoadingEnabled = false;
                var task = this.m_TaskManager.FetchTasksByOrgId(orgId).ToList();
                if (task.Any())
                    return new ObjectResult(task.Select(p => p.ToDetailViewModel(true,false)));
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
            return m_TaskManager.FetchTasksByOrgId(orgId)
                .Where(p => p.Partakers.Any(s => s.Staff.Id == staffId))
                .Select(task => task.ToViewModel()).ToList();
        }

        [HttpGet("FetchTasksWithAlarmByOrgId")]
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
             
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                if (parentTaskId.HasValue)
                {
                    var parentTask = TaskExistsResult.Check(m_TaskManager, parentTaskId.Value).ThrowIfFailed().Task;

                
                    //发送群消息
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeParentTask"],
                        partaker.Staff.Name,
                        parentTask.Name);
                    m_IMService.SendTextMessageByConversationAsync(task.Id,this.AccountId, task.ConversationId, task.Name, message); 
                }

                m_TaskManager.ChangeParentTask(this.AccountId,taskId, parentTaskId);
                tx.Complete();

                return new HttpStatusCodeResult(200);
            }
        }

        #region 修改task属性

        [HttpPost("ChangeTaskGoal")]
        //[DataScoped(true)]
        public void ChangeTaskGoal(Guid taskId, string newGoal)
        {
            if (!string.IsNullOrEmpty(newGoal))
                Args.MaxLength(newGoal, 128, nameof(newGoal), "任务目标");

            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                task.Goal = newGoal;
                m_TaskManager.UpdateTask(task);
                tx.Complete();

                string message;
                if (string.IsNullOrEmpty(newGoal))
                    message = $"[任务提示] {partaker.Staff.Name} 清除了任务目标";
                //发送群消息
                else
                    message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskGoal"], partaker.Staff.Name,
                        newGoal);

                Task.Factory.StartNew(async () =>
                {
                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id,task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务目标为{newGoal}","Goal");
                });


            }

        }

        [HttpPost("UpdateTaskLevel")]
        //[DataScoped(true)]
        public void ChangeTaskLevel(Guid taskId, int newLevel)
        {
            if (newLevel <= 0) throw new ArgumentException(nameof(newLevel));
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                task.Level = newLevel;
                m_TaskManager.UpdateTask(task);

                tx.Complete();
                //发送群消息
                var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskLevel"], partaker.Staff.Name,
                    this.TransferTaskLevel(newLevel));

                Task.Factory.StartNew(async () =>
                {
                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务等级为{this.TransferTaskLevel(newLevel)}","Level");
                });

            }

        }

        [HttpPost("ChangeTaskName")]
        //[DataScoped(true)]
        public void ChangeTaskName(Guid taskId, string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("任务名称不可为空.", nameof(newName));

            Args.MaxLength(newName, 64, nameof(newName), "任务名称");
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                TaskNotExistsResult.CheckInOrg(this.m_TaskManager, task.Creator.Org.Id, newName, taskId).ThrowIfFailed();

                var originalTask = new TaskEntity()
                {
                    Id = task.Id,
                    Name = task.Name,
                    ConversationId = task.ConversationId
                };
                task.Name = newName;
                //更新数据库
                m_TaskManager.UpdateTask(task);

                tx.Complete();


                Task.Factory.StartNew(async () =>
                {
                    //更新聊天室名称 
                    await m_IMService.ChangeTaskName(partaker.Staff.Id.ToString(), originalTask, newName);

                    //发送群消息
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskName"], partaker.Staff.Name,
                        newName);
                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务名称为{newName}","Name");
                });
            }

        }

        [HttpPost("ChangeTaskProgress")]
        //[DataScoped(true)]
        public void ChangeTaskProgress(Guid taskId, int newProgress)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
                //只有任务负责人可以设置进度条
                var partaker =
                    AccountIsPartakerResult.Check(task, this.AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;

                task.Progress = newProgress;

                m_TaskManager.UpdateTask(task);
                tx.Complete();

                Task.Factory.StartNew(async () =>
                {
                    //更新聊天室名称 
                    await m_IMService.ChangeTaskProgress(partaker.Staff.Id.ToString(), task.ConversationId, newProgress);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务进度为{newProgress}","Progress");
                });


            }
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
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

                //必须是Leader才可以修改
                var partaker =
                    AccountIsPartakerResult.Check(task, AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;
                task.IsMentorInvEnabled = newMentorInvStatus;
                m_TaskManager.UpdateTask(task);
                tx.Complete();

                Task.Factory.StartNew(async () =>
                {
                    var transferStatus = newMentorInvStatus ? "允许" : "禁止";
                    //发送群消息 
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskMentorInvStatus"],
                        partaker.Staff.Name, transferStatus);

                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"{transferStatus}邀请指导者", "IsMentorInvEnabled");
                });


            }
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
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;

                //必须是Leader才可以修改
                var partaker =
                    AccountIsPartakerResult.Check(task, AccountId, PartakerKinds.Leader).ThrowIfFailed().Partaker;
                AccountIsPartakerResult.Check(task, AccountId, PartakerKinds.Leader).ThrowIfFailed();
                task.IsCollabratorInvEnabled = newCollabratorInvStatus;
                m_TaskManager.UpdateTask(task);
                tx.Complete();

                Task.Factory.StartNew(async () =>
                {
                    var transferStatus = newCollabratorInvStatus ? "允许" : "禁止";
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskCollabratorInvStatus"],
                        partaker.Staff.Name, transferStatus);

                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"{transferStatus}邀请协同者", "IsCollabratorInvEnabled");
                });
            }
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
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed();

                task.IsRecruitEnabled = newRecuitmentStatus;

                //默认招募协同者
                if (String.IsNullOrEmpty(task.RecruitmentRoles))
                    task.RecruitmentRoles = ((int) PartakerKinds.Collaborator).ToString();
                m_TaskManager.UpdateTask(task);

                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                var statusText = newRecuitmentStatus ? "开启" : "关闭";
                tx.Complete();

                Task.Factory.StartNew(async () =>
                {

                    //发送群消息
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskRecruitmentStatus"],
                        partaker.Staff.Name, statusText); 

                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"{statusText}任务招募", "RecruitmentRoles");
                });

            }
        }

        /// <summary>
        /// 更新招募的角色
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentRoles"></param>
        [HttpPost("ChangeTaskRecruitmentRoles")]
        //[DataScoped(true)]
        public void ChangeTaskRecruitmentRoles(Guid taskId, string newRecuitmentRoles)
        {
            if (string.IsNullOrEmpty(newRecuitmentRoles))
                throw new FineWorkException("请选择招募的角色。");
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
            using (var tx = TxManager.Acquire())
            {
                task.RecruitmentRoles = newRecuitmentRoles;
                m_TaskManager.UpdateTask(task);

                tx.Complete();

                Task.Factory.StartNew(async () =>
                {
                    var transferRoles = this.TransferTaskRecruimentRole(newRecuitmentRoles);
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskRecruitmentRoles"],
                        partaker.Staff.Name, transferRoles);
                    ;
                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);


                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务招募角色为{transferRoles}", "RecruitmentRoles");
                });


            }
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

            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task; 

                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;

                task.RecruitmentDesc = newRecuitmentDesc;
                m_TaskManager.UpdateTask(task);
                tx.Complete();


                Task.Factory.StartNew(async () =>
                {
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskRecruitmentDesc"],
                        partaker.Staff.Name); 

                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id,partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务招募信息为{newRecuitmentDesc}", "RecruitmentDesc"); 
                });
            }
        }

        [HttpPost("ChangeTaskEndAt")]
        public void ChangeTaskEndAt(Guid taskId, DateTime endAt)
        {
            using (var tx = TxManager.Acquire())
            {
                var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
                var partaker = AccountIsPartakerResult.Check(task, this.AccountId).ThrowIfFailed().Partaker;
                task.EndAt = endAt;
                m_TaskManager.UpdateTask(task);
                tx.Complete();

                Task.Factory.StartNew(async () =>
                {
                    var message = string.Format(m_Config["LeanCloud:Messages:Task:ChangeTaskEndAt"],
                        partaker.Staff.Name,endAt.ToString("YYYY/mm/dd mm:ss"));

                    await
                        m_IMService.SendTextMessageByConversationAsync(task.Id, partaker.Staff.Account.Id, task.ConversationId, task.Name, message);
                    m_TaskLogManager.CreateTaskLog(task.Id, partaker.Staff.Id, task.GetType().FullName, task.Id,
                        ActionKinds.UpdateColumn, $"修改任务结束时间为{endAt.ToString("YYYY/mm/dd mm:ss")}", "EndAt");
                });
            }
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

        #endregion
    }
}

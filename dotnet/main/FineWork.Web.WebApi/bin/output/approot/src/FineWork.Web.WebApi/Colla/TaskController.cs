using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Security;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Authorization;
using FineWork.Web.WebApi.Core;
using AppBoot.Checks;
using AppBoot.Transactions;
using FineWork.Common;

namespace FineWork.Web.WebApi.Colla
{
    [Route("api/Tasks")]
    [Authorize("Bearer")]
    public class TaskController : FwApiController
    {
        public TaskController(ISessionScopeFactory sessionScopeFactory, ITaskManager taskManager
            , IPartakerManager partakerManager,
            IStaffManager staffManager,
            ITaskIncentiveManager taskIncentiveManager)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentException("sessionScopeFactory");
            if (taskManager == null) throw new ArgumentException("TaskManager");
            if (taskIncentiveManager == null) throw new ArgumentException("taskIncentiveManager");

            m_TaskManager = taskManager;
            m_PartakerManager = partakerManager;
            m_TaskIncentiveManager = taskIncentiveManager;
            m_StaffManager = staffManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly ITaskIncentiveManager m_TaskIncentiveManager;
        private readonly IStaffManager m_StaffManager;

        [HttpPost("CreateTask")]
        [DataScoped(true)]
        public TaskViewModel CreateTask([FromBody] CreateTaskModel taskModel)
        {
            if (taskModel == null) throw new ArgumentNullException(nameof(taskModel));

            var task = m_TaskManager.CreateTask(taskModel);
            return task.ToViewModel();
        }

        [HttpGet("FindTaskById")]
        public IActionResult FindTaskById(Guid taskId)
        {
            var task = m_TaskManager.FindTask(taskId);

            return task != null ? new ObjectResult(task.ToDetailViewModel()) : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchTaskDetailInfosByStaffId")]
        public IEnumerable<TaskDetailViewModel> FetchTaskDetailInfosByStaffId(Guid staffId)
        {
            var tasks = this.m_TaskManager.FetchTasksByStaffId(staffId);
            return tasks.Select(t => t.ToDetailViewModel()).ToList();
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
        public IEnumerable<TaskViewModel> FetchTasksByOrgAndStaff(Guid orgId,Guid staffId)
        {
            return m_TaskManager.FetchTasksByOrgId(orgId)
                .Where(p=>p.Partakers.Any(s=>s.Staff.Id==staffId))
                .Select(task => task.ToViewModel()).ToList();
        }

        [HttpGet("FetchTasksWithAlarmByOrgId")]
        public IEnumerable<TaskWithAlarmViewModel> FetchTasksWithAlarmByOrgId(Guid orgId)
        {
            return m_TaskManager.FetchTasksByOrgId(orgId)
                .Select(task => task.ToViewModelWithAlarm())
                .OrderBy(p => p.Level)
                .ToList();
        }

        #region 修改task属性

        [HttpPost("ChangeTaskGoal")]
        [DataScoped(true)]
        public void ChangeTaskGoal(Guid taskId, string newGoal)
        {
            if (string.IsNullOrEmpty(newGoal))
                throw new ArgumentException("value cannot be null or empty", nameof(newGoal));

            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.Goal = newGoal;
            m_TaskManager.UpdateTask(task);
        }

        [HttpPost("UpdateTaskLevel")]
        [DataScoped(true)]
        public void ChangeTaskLevel(Guid taskId, int newLevel)
        {
            if (newLevel == 0) throw new ArgumentException(nameof(newLevel));
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            task.Level = newLevel;
            m_TaskManager.UpdateTask(task);
        }

        [HttpPost("ChangeTaskName")]
        [DataScoped(true)]
        public void ChangeTaskName(Guid taskId, string newName)
        { 
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("value cannot be null or empty", nameof(newName));
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;
            TaskNotExistsResult.CheckInOrg(this.m_TaskManager, task.Creator.Org.Id, newName).ThrowIfFailed();
            task.Name = newName;
            m_TaskManager.UpdateTask(task);
        } 

        [HttpPost("ChangeTaskProgress")]
        [DataScoped(true)]
        public void ChangeTaskProgress(Guid taskId, int newProgress)
        {
            var task = TaskExistsResult.Check(m_TaskManager, taskId).ThrowIfFailed().Task;

            //只有任务负责人可以设置进度条
            AccountIsPartakerResult.Check(task, this.AccountId, PartakerKinds.Leader).ThrowIfFailed();
            
            task.Progress = newProgress;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 是否允许邀请指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newMentorInvStatus"></param>
        [HttpPost("ChangeTaskMentorInvStatus")]
        [DataScoped(true)]
        public void ChangeTaskMentorInvStatus(Guid taskId, bool newMentorInvStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsMentorInvEnabled = newMentorInvStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 是否允许要求协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newCollabratorInvStatus"></param>
        [HttpPost("ChangeTaskCollabratorInvStatus")]
        [DataScoped(true)]
        public void ChangeTaskCollabratorInvStatus(Guid taskId, bool newCollabratorInvStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsCollabratorInvEnabled = newCollabratorInvStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 是否允许申请负责人
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newLeaderReqStatus"></param>
        [HttpPost("ChangeTaskLeaderReqStatus")]
        [DataScoped(true)]
        public void ChangeTaskLeaderReqStatus(Guid taskId, bool newLeaderReqStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsLeaderReqEnabled = newLeaderReqStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 是否允许申请指导者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newMentorReqStatus"></param>
        [HttpPost("ChangeTaskMentorReqStatus")]
        [DataScoped(true)]
        public void ChangeTaskMentorReqStatus(Guid taskId, bool newMentorReqStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsMentorReqEnabled = newMentorReqStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 是否允许申请接受者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecruitStatus"></param>
        [HttpPost("ChangeTaskRecruitStatus")]
        [DataScoped(true)]
        public void ChangeTaskRecruitStatus(Guid taskId, bool newRecruitStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsRecruitEnabled = newRecruitStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 是否允许申请协同者
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newCollabratorReqStatus"></param>
        [HttpPost("ChangeTaskCollabratorReqStatus")]
        [DataScoped(true)]
        public void UpdateTaskCollabratorReqStatus(Guid taskId, bool newCollabratorReqStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsCollabratorReqEnabled = newCollabratorReqStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        ///是否允许招募
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentStatus"></param>
        [HttpPost("ChangeTaskRecruitmentStatus")]
        [DataScoped(true)]
        public void ChangeTaskRecruitmentStatus(Guid taskId, bool newRecuitmentStatus)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.IsRecruitEnabled = newRecuitmentStatus;
            m_TaskManager.UpdateTask(task);
        }

        /// <summary>
        /// 更新招募的角色
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentRoles"></param>
        [HttpPost("ChangeTaskRecruitmentRoles")]
        [DataScoped(true)]
        public void ChangeTaskRecruitmentRoles(Guid taskId, int[] newRecuitmentRoles)
        {
            if (!newRecuitmentRoles.Any())
                throw new FineWorkException("请选择招募的角色。");
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.RecruitmentRoles = string.Join(";", newRecuitmentRoles);
            m_TaskManager.UpdateTask(task); 
        }

        /// <summary>
        /// 更新招募的信息
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="newRecuitmentDesc"></param>
        [HttpPost("ChangeTaskRecruitmentDesc")]
        [DataScoped(true)]
        public void ChangeTaskRecruitmentDesc(Guid taskId, string newRecuitmentDesc)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            task.RecruitmentDesc = newRecuitmentDesc;
            m_TaskManager.UpdateTask(task);
        } 

        #endregion
    }
}

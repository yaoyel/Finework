using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Avatar;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class TaskLogManager : AefEntityManager<TaskLogEntity, Guid>, ITaskLogManager
    {
        public TaskLogManager(ISessionProvider<AefSession> dbContextProvider,
            ITaskManager taskManager,
            IStaffManager staffManager)
            : base(dbContextProvider)
        {
            TaskManager = taskManager;
            StaffManager = staffManager;
        }


        private ITaskManager TaskManager { get; }
        private IStaffManager StaffManager { get; set; }

        public TaskLogEntity CreateTaskLog(Guid taskId, Guid staffId, string targetKind, Guid targetId,
            ActionKinds actionKind,
            string message,string columnName="")
        {
            var task = TaskExistsResult.Check(TaskManager, taskId).Task;
            var staff = StaffExistsResult.Check(StaffManager, staffId).Staff;
       
            var taskLog = new TaskLogEntity()
            {
                Id = Guid.NewGuid(),
                TargetId = targetId,
                TargetKind = string.IsNullOrEmpty(columnName)?targetKind: string.Concat(targetKind, ".", columnName),
                Staff = staff,
                Task = task,
                ActionKind = actionKind.GetLabel(),
                Message = message
            };

            this.InternalInsert(taskLog);
            return taskLog;
        }

        public IEnumerable<TaskLogEntity> FetchTaskLogByTaskId(Guid taskId,bool includeAll= false)
        {
            var insert = ActionKinds.InsertColumn.GetLabel();
            var update = ActionKinds.UpdateColumn.GetLabel();
            var delete = ActionKinds.DeleteColumn.GetLabel();

            var result= this.InternalFetch(p => p.Task.Id == taskId && p.Staff.IsEnabled) 
                .OrderByDescending(p => p.CreatedAt);
            if (includeAll) return result;
            return result.Where(p => p.ActionKind != insert
                                     && p.ActionKind != update
                                     && p.ActionKind != delete);

        }

        /// <summary>
        ///  获取对任务属性更新的日志，这部分日志不显示在任务中心页
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="cloumnName"></param>
        /// <returns></returns>
        public IEnumerable<TaskLogEntity> FetchUpdateLogByTaskId(Guid taskId, string cloumnName)
        {
            var insert = ActionKinds.InsertColumn.GetLabel();
            var update = ActionKinds.UpdateColumn.GetLabel();
            var delete = ActionKinds.DeleteColumn.GetLabel();

            return this.InternalFetch(p => p.TargetId == taskId && p.Staff.IsEnabled
                                           && p.TargetKind.EndsWith(cloumnName)
                                           && (p.ActionKind == insert
                                               || p.ActionKind == update
                                               || p.ActionKind == delete))
                .OrderByDescending(p => p.CreatedAt);
        }

        public IEnumerable<TaskLogEntity> FetchExcitationLogByTaskId(Guid taskId)
        { 
            return this.InternalFetch(p => p.Task.Id == taskId);
        }
    }
}

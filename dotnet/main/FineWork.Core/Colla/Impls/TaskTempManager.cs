using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;

namespace FineWork.Colla.Impls
{
    public class TaskTempManager : AefEntityManager<TaskTempEntity, Guid>, ITaskTempManager
    {
        public TaskTempManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(staffManager, nameof(staffManager));

            m_StaffManager = staffManager;
            m_TaskManager = taskManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        public TaskTempEntity CreateTaskTemp(Guid taskId, Guid staffId)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(m_StaffManager, staffId).ThrowIfFailed().Staff;

            var taskTemp=new TaskTempEntity();
            taskTemp.Task = task;
            taskTemp.Staff = staff;
            taskTemp.Id = Guid.NewGuid();
            this.InternalInsert(taskTemp);
            return taskTemp;
        }

        public void UpdateTaskTemp(TaskTempEntity taskTemp)
        {
            Args.NotNull(taskTemp, nameof(taskTemp));

            this.InternalUpdate(taskTemp);
        }

        public TaskTempEntity FindById(Guid taskTempId)
        {
            return this.InternalFind(taskTempId);
        }

        public TaskTempEntity FindByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId).FirstOrDefault();
        }

        public IEnumerable<TaskTempEntity> FecthTaskTempsByOrgId(Guid orgId)
        {
            return this.InternalFetch(p => p.Staff.Org.Id== orgId);
        }

        public void DeleteTaskTemp(TaskTempEntity taskTemp)
        {
            if(taskTemp!=null)
                this.InternalDelete(taskTemp);
        }
    }
}
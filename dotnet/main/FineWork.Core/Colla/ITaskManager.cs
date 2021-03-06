using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskManager
    {
        TaskEntity CreateTask(CreateTaskModel taskModel);

        TaskEntity FindTask(Guid taskId);

        /// <summary> 在一个组织中根据名称查找顶级任务. </summary>
        TaskEntity FindTaskByNameInOrg(Guid orgId, String taskName);

        /// <summary> 在一个任务中根据名称查找子任务. </summary>
        TaskEntity FindTaskByNameInParent(Guid parentTaskId, String taskName);

        //TODO: Rename FetchTasksByCreatorStaff(Guid staffId);
        IEnumerable<TaskEntity> FetchTasksByStaff(params Guid[] ids);

        /// <summary> 查找某个员工所有参与的任务. </summary>
        IEnumerable<TaskEntity> FetchTasksByStaffId(Guid staffId);

        /// <summary> 查找某个组织的所有任务. </summary>
        IEnumerable<TaskEntity> FetchTasksByOrgId(Guid orgId);

        int FetchTaskNumByStaffId(Guid staffId);

        void UpdateTask(TaskEntity task);

        void ChangeParentTask(Guid accountId,Guid taskId, Guid? parentTaskId); 
    }
}
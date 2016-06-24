using System;
using System.Collections.Generic;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskManager
    {
        TaskEntity CreateTask(CreateTaskModel taskModel);

        TaskEntity FindTask(Guid taskId);

        /// <summary> ��һ����֯�и������Ʋ��Ҷ�������. </summary>
        TaskEntity FindTaskByNameInOrg(Guid orgId, String taskName);

        /// <summary> ��һ�������и������Ʋ���������. </summary>
        TaskEntity FindTaskByNameInParent(Guid parentTaskId, String taskName);

        //TODO: Rename FetchTasksByCreatorStaff(Guid staffId);
        IEnumerable<TaskEntity> FetchTasksByStaff(params Guid[] ids);

        /// <summary> ����ĳ��Ա�����в��������. </summary>
        IEnumerable<TaskEntity> FetchTasksByStaffId(Guid staffId);

        /// <summary> ����ĳ����֯����������. </summary>
        IEnumerable<TaskEntity> FetchTasksByOrgId(Guid orgId);

        int FetchTaskNumByStaffId(Guid staffId);

        void UpdateTask(TaskEntity task);

        void ChangeParentTask(Guid accountId,Guid taskId, Guid? parentTaskId); 
    }
}
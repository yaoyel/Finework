using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface ITaskTempManager
    {
        TaskTempEntity CreateTaskTemp(Guid taskId, Guid staffId); 

        void UpdateTaskTemp(TaskTempEntity taskTemp);

        TaskTempEntity FindById(Guid taskTempId);

        TaskTempEntity FindByTaskId(Guid taskId);

        IEnumerable<TaskTempEntity> FecthTaskTempsByOrgId(Guid orgId);

        void DeleteTaskTemp(TaskTempEntity taskTemp);
    }
}
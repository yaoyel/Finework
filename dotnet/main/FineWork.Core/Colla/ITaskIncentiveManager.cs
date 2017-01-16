using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface ITaskIncentiveManager
    {
        TaskIncentiveEntity FindTaskIncentiveById(Guid incentiveId);

        TaskIncentiveEntity UpdateTaskIncentive(Guid taskId,int kindId,decimal amount);

        TaskIncentiveEntity FindTaskIncentiveByTaskIdAndKindId(Guid taskId,int kindId,bool returnNull=false);

        IEnumerable<TaskIncentiveEntity> FetchTaskIncentiveByTaskId(Guid taskId,bool includeAllKind=true);

        void DeleteTaskIncentiveByTaskId(Guid taskId);
    }
}

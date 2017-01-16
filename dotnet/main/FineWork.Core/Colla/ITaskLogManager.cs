using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface ITaskLogManager
    {
        TaskLogEntity CreateTaskLog(Guid taskId,Guid staffId,string targetKind,Guid targetId,ActionKinds actionKind,string message,string columnName="");

        IEnumerable<TaskLogEntity> FetchTaskLogByTaskId(Guid taskId,bool includeAll=false);

        IEnumerable<TaskLogEntity> FetchExcitationLogByTaskId(Guid taskId);

        IEnumerable<TaskLogEntity> FetchUpdateLogByTaskId(Guid taskId,string columnName);
    }
}

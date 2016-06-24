using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    interface IExcPartakerManager
    {
        TaskExilsEntity CreateExcPartaker(Guid taskReportId, Guid partakerId);

        void DeleteExcPartaker(Guid excPartakerId);

        TaskExilsEntity FindExcPartakerById(Guid excPartakerId);

        IEnumerable<TaskExilsEntity> FecthExcPartakersByReportId(Guid taskReportId);

    }
}

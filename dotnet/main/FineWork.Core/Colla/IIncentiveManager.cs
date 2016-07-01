using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IIncentiveManager
    {
        IncentiveEntity CreateIncentive(Guid taskId, int incentiveKindId, Guid senderStaffId,
            Guid receiverStaffId, decimal quantity);

        IEnumerable<IncentiveEntity> FetchIncentiveByTaskId(Guid taskId); 
         
    }
}

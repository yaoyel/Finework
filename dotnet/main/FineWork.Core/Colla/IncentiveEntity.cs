using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class IncentiveEntity:EntityBase<Guid>
    {
        public  decimal Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual TaskIncentiveEntity TaskIncentive { get; set; }

        public virtual StaffEntity SenderStaff { get; set; }

        public virtual StaffEntity ReceiverStaff { get; set; } 
    }
}

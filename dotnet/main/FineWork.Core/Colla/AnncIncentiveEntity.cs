using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnncIncentiveEntity:EntityBase<Guid>
    {
        //预定的值
        public decimal  Amount { get; set; }

        //实际发放的值
        public decimal? Grant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual IncentiveKindEntity IncentiveKind { get; set; }

        public virtual AnnouncementEntity Announcement { get; set; }

    }
}

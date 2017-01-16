﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnncReviewEntity:EntityBase<Guid>
    {
        public  AnncStatus Reviewstatus { get; set; }

        public AnnouncementEntity Annc { get; set; }

        public DateTime? DelayAt { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;
    }
}

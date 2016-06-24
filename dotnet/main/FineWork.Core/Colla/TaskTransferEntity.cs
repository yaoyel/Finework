using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{ 
    public class TaskTransferEntity:EntityBase<Guid>
    {
        public ReviewStatuses AttStatus { get; set; } = ReviewStatuses.Unspecified;

        public ReviewStatuses DetStatus { get; set; }= ReviewStatuses.Unspecified;

        public Guid? AttReviewStaffId { get; set; }

        public Guid? DetReviewStaffId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? AttachedAt { get; set; }

        public DateTime? DetachedAt { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public virtual StaffEntity Staff { get; set; }

        /// <summary>
        /// 迁出的任务
        /// </summary>
        public virtual TaskEntity Task { get; set; }

        /// <summary>
        /// 迁入的任务
        /// </summary>
        public virtual TaskEntity AttachedTask { get; set; }

    }
}

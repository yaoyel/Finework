using System;
using FineWork.Common;

namespace FineWork.Colla
{
    /// <summary> 代表一个任务的成员申请. </summary>
    public class PartakerReqEntity : EntityBase<Guid>
    {
        /// <summary> 任务. </summary>
        public virtual TaskEntity Task { get; set; }

        /// <summary> 申请人. </summary>
        public virtual StaffEntity Staff { get; set; }

        /// <summary> 成员资格类型 </summary>
        public PartakerKinds PartakerKind { get; set; }

        /// <summary> 申请内容. </summary>
        public String Message { get; set; }

        /// <summary> 申请日期. </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary> 审批状态. </summary>
        public ReviewStatuses ReviewStatus { get; set; }

        /// <summary> 审批日期. </summary>
        public DateTime ReviewAt { get; set; }
    }
}
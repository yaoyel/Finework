using System;
using FineWork.Common;

namespace FineWork.Colla
{
    public class PartakerEntity : EntityBase<Guid>
    {
        public PartakerKinds Kind { get; set; }

        public virtual StaffEntity Staff { get; set; }

        public virtual TaskEntity Task { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //�Ƿ����ͻ����ս��
        public bool? IsExils { get; set; } 
    }
}
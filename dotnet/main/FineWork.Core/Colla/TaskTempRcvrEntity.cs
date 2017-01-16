using System;
using FineWork.Common;

namespace FineWork.Colla
{
    public class TaskTempRcvrEntity:EntityBase<Guid>
    {
        public StaffEntity Staff { get; set; }

        public TaskTempEntity TaskTemp { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
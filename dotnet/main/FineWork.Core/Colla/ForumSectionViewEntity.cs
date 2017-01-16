using System;
using FineWork.Common;

namespace FineWork.Colla
{
    public class ForumSectionViewEntity : EntityBase<Guid> 
    {
        public virtual StaffEntity Staff { get; set; }

        public virtual ForumSectionEntity ForumSection { get; set; }

        public DateTime CreatedAt { get; set; }=DateTime.Now;
    }
}
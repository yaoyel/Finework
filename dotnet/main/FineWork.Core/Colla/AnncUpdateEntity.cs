using System;
using FineWork.Common;

namespace FineWork.Colla
{
    public class AnncUpdateEntity:EntityBase<Guid>
    {
        public virtual StaffEntity Staff { get; set; }

        public virtual AnnouncementEntity Annc { get; set; }

        public DateTime CreatedAt { get; set; }=  DateTime.Now;
        
    }
}
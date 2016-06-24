using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
namespace FineWork.Web.WebApi.Colla
{
    public class StaffInvViewModel
    {
        public Guid StaffInvId { get; set; }

        public OrgViewModel Org { get; set; } 

        public DateTime CreatedAt { get; set; }

        public string Message { get; set; }

        public string InviterNames { get; set; }

        public virtual void AssignFrom(StaffInvEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            StaffInvId = entity.Id;
            Org = entity.Org.ToViewModel(); 
            Message = entity.Message;
            CreatedAt = entity.CreatedAt;
            InviterNames = entity.InviterNames;
        }

    }

    public static class StaffInvViewModelExtensions
    {
        public static StaffInvViewModel ToViewModel(this StaffInvEntity entity)
        {
            var result = new StaffInvViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }
}
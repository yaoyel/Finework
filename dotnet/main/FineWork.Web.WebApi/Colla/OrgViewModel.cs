using System;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class OrgViewModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public StaffViewModel AdminStaff { get; set; }

        public bool IsInvEnabled { get; set; }

        public string SecurityStamp { get; set; }

        public virtual void AssignFrom(OrgEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.Name = entity.Name;
            this.AdminStaff = entity.AdminStaff?.ToViewModel();
            this.IsInvEnabled = entity.IsInvEnabled;
            this.SecurityStamp = entity.SecurityStamp;
        }
    }

    public static class OrgViewModelExtensions
    {
        public static OrgViewModel ToViewModel(this OrgEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new OrgViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

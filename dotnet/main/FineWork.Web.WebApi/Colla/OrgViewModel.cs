using System;
using System.Linq;
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

        public Guid[] ReqAccIds { get; set; }

        public Guid[] DisabledAccIds { get; set; }

        public virtual void AssignFrom(OrgEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.Name = entity.Name;
            this.AdminStaff = entity.AdminStaff?.ToViewModel();
            this.IsInvEnabled = entity.IsInvEnabled;
            this.SecurityStamp = entity.SecurityStamp;
            this.ReqAccIds = entity.StaffReqs.Where(p=>p.ReviewStatus==ReviewStatuses.Unspecified)
                .Select(p => p.Account.Id).ToArray();
            this.DisabledAccIds = entity.Staffs.Where(p => !p.IsEnabled).Select(p => p.Account.Id).ToArray();
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

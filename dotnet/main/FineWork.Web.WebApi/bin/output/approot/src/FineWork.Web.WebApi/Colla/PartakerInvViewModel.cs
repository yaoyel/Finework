using System;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class PartakerInvViewModel
    {
        public Guid Id { get; set; }

        public TaskViewModel Task { get; set; }

        public StaffViewModel Staff { get; set; }

        public PartakerKinds PartakerKind { get; set; }

        public String InviterNames { get; set; }

        public String Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ReviewStatuses ReviewStatus { get; set; }

        public DateTime ReviewAt { get; set; }

        public void AssignFrom(PartakerInvEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.Task = entity.Task.ToViewModel();
            this.Staff = entity.Staff.ToViewModel();
            this.PartakerKind = entity.PartakerKind;
            this.InviterNames = entity.InviterNames;
            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt;
            this.ReviewStatus = entity.ReviewStatus;
            this.ReviewAt = entity.ReviewAt;
        }
    }

    public static class PartakerInvViewModelExtensions
    {
        public static PartakerInvViewModel ToViewModel(this PartakerInvEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PartakerInvViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}
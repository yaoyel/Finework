using System;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class PartakerViewModel
    {
        public Guid Id { get; set; }

        public Guid TaskId { get; set; }

        public Guid StaffId { get; set; }

        public PartakerKinds Kind { get; set; }

        public virtual void AssignFrom(PartakerEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.TaskId = entity.Task.Id;
            this.StaffId = entity.Staff.Id;
            this.Kind = entity.Kind;
        }
    }

    public class PartakerDetailViewModel : PartakerViewModel
    {
        public TaskViewModel Task { get; set; }

        public StaffViewModel Staff { get; set; }

        public override void AssignFrom(PartakerEntity entity)
        {
            base.AssignFrom(entity);

            this.Task = entity.Task.ToViewModel();
            this.Staff = entity.Staff.ToViewModel();
        }
    }

    public static class PartakerViewModelExtensions
    {
        public static PartakerViewModel ToViewModel(this PartakerEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new PartakerViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static PartakerDetailViewModel ToDetailViewModel(this PartakerEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new PartakerDetailViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Security.Checkers;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskTransferViewModel
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public StaffViewModel Staff { get; set; }

        public TaskViewModel Task { get; set; }

        public TaskViewModel AttachedTask { get; set; }

        public StaffViewModel AttReviewStaff { get; set; }

        public StaffViewModel DetReviewStaff { get; set; }

        public ReviewStatuses AttStatus { get; set; }

        public ReviewStatuses DetStatus { get; set; }

        public virtual void AssignFrom(TaskTransferEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            Id = entity.Id;
            CreatedAt = entity.CreatedAt;
            Staff = entity.Staff.ToViewModel();
            Task = entity.Task.ToViewModel();
            AttachedTask = entity.AttachedTask.ToViewModel();
            AttStatus = entity.AttStatus;
            DetStatus = entity.DetStatus;
        } 
    
    }

    public static class TaskTransferViewModelExtensions
    {
        public static TaskTransferViewModel ToViewModel(this TaskTransferEntity entity,IStaffManager staffManager)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskTransferViewModel();
            result.AssignFrom(entity);
            if (entity.AttReviewStaffId.HasValue)
                result.AttReviewStaff = staffManager.FindStaff(entity.AttReviewStaffId.Value).ToViewModel();
            if (entity.DetReviewStaffId.HasValue)
                result.DetReviewStaff = staffManager.FindStaff(entity.DetReviewStaffId.Value).ToViewModel();

            return result;
        }
    }
}

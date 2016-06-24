using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskSharingViewModel
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; } 

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        public StaffViewModel Staff { get; set; } 

        public TaskViewModel Task { get; set; }

        public virtual void AssignFrom(TaskSharingEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Id = entity.Id;
            FileName = entity.FileName;
            CreatedAt = entity.CreatedAt;
            Size = entity.Size;
            Staff = entity.Staff.ToViewModel();
            Task = entity.Task.ToViewModel();
            ContentType = entity.ContentType;
        } 
    }

    public static class TaskSharingViewModelExtensions
    {
        public static TaskSharingViewModel ToViewModel(this TaskSharingEntity entity)
        {
            var result = new TaskSharingViewModel();
            result.AssignFrom(entity);
            return result;
        }

    } 
}

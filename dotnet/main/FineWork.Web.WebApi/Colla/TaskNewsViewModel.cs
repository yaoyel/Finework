using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskNewsViewModel
    {
        public Guid Id { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        [Necessity(NecessityLevel.Low)]
        public StaffViewModel Staff { get; set; } 

        [Necessity(NecessityLevel.Low)]
        public TaskViewModel Task { get; set; }

        public virtual void AssignFrom(TaskNewsEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));  

            this.Id = entity.Id;
            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt; 
            this.Staff = entity.Staff.ToViewModel();
            this.Task = entity.Task.ToViewModel();
        }
    }

    public static class TaskNewsExtensions
    {
        public static TaskNewsViewModel ToViewModel(this TaskNewsEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskNewsViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

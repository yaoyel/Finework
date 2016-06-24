using System;
using AppBoot.Common;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskReportAttViewModel
    {
        public Guid AttId { get; set; }

        public TaskSharingViewModel TaskShaing { get; set; }
      
        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(TaskReportAttEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            Args.NotNull(entity, nameof(entity));
 
            this.AttId = entity.Id;
            this.CreatedAt = entity.CreatedAt;
            this.TaskShaing = entity.TaskSharing.ToViewModel(false,false);
        }
    }

    public static class TaskReportAttViewModelExtensions
    {
        public static TaskReportAttViewModel ToViewModel(this TaskReportAttEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new TaskReportAttViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }

    }

}
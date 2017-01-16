using System;
using AppBoot.Common;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskTempViewModel
    {
        public StaffViewModel Staff { get; set; }

        public TaskViewModel Task { get; set; }

        public DateTime SharedAt { get; set; }

        public int Copys { get; set; }

        public StaffViewModel SharedBy { get; set; }

        public virtual void AssignFrom(TaskTempEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            Args.NotNull(entity, nameof(entity));
            var taskManager = (ITaskManager)HttpUtil.HttpContext.RequestServices.GetService(typeof(ITaskManager));
            var copys = taskManager.CountCopysBySharedTaskId(entity.Task.Id);

            Staff = entity.Staff.ToViewModel(true, false);
            Task = entity.Task.ToViewModel(true); 
            SharedAt = entity.LastUpdatedAt ?? entity.CreatedAt;
            SharedBy = entity.Staff.ToViewModel(true);
            Copys = copys;
        } 
    }

    public static class TaskTempEntityExtensions
    {
        public static TaskTempViewModel ToViewModel(this TaskTempEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskTempViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }
}
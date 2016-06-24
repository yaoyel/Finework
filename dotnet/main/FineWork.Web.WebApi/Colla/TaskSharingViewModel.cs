using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;

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

        [Necessity(NecessityLevel.Low)]
        public TaskViewModel Task { get; set; }

        public virtual void AssignFrom(TaskSharingEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));


            var propertiesDic = new Dictionary<string, Func<TaskSharingEntity, dynamic>>
            {
                ["Id"] = (t) => t.Id,
                ["CreatedAt"] = (t) => t.CreatedAt,
                ["FileName"] = (t) => t.FileName,
                ["ContentType"] = (t) => t.ContentType,
                ["Size"] = (t) => t.Size,
                ["Staff"] = (t) => t.Staff.ToViewModel(isShowhighOnly, isShowLow),
                ["Task"] = (t) => t.Task.ToViewModel(isShowhighOnly, isShowLow)
            };

            NecessityAttributeUitl<TaskSharingViewModel, TaskSharingEntity>.SetVuleByNecssityAttribute(this, entity,
                propertiesDic, isShowhighOnly,
                isShowLow);

        }
    }

    public static class TaskSharingViewModelExtensions
    {
        public static TaskSharingViewModel ToViewModel(this TaskSharingEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            var result = new TaskSharingViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }

    }

}
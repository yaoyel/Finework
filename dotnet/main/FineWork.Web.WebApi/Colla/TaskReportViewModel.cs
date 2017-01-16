using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskReportViewModel
    {
        public Guid ReportId { get; set; }

        public DateTime EndedAt { get; set; }

        public TaskViewModel Task { get; set; }

        public List<PartakerViewModel> Partakers { get; set; }

        public List<TaskViewModel> ParentTasks { get; set; }

        public string Summary { get; set; }

        public decimal EffScore { get; set; }

        public decimal QualityScore { get; set; }

        public List<PartakerViewModel> Exilses { get; set; }

        public List<TaskReportAttViewModel> Atts { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        public virtual void AssignFrom(TaskReportEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.ReportId = entity.Id;
            this.EndedAt = entity.EndedAt;
            this.Task = entity.Task.ToViewModel();
            this.Partakers = entity.Task.Partakers?.Select(p => p.ToViewModel(true)).ToList();
            this.ParentTasks = GetParentTasks(entity.Task);
            this.Summary = entity.Summary;
            this.EffScore = entity.EffScore;
            this.QualityScore = entity.QualityScore;
            this.Exilses =
                entity.Task.Partakers?.Where(p => p.IsExils.HasValue && p.IsExils.Value)
                    .Select(p => p.ToViewModel(true))
                    .ToList();
            this.Atts = entity.Atts?.Select(p => p.ToViewModel()).ToList();
            this.CreatedAt = entity.CreatedAt;
            this.LastUpdatedAt = entity.LastUpdatedAt;
        }

        private List<TaskViewModel> GetParentTasks(TaskEntity entity, List<TaskViewModel> parentTasks = null)
        {

            if (entity == null) return parentTasks;
            if (entity.ParentTask == null) return parentTasks;
            if (entity.ToViewModel().Layer == 1) return parentTasks;

            if (parentTasks == null) parentTasks = new List<TaskViewModel>();

            parentTasks.Add(entity.ParentTask.ToViewModel());

            GetParentTasks(entity.ParentTask, parentTasks);

            return parentTasks;
        }

    }

    public static class TaskReportViewModelExtensions
    {
        public static TaskReportViewModel ToViewModel(this TaskReportEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new TaskReportViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }

    }
}

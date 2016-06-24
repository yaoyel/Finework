using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class AlarmPeriodViewModel
    {
       public Guid Id { get; set; }

        public int Weekdays { get; set; }

        public string ShortTime { get; set; }

        public bool IsEnabled { get; set; }

        public TaskViewModel Task { get; set; }

        public virtual void AssignFrom(AlarmPeriodEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Weekdays = entity.Weekdays;
            this.ShortTime = entity.ShortTime;
            this.IsEnabled = entity.IsEnabled;
            this.Task = entity.Task.ToViewModel();
        }
    }

    public static class AlarmPeriodViewModelExtensions
    {
        public static AlarmPeriodViewModel ToViewModel(this AlarmPeriodEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AlarmPeriodViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{

    public class IncentiveKindViewModel
    { 
        public int Id { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }
        public virtual void AssignFrom(IncentiveKindEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Name = entity.Name;
            this.Unit = entity.Unit; 
        }
    }

    public static class IncentiveKindViewModelExtensions
    {
        public static IncentiveKindViewModel ToViewModel(this IncentiveKindEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new IncentiveKindViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
    public class TaskIncentiveViewModel
    {
        public int Amount { get; set; }

        public IncentiveKindViewModel IncentiveKind { get; set; } 

        public virtual void AssignFrom(TaskIncentiveEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Amount = entity.Amount;
            this.IncentiveKind = entity.IncentiveKind.ToViewModel(); 
        }
    }

    public static class TaskIncentiveViewModelExtensions
    {
        public static TaskIncentiveViewModel ToViewModel(this TaskIncentiveEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskIncentiveViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }


}

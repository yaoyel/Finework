using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{

    #region IncentiveKindViewModel
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

    #endregion

    #region TaskIncentiveViewModel
    public class TaskIncentiveViewModel
    {
        public decimal Amount { get; set; }

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
    #endregion

    #region IncentiveViewModel

    public class IncentiveViewModel
    {
        public StaffViewModel SenderStaff { get; set; }

        public StaffViewModel ReceiverStaff { get; set; }

        public IncentiveKindViewModel IncentiveKind { get; set; }

        public decimal Quantity { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(IncentiveEntity entity)
        {
            Args.NotNull(entity, nameof(entity));
            this.SenderStaff = entity.SenderStaff.ToViewModel();
            this.ReceiverStaff = entity.ReceiverStaff.ToViewModel();
            this.IncentiveKind = entity.TaskIncentive.IncentiveKind.ToViewModel();
            this.Quantity = entity.Quantity;
            this.CreatedAt = entity.CreatedAt;
        }

    }


    public static class IncentiveViewModelExtensions
    {
        public static IncentiveViewModel ToViewModel(this IncentiveEntity entity)
        {
            if (entity == null) throw new ArgumentException(nameof(entity));
            var result = new IncentiveViewModel();
            result.AssignFrom(entity);
            return result;
        }

    } 
    #endregion


}

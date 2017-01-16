using System;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class PushLogViewModel
    {
        public Guid Id { get; set; } 
      
        public TaskViewModel Task { get; set; }

        public string Content { get; set; }

        public PushKinds TargetType { get; set; }

        public Guid TargetId { get; set; }

        public string ShortTime { get; set; }

        public int Repeat { get; set; }

        public bool IsViewed { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(PushLogEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Task = entity.Task.ToViewModel(true,false); 
            this.Content = entity.Content;
            this.TargetId = entity.TargetId;
            this.TargetType = entity.TargetType;
            this.ShortTime = entity.ShortTime;
            this.Repeat = entity.Repeat;
            this.IsViewed = entity.IsViewed;
            this.CreatedAt = entity.CreatedAt;
        }
    } 


    public static class PushLogViewModelExtensions
    {
        public static PushLogViewModel ToViewModel(this PushLogEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PushLogViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}
 
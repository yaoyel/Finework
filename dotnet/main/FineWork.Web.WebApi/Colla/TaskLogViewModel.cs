using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskLogViewModel
    {
        public Guid Id { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public StaffViewModel Staff { get; set; } 

        public virtual void AssignFrom(TaskLogEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt;
            this.Staff = entity.Staff.ToViewModel(); 
        }
    }

    public class TaskSummaryViewModel
    {
        public string Message { get; set; } 
        
        public Guid TargetId { get; set; }

        //note纪要 annc 里程碑 childtask 子任务
        public string Type { get; set; }

        public DateTime? CreatedAt { get; set; }

        //里程碑状态 0 未处理 1 已达成 2 未达成
        public int? AnncStatus { get; set; }

        public DateTime? EndAt { get; set; }

        //annc的开始时间
        public DateTime? StartAt { get; set; }

        //计划是否和我有关系
        public bool RelatedToMe { get; set; }
        
        public bool IsDraft { get; set; }
    }

    public static class TaskLogViewModelExtensions
    {
        public static TaskLogViewModel ToViewModel(this TaskLogEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskLogViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

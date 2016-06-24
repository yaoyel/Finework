using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Colla.Checkers;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskAlarmViewModel
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public string AlarmKind { get; set; }

        public bool IsResolved { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public string Comment { get; set; } 

        public DateTime CreatedAt { get; set; }

        public TaskViewModel Task { get;set; }

        public StaffViewModel StaffFrom { get; set; }

        public List<StaffViewModel> StaffTo { get; set; }

        public virtual void AssignFrom(TaskAlarmEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            Id = entity.Id; 
             AlarmKind = entity.TaskAlarmKind.GetLabel();
            IsResolved = entity.IsResolved;
            ResolvedAt = entity.ResolvedAt;
            Comment = entity.Comment;
            CreatedAt = entity.CreatedAt;
            Task = entity.Task.ToViewModel();
            StaffFrom = entity.Staff.ToViewModel(); 
            //任务管理者的预警发送到任务指导者，其他人发送到任务管理者
            //获取发送者的partakerkind
            var partaker = PartakerExistsResult.CheckForStaff(entity.Task, entity.Staff.Id).Partaker;
            if (partaker.Kind == PartakerKinds.Mentor)
                StaffTo = entity.Task.Partakers.Where(p => p.Kind == PartakerKinds.Leader).Select(p=>p.Staff.ToViewModel()).ToList();

            StaffTo = entity.Task.Partakers.Where(p => p.Kind == PartakerKinds.Mentor).Select(p => p.Staff.ToViewModel()).ToList();

        } 
    }

    public static class TaskAlarmViewModelExtensions
    {
        public static TaskAlarmViewModel ToViewModel(this TaskAlarmEntity entity)
        {
            var result = new TaskAlarmViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }
}
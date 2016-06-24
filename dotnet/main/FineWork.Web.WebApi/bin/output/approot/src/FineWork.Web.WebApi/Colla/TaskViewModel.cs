using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public string ConversationId { get; set; }

        public virtual void AssignFrom(TaskEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.Name = entity.Name;
            this.ConversationId = entity.ConversationId;
        }
    }

    public class TaskDetailViewModel : TaskViewModel
    {

        public string Goal { get; set; }

        public int Level { get; set; }

        public int Progress { get; set; }
        public StaffViewModel Creator { get; set; }

        public Guid? ParentTaskId { get; set; }  

        public bool IsMentorInvEnabled { get; set; }
         
        public bool IsCollabratorInvEnabled { get; set; } 
         
        public bool IsLeaderReqEnabled { get; set; }
         
        public bool IsMentorReqEnabled { get; set; }
         
        public bool IsCollabratorReqEnabled { get; set; }
         
        public bool IsRecruitEnabled { get; set; } 

        public int[] RecruitmentRoles { get; set; }

        public string RecruitmentDesc { get; set; }

        public bool IsTopEnabled { get; set; }

        public IList<PartakerDetailViewModel> Partakers { get; set; }

        public IList<PartakerInvViewModel> PartakerInvs { get; set; }

        public IList<TaskIncentiveViewModel> TaskIncentives { get; set; }

        public IList<TaskViewModel> ChildTasks { get; set; }
        public override void AssignFrom(TaskEntity entity)
        {
            base.AssignFrom(entity);
            Goal = entity.Goal;
            Level = entity.Level;
            Progress = entity.Progress;
            ParentTaskId = entity.ParentTask?.Id; 
            IsMentorInvEnabled = entity.IsMentorInvEnabled;
            IsCollabratorInvEnabled = entity.IsCollabratorInvEnabled;
            IsLeaderReqEnabled = entity.IsLeaderReqEnabled;
            IsMentorReqEnabled = IsMentorReqEnabled;
            IsCollabratorReqEnabled = entity.IsCollabratorReqEnabled;
            IsRecruitEnabled = entity.IsRecruitEnabled;
            RecruitmentRoles = !string.IsNullOrEmpty(entity.RecruitmentRoles)
                ? Array.ConvertAll(entity.RecruitmentRoles.Split(';'), int.Parse)
                : new int[] {}; 
            RecruitmentDesc = entity.RecruitmentDesc;
            Creator = entity.Creator.ToViewModel();
            Partakers = entity.Partakers.Select(p => p.ToDetailViewModel()).ToList();
            PartakerInvs = entity.PartakerInvs.Where(p=>p.ReviewStatus!=ReviewStatuses.Approved)
                 .GroupBy(p => p.Staff).Select(p => p.First()).Select(p => p.ToViewModel()).ToList();
            TaskIncentives = entity.Incentives.Select(p => p.ToViewModel()).ToList();
            ChildTasks = entity.ChildTasks.Select(p => p.ToViewModel()).ToList();
        }
    }

    public class TaskGroupedListsViewModel
    {
        public IList<TaskViewModel> AsCollabrator { get; set; }

        public IList<TaskViewModel> AsLeader { get; set; }

        public IList<TaskViewModel> AsMentor { get; set; }

        public IList<TaskViewModel> AsRecipient { get; set; }
    }

    public class TaskWithAlarmViewModel : TaskDetailViewModel
    {
        public IEnumerable<TaskAlarmViewModel> Alarms { get; set; }

        public override void AssignFrom(TaskEntity entity)
        {
            base.AssignFrom(entity);
            if(entity.Alarms.Any())
            this.Alarms = entity.Alarms.Select(p => p.ToViewModel()).ToList(); 
        }
    }

    public static class TaskExtensions
    {
        public static TaskViewModel ToViewModel(this TaskEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static TaskDetailViewModel ToDetailViewModel(this TaskEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskDetailViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static TaskWithAlarmViewModel ToViewModelWithAlarm(this TaskEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskWithAlarmViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}
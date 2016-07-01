using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public string ConversationId { get; set; }

        public string Goal { get; set; }

        public int Level { get; set; }

        public int Progress { get; set; }

        public StaffViewModel Creator { get; set; }

        public Guid? ParentTaskId { get; set; }

        //任务的层级，从父任务中迭代出来
        public int Layer { get; set; } 

        public DateTime CreatedAt { get; set; }

        public Guid[] AvatarIds { get; set; }

        public DateTime? EndAt { get; set; }

        public bool IsEnded { get; set; }
        public virtual void AssignFrom(TaskEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));


            this.Id = entity.Id;
            this.Name = entity.Name;
            this.ConversationId = entity.ConversationId;
            this.Creator = entity.Creator.ToViewModel(isShowhighOnly, isShowLow);
            this.Goal = entity.Goal;
            this.Progress = entity.Progress;
            this.Level = entity.Level;
            this.ParentTaskId = entity.ParentTask?.Id;
            this.Layer = GetLayerByParentTask(entity);
            this.CreatedAt = entity.CreatedAt;
            this.AvatarIds = entity.Partakers.OrderBy(p => p.CreatedAt).Take(9).Select(p => p.Staff.Account.Id).ToArray();
            this.EndAt = entity.EndAt;
            this.IsEnded = entity.Report != null;
        }

        protected int GetLayerByParentTask(TaskEntity entity, int layer = 1)
        { 
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity.ParentTask == null || entity.ParentTask.Id==entity.Id) return layer; 

            layer++;
            return GetLayerByParentTask(entity.ParentTask, layer); 
        }
    }

    public class TaskDetailViewModel : TaskViewModel
    { 
        public IList<PartakerViewModel> Partakers { get; set; }

        public bool IsMentorInvEnabled { get; set; }

        public bool IsCollabratorInvEnabled { get; set; }

        public bool IsRecruitEnabled { get; set; }

        public int[] RecruitmentRoles { get; set; }

        public string RecruitmentDesc { get; set; } 

        public IList<PartakerInvViewModel> PartakerInvs { get; set; }

        public IList<TaskIncentiveViewModel> TaskIncentives { get; set; }

        public IList<TaskViewModel> ChildTasks { get; set; }

        public IList<TaskNewsViewModel> GoodNews { get; set; }

        public IList<TaskAlarmViewModel> TaskAlarms { get; set; }


        public override void AssignFrom(TaskEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            base.AssignFrom(entity, isShowhighOnly, isShowLow);

            this.IsMentorInvEnabled = entity.IsMentorInvEnabled;
            this.IsCollabratorInvEnabled = entity.IsCollabratorInvEnabled;
            this.IsRecruitEnabled = entity.IsRecruitEnabled;
            this.RecruitmentRoles = !string.IsNullOrEmpty(entity.RecruitmentRoles)
                ? Array.ConvertAll(entity.RecruitmentRoles.Split(','), int.Parse)
                : new int[] {};
            this.RecruitmentDesc = entity.RecruitmentDesc;
            this.Partakers = entity.Partakers.Where(p => p.Staff.IsEnabled)
                .Select(p => p.ToViewModel(isShowhighOnly, isShowLow))
                .ToList();
            this.TaskIncentives = entity.Incentives.Select(p => p.ToViewModel()).ToList();

            this.ChildTasks = entity.ChildTasks.Where(p => p.Creator.IsEnabled)
                .Select(p => p.ToViewModel(isShowhighOnly, isShowLow))
                .ToList();
            this.GoodNews = entity.Newses.Where(p=>p.Staff.IsEnabled)
                .Select(p => p.ToViewModel())
                .ToList();

            this.TaskAlarms =
                entity.Alarms.Where(p => p.Staff.IsEnabled && p.TaskAlarmKind != TaskAlarmKinds.GreenLight)
                    .Select(p => p.ToViewModel(isShowhighOnly, isShowLow))
                    .ToList();
        } 
    }

    public class TaskGroupedListsViewModel
    {
        public IList<TaskViewModel> AsCollabrator { get; set; }

        public IList<TaskViewModel> AsLeader { get; set; }

        public IList<TaskViewModel> AsMentor { get; set; }

        public IList<TaskViewModel> AsRecipient { get; set; }
    }

    public static class TaskExtensions
    {
        public static TaskViewModel ToViewModel(this TaskEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        } 

        public static TaskDetailViewModel ToDetailViewModel(this TaskEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskDetailViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }

    }

}
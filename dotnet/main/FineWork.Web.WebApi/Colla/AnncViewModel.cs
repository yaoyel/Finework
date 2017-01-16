using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Colla;
using FineWork.Colla.Models;

namespace FineWork.Web.WebApi.Colla
{
    public class AnncViewModel
    {
        public Guid AnncId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? EndAt { get; set; }

        public DateTime? StartAt { get; set; }

        public bool IsNeedAchv { get; set; }

        public List<AnncAttViewModel> Atts { get; set; }

        public List<AnncReviewViewModel> Reviews { get; set; }
         
        public StaffViewModel Staff { get; set; }

        public StaffViewModel Executor { get; set; }

        public List<StaffViewModel> Executors { get; set; }

        public StaffViewModel Inspecter { get; set; }

        public List<AnncAlarmViewModel> Alarms { get; set; }

        public Guid TaskId { get; set; }

        public List<AnncUpdateViewModel> Updates { get; set; }

        public string ConversationId { get; set; }

        public bool IsDraft { get; set; }

        public virtual void AssignFrom(AnnouncementEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var executors = source.Executors.Select(p => p.Staff.ToViewModel(true, false)).ToList();
            this.TaskId = source.Task.Id;
            this.ConversationId = source.Task.ConversationId;
            this.AnncId = source.Id;
            this.Content = source.Content;
            this.CreatedAt = source.CreatedAt;
            this.EndAt = source.EndAt;
            this.StartAt = source.StartAt;
            this.IsNeedAchv = source.IsNeedAchv;
            var anncReviews = source.Reviews.OrderByDescending(p => p.CreatedAt);

            if (source.Updates.Any())
                this.Updates = source.Updates.Select(p => p.ToViewModel()).ToList();

            if (!anncReviews.Any())
            {
                if (source.EndAt <= DateTime.Now)
                    Reviews = new List<AnncReviewViewModel>()
                    {
                        new AnncReviewViewModel()
                        {
                            CreatedAt = source.CreatedAt,
                            Status = AnncStatus.Unspecified
                        }
                    };
            } 
            else
            {
                Reviews = anncReviews.Select(p => p.ToViewModel()).OrderByDescending(p => p.CreatedAt).ToList();
            } 
            this.Atts = source.Atts.Select(p => p.ToViewModel(false, false)).ToList();
            this.Staff = source.Creator.ToViewModel(true, false);
            this.Executor = executors.Count() == 1 ? executors.First() : null;
            this.Executors = executors;
            this.Inspecter = source.Inspecter?.ToViewModel(true, false);
            this.Alarms = source.Alarms.Select(p => p.ToViewModel(true, false)).ToList();

            //IsDraft = !source.StartAt.HasValue || !source.Executors.Any() || source.Inspecter == null;
        }
    }

    public class AnncAttViewModel
    { 
        public Guid AttId { get; set; }

        public TaskSharingViewModel TaskSharing { get; set; }

        public bool IsAchv { get; set; }

        public virtual void AssignFrom(AnncAttEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            AttId = source.Id;
            TaskSharing = source.TaskSharing.ToViewModel(false,false);
            IsAchv = source.IsAchv;
        }
    }

    public class AnncReviewViewModel
    {
        public AnncStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } 

        public DateTime? DelayAt { get; set; }
        public virtual void AssignFrom(AnncReviewEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.Status = source.Reviewstatus;
            this.CreatedAt = source.CreatedAt;
            this.DelayAt = source.DelayAt;
        }
    }

    public class AnncUpdateViewModel
    {
        public StaffViewModel Staff { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(AnncUpdateEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source)); 
          
            this.CreatedAt = source.CreatedAt;
            this.Staff = source.Staff.ToViewModel(true);
        }
    }

    public class AnncWithTaskViewModel
    {
        public Guid AnncId { get; set; }

        public string Content { get; set; }
        
        public TaskViewModel Task { get; set; }

        public virtual void AssignFrom(AnnouncementEntity source)
        {
            Args.NotNull(source, nameof(source));

            AnncId = source.Id;
            Content = source.Content;
            Task = source.Task.ToViewModel();
        }

    }

    public class AnncAlarmRecViewModel
    {
        public Guid RecId { get; set; }

        public StaffViewModel Staff { get; set; }

        public AnncRoles Role { get; set; }

        public virtual void AssignFrom(AnncAlarmRecEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            RecId = source.Id;
            Staff = source.Staff.ToViewModel(false, false);
            Role = source.AnncRole;
        }
    }

    public class AnncAlarmViewModel
    {
        public Guid AlarmId { get; set; }

        public DateTime? Time { get; set; }

        public string Bell { get; set; }

        public bool IsEnabled { get; set; }

        public IList<AnncAlarmRecViewModel> Recs { get; set; }

        public string BeforeStart { get; set; }

        public virtual void AssignFrom(AnncAlarmEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            AlarmId = source.Id;
            Time = source.Time;
            Recs = source.Recs.Select(p => p.ToViewModel(isShowhighOnly, isShowLow)).ToList();
            Bell = source.Bell;
            IsEnabled = source.IsEnabled &&
                        ((source.Time ?? source.Annc.StartAt??source.Annc.StartAt.Value.AddMinutes(-(source.BeforeStart ?? 0))) > DateTime.Now);
            BeforeStart = source.BeforeStart==null?"" : AlarmTimeConverter.BackConverter(source.BeforeStart.Value);
        }

       
    }

    public static class AnncAttViewModelExtensions
    {
        public static AnncAttViewModel ToViewModel(this AnncAttEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncAttViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }
     
    public static class AnncViewModelExtensions
    {
        public static AnncViewModel ToViewModel(this AnnouncementEntity entity , bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }

    public static class AnncReviewViewModelExtensions
    {
        public static AnncReviewViewModel ToViewModel(this AnncReviewEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncReviewViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    } 

    public static class AnncWithTaskViewModelExtensions
    {
        public static AnncWithTaskViewModel ToViewModelWithTask(this AnnouncementEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncWithTaskViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }

    public static class AnncAlarmViewModelExtensions
    {
        public static AnncAlarmViewModel ToViewModel(this AnncAlarmEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncAlarmViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }

    public static class AnncAlarmRecViewModelExtensions
    {
        public static AnncAlarmRecViewModel ToViewModel(this AnncAlarmRecEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncAlarmRecViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }

    public static class AnncUpdateViewModelExtensions
    {
        public static AnncUpdateViewModel ToViewModel(this AnncUpdateEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncUpdateViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }


}

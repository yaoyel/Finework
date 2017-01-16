using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Common;
using FineWork.Colla;
using FineWork.Colla.Models;

namespace FineWork.Web.WebApi.Colla
{
    public class PlanAtViewModel
    {
        public Guid PlanAtId { get; set; }

        public bool? IsChecked { get; set; } 

        public PlanViewModel Plan { get; set; }
        public virtual void AssignFrom(PlanAtEntity entity)
        {
            Args.NotNull(entity, nameof(entity));
            PlanAtId = entity.Id;
            IsChecked = entity.IsChecked;
            Plan = entity.Plan.ToViewModel();
        }
    }

    public class PlanViewModel
    {
        public Guid PlanId { get; set; }

        public string Content { get; set; }

        public PlanType Type { get; set; }

        public DateTime? StartAt { get; set; }

        public DateTime? EndAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public string MonthOrYear { get; set; }

        public int Stars { get; set; }

        public bool IsPrivate { get; set; } 

        public bool ExecFrPartaker { get; set; }

        public StaffViewModel Creator { get; set; }

        public AnncStatus Status { get; set; }

        public virtual void AssignFrom(PlanEntity entity)
        {
            Args.NotNull(entity, nameof(entity));
            PlanId = entity.Id;
            Content = entity.Content;
            Type = entity.Type;
            StartAt = entity.StartAt;
            EndAt = entity.EndAt;
            CreatedAt = entity.CreatedAt;
            MonthOrYear = entity.MonthOrYear;
            Stars = entity.Stars;
            IsPrivate = entity.IsPrivate;
            ExecFrPartaker = entity.ExecFrPartaker;
            Creator = entity.Creator.ToViewModel(true, false);
        }


        public virtual void AssignFrom(AnnouncementEntity entity)
        {
            Args.NotNull(entity, nameof(entity));

            PlanId = entity.Id;
            Content = entity.Content;
            Type = entity.Type;
            StartAt = entity.StartAt;
            EndAt = entity.EndAt;
            CreatedAt = entity.CreatedAt;
            MonthOrYear = entity.MonthOrYear;
            Stars = entity.Statrs;
            IsPrivate = entity.IsPrivate;
            Creator = entity.Creator.ToViewModel(true, false);
            Status = entity.Reviews.Any()
                ? entity.Reviews.OrderByDescending(p => p.CreatedAt).First().Reviewstatus
                : AnncStatus.Unspecified;
        }
    }

    public class PlanDetailViewModel : PlanViewModel
    {   
        public StaffViewModel Inspecter { get; set; }

        public List<StaffViewModel> Executors { get; set; }

        public List<PlanAlarmViewModel> Alarms { get; set; } 

        public List<AnncUpdateViewModel> Updates { get; set; }

        public List<AnncReviewViewModel> Reviews { get; set; }

        public bool IsNeedAchv { get; set; }

        public List<AnncAttViewModel> Atts { get; set; }

        public override void AssignFrom(PlanEntity entity)
        {
            Args.NotNull(entity, nameof(entity));
            base.AssignFrom(entity);
         
            Inspecter = entity.Inspecter?.ToViewModel(true);
            Executors = entity.Executors?.Select(p => p.Staff.ToViewModel(true)).ToList();
            Alarms = entity.Alarms.Select(p => p.ToViewModel()).ToList();
        }


        public override void AssignFrom(AnnouncementEntity entity)
        {
            Args.NotNull(entity, nameof(entity)); 
            base.AssignFrom(entity);

            Inspecter = entity.Inspecter?.ToViewModel(true);
            Executors = entity.Executors?.Select(p => p.Staff.ToViewModel(true)).ToList();
            Alarms = entity.Alarms.Select(p => p.ToViewModel()).ToList();
            Updates = entity.Updates.Select(p => p.ToViewModel()).ToList();
            Reviews = entity.Reviews.Select(p => p.ToViewModel()).ToList();
            IsNeedAchv = entity.IsNeedAchv;
            Atts = entity.Atts.Select(p => p.ToViewModel()).ToList();
        }
    }

    public class PlanAlarmViewModel
    {
        public Guid PlanAlarmId { get; set; }

        public DateTime? Time { get; set; }

        public string Bell { get; set; }

        public bool IsEnabled { get; set; }

        public string BeforeStart { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(PlanAlarmEnitty entity)
        {
            Args.NotNull(entity, nameof(entity));
            PlanAlarmId = entity.Id;
            Time = entity.Time;
            BeforeStart = entity.BeforeStart.HasValue ? AlarmTimeConverter.BackConverter(entity.BeforeStart.Value) : "";
            Bell = entity.Bell;
            CreatedAt = entity.CreatedAt;
            IsEnabled = entity.IsEnabled;
        }

        public virtual void AssignFrom(AnncAlarmEntity entity)
        {
            Args.NotNull(entity, nameof(entity));

            PlanAlarmId = entity.Id;
            Time = entity.Time;
            BeforeStart=entity.BeforeStart.HasValue? AlarmTimeConverter.BackConverter(entity.BeforeStart.Value) : "";
            Bell = entity.Bell;
            CreatedAt = entity.CreatedAt;
            IsEnabled = entity.IsEnabled;
        }


    }

    public static class PlanViewModelExtensions
    {
        public static PlanViewModel ToViewModel(this PlanEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static PlanViewModel ToPlanViewModel(this AnnouncementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }

    public static class PlanAtViewModelExtensions
    {
        public static PlanAtViewModel ToViewModel(this PlanAtEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanAtViewModel();
            result.AssignFrom(entity);
            return result;
        } 
    }

    public static class PlanAlarmViewModelExtensions
    {
        public static PlanAlarmViewModel ToViewModel(this PlanAlarmEnitty entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanAlarmViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static PlanAlarmViewModel ToViewModel(this AnncAlarmEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanAlarmViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }

    public static class PlanDetailViewModelExtensions
    {
        public static PlanDetailViewModel ToDetailViewModel(this PlanEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanDetailViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static PlanDetailViewModel ToDetailViewModel(this AnnouncementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PlanDetailViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }
}
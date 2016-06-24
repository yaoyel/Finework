using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class AnncViewModel
    {
        public Guid AnncId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime EndAt { get; set; }

        public bool IsNeedAchv { get; set; }

        public ReviewStatuses ReviewStatus { get; set; }

        public List<AnncIncentiveViewModel> Incentives { get; set; }

        public List<AnncAttViewModel> Atts { get; set; }

        public StaffViewModel Staff { get; set; }

        public virtual void AssignFrom(AnnouncementEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            this.AnncId = source.Id;
            this.Content = source.Content;
            this.CreatedAt = source.CreatedAt;
            this.EndAt = source.EndAt;
            this.IsNeedAchv = source.IsNeedAchv;
            this.ReviewStatus = source.ReviewStatus;
            this.Incentives = source.AnncIncentives.Select(p => p.ToViewModel()).ToList();
            this.Atts = source.Atts.Select(p => p.ToViewModel(false, false)).ToList();
            this.Staff = source.Staff.ToViewModel(true, false);

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
            TaskSharing = source.TaskSharing.ToViewModel();
            IsAchv = source.IsAchv;
        }
    }


    public class AnncIncentiveViewModel 
    {
        public IncentiveKindViewModel Kind { get; set; }

        public decimal Amount { get; set; }

        public virtual void AssignFrom(AnncIncentiveEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.Kind = source.IncentiveKind.ToViewModel();
            this.Amount = source.Amount;
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

    public static class AnncIncentiveViewModelExtensions
    {
        public static AnncIncentiveViewModel ToViewModel(this AnncIncentiveEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncIncentiveViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }

    public static class AnncViewModelExtensions
    {
        public static AnncViewModel ToViewModel(this AnnouncementEntity entity, bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncViewModel();
            result.AssignFrom(entity, isShowhighOnly, isShowLow);
            return result;
        }
    }
}

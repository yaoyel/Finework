using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
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
          
        public List<AnncIncentiveViewModel> Incentives { get; set; }

        public List<AnncReviewViewModel> Reviews { get; set; }

        public List<AnncAttViewModel> Atts { get; set; }

        public StaffViewModel Staff { get; set; }

        public virtual void AssignFrom(AnnouncementEntity source, List<IncentiveKindViewModel> incentivekinds, bool isShowhighOnly = false, bool isShowLow = true)
        { 
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (incentivekinds == null) throw new ArgumentNullException(nameof(incentivekinds));

            var setIncentiveKinds = source.AnncIncentives.Select(p => p.IncentiveKind.Id).ToArray();
            var incentiveKinds = incentivekinds.Where(p=>!setIncentiveKinds.Contains(p.Id));

            var annIncentives = new List<AnncIncentiveViewModel>();

            foreach (var kind in incentiveKinds)
            {
                annIncentives.Add(new AnncIncentiveViewModel() {Amount = 0,Kind=kind});
            }

            this.AnncId = source.Id;
            this.Content = source.Content;
            this.CreatedAt = source.CreatedAt;
            this.EndAt = source.EndAt;
            this.IsNeedAchv = source.IsNeedAchv;
            var anncReviews = source.Reviews.OrderByDescending(p => p.CreatedAt);
           
            if(!anncReviews.Any())
                Reviews=new List<AnncReviewViewModel>()
                {
                    new AnncReviewViewModel()
                    {
                        CreatedAt = source.CreatedAt,
                        Status =ReviewStatuses.Unspecified 
                    }
                };
            else
            {
                Reviews = anncReviews.Select(p => p.ToViewModel()).OrderByDescending(p=>p.CreatedAt).ToList();
            }

            this.Incentives = source.AnncIncentives.Select(p => p.ToViewModel()).ToList().Union(annIncentives).ToList();
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
            TaskSharing = source.TaskSharing.ToViewModel(false,false);
            IsAchv = source.IsAchv;
        }
    }

    public class AnncReviewViewModel
    {
        public ReviewStatuses Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(AnncReviewEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.Status = source.Reviewstatus;
            this.CreatedAt = source.CreatedAt; 
        }
    } 

    public class AnncIncentiveViewModel 
    {
        public IncentiveKindViewModel Kind { get; set; }

        public decimal Amount { get; set; }

        public decimal Grant { get; set; }

        public virtual void AssignFrom(AnncIncentiveEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.Kind = source.IncentiveKind.ToViewModel();
            this.Amount = source.Amount;
            this.Grant = source.Grant ?? Amount;
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
        public static AnncViewModel ToViewModel(this AnnouncementEntity entity,List<IncentiveKindViewModel> incentivekinds , bool isShowhighOnly = false,
            bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AnncViewModel();
            result.AssignFrom(entity, incentivekinds, isShowhighOnly, isShowLow);
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
}

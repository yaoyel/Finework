using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{  
    public class VoteOptionViewModel
    {
        public Guid OptionId { get; set; } 
       
        public string Content { get; set; }

        public bool IsNeedReason { get; set; }

        public ICollection<VotingViewModel> Votings { get; set; }

        /// <summary>
        /// 匿名情况下，不允许查看选票详情，只显示投票数
        /// </summary>
        public int VotingNumber { get; set; } 

        public virtual void AssignFrom(VoteOptionEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.OptionId = entity.Id;
            this.Content = entity.Content;
            this.IsNeedReason = entity.IsNeedReason;
            if (!entity.Vote.IsAnonEnabled)
                this.Votings = entity.Votings.Select(p=>p.ToViewModel()).ToList();
            VotingNumber = entity.Votings.Count();
        }
    } 

    public class VoteViewModel  
    {
        public Guid Id { get; set; }

        public string Subject { get; set; }

        public Guid TaskId { get; set; }

        public StaffViewModel Createor{ get; set; } 

        public DateTime CreatedAt { get; set; }

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public bool IsMultiEnabled { get; set; }

        public bool IsAnonEnabled { get; set; } 

        public IEnumerable<VoteOptionViewModel> VoteOptions { get; set; }

        public virtual void AssignFrom(VoteEntity entity)
        { 
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Subject = entity.Subject;
            this.TaskId = entity.Task.Id;
            this.Createor = entity.Creator.ToViewModel();
            this.StartAt = entity.StartAt;
            this.EndAt = entity.EndAt;
            this.CreatedAt = entity.CreatedAt;
            this.IsAnonEnabled = entity.IsAnonEnabled;
            this.IsMultiEnabled = entity.IsMultiEnabled;
            this.VoteOptions = entity.VoteOptions.Select(p => p.ToViewModel());
        }
    } 

    public class VotingViewModel
    { 
        public Guid Id { get; set; }

        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; }

        public Guid VoteOptionId { get; set; }

        public StaffViewModel Staff { get; set; }

        public virtual void AssignFrom(VotingEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.Reason = entity.Reason;
            this.VoteOptionId = entity.Option.Id;
            this.Staff = entity.Staff.ToViewModel(); 
            this.CreatedAt = entity.CreatedAt; 
        }
    }  

    public static class VoteOptionViewModelExtensions
    {
        public static VoteOptionViewModel ToViewModel(this VoteOptionEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new VoteOptionViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }

    public static class VoteViewModelExtensions
    {
        public static VoteViewModel ToViewModel(this VoteEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new VoteViewModel();
            result.AssignFrom(entity);
            return result;
        }
    } 

    public static class VotingViewModelExtensions
    {
        public static VotingViewModel ToViewModel(this VotingEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new VotingViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }

}

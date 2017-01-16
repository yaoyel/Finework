using System;
using FineWork.Colla;
using System.Collections.Generic; 
using System.Linq;
using FineWork.Core;
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApi.Colla
{
    public class SimplePartakerInvViewModel
    {
        public Guid Id { get; set; }

        public PartakerKinds PartakerKind { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual void AssignFrom(PartakerInvEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Id = entity.Id;
            this.PartakerKind = entity.PartakerKind;
            this.CreatedAt = entity.CreatedAt;
        }
    }

    public class PartakerInvViewModel:SimplePartakerInvViewModel
    {    
        public TaskViewModel Task { get; set; }

        public StaffViewModel Staff { get; set; } 

        public String InviterNames { get; set; }

        public String Message { get; set; } 

        public ReviewStatuses ReviewStatus { get; set; }

        public DateTime ReviewAt { get; set; }

        public PartakerKinds? ReviewKind { get; set; }

        public override void AssignFrom(PartakerInvEntity entity)
        { 
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var inviterIdsToArray = Array.ConvertAll(entity.InviterStaffIds.Split(','),Guid.Parse);
            var staffManager = (IStaffManager) HttpUtil.HttpContext.RequestServices.GetService(typeof (IStaffManager));
            var inviterNames= staffManager.FetchStaffsByIds(inviterIdsToArray)
                .Select(p => p.Name);

            base.AssignFrom(entity); 
            this.Task = entity.Task.ToViewModel();
            this.Staff = entity.Staff.ToViewModel(); 
            this.InviterNames = string.Join(",", inviterNames);
            this.Message = entity.Message; 
            this.ReviewStatus = entity.ReviewStatus;
            this.ReviewAt = entity.ReviewAt;
            this.ReviewKind = entity.Task.Partakers.FirstOrDefault(p => p.Staff == entity.Staff)?.Kind; 
        } 
    }

    public static class PartakerInvViewModelExtensions
    {
        public static PartakerInvViewModel ToViewModel(this PartakerInvEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity)); 
            var result = new PartakerInvViewModel();
            result.AssignFrom(entity);
            return result;
        }

        public static SimplePartakerInvViewModel ToSimpleViewModel(this PartakerInvEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new SimplePartakerInvViewModel();
            result.AssignFrom(entity);
            return result;
        }

    }
}
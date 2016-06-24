using System;
using System.Collections.Generic;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;

namespace FineWork.Web.WebApi.Colla
{ 
    public class PartakerViewModel
    {  
        [Necessity]
        public PartakerKinds Kind { get; set; }

        public Guid Id { get; set; }

        [Necessity]
        public StaffViewModel Staff { get; set; }

        [Necessity(NecessityLevel.Low)]
        public TaskViewModel Task { get; set; } 

        public DateTime CreatedAt { get; set; }

        public  virtual void AssignFrom(PartakerEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {

            if (entity == null) throw new ArgumentNullException(nameof(entity)); 


            var propertiesDic = new Dictionary<string, Func<PartakerEntity, dynamic>>
            {
                ["Kind"] = (t) => t.Kind,
                ["Id"] = (t) => t.Id,
                ["Task"] = (t) => t.Task.ToViewModel(),
                ["Staff"] = (t) => t.Staff.ToViewModel(isShowhighOnly,isShowLow),
                ["Kind"] = (t) => t.Kind,
                ["CreatedAt"] = (t) => t.CreatedAt
            };

            NecessityAttributeUitl<PartakerViewModel, PartakerEntity>.SetVuleByNecssityAttribute(this, entity, propertiesDic, isShowhighOnly,
                isShowLow);
        } 
    }
     

    public static class PartakerViewModelExtensions
    {
        public static PartakerViewModel ToViewModel(this PartakerEntity entity,bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var result = new PartakerViewModel();
            result.AssignFrom(entity,isShowhighOnly,isShowLow);
            return result;
        } 

    }
}
using System;
using System.Collections.Generic;
using FineWork.Colla;
using FineWork.Web.WebApi.Common;

namespace FineWork.Web.WebApi.Colla
{
    public class StaffViewModel
    {
        [Necessity] 
        public String Name { get; set; }

        [Necessity]
        public Guid AccountId { get; set; }

        [Necessity]
        public string SecurityStamp { get; set; }

        [Necessity]
        public Guid Id { get; set; }

        public Guid OrgId { get; set; }

        public bool IsEnabled { get; set; }

        public string Department { get; set; }

        public bool HasUnReadMomnet { get; set; }

        public virtual void AssignFrom(StaffEntity source, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var propertiesDic = new Dictionary<string, Func<StaffEntity, dynamic>>
            {
                ["Name"] = (t) => t.Name,
                ["AccountId"] = (t) => t.Account.Id,
                ["SecurityStamp"]=(t)=>t.Account.SecurityStamp,
                ["Id"] = (t) => t.Id,
                ["IsEnabled"] = (t) => t.IsEnabled,
                ["OrgId"] = (t) => t.Org.Id,
                ["Department"] = (t) => t.Department
            };

            NecessityAttributeUitl<StaffViewModel, StaffEntity>.SetVuleByNecssityAttribute(this, source, propertiesDic, isShowhighOnly,
                isShowLow);
        }
    }

    public static class StaffViewModelExtensions
    {
        public static StaffViewModel ToViewModel(this StaffEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new StaffViewModel();
            result.AssignFrom(entity,isShowhighOnly,isShowLow);
            return result;
        } 
    }
}
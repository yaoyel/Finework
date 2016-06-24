using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Colla;

namespace FineWork.Web.WebApp.Models
{
    public class StaffViewModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public Guid AccountId { get; set; }

        public Guid OrgId { get; set; }

        public StaffViewModel AssignFrom(StaffEntity source)
        {
            Args.NotNull(source, nameof(source));

            this.Id = source.Id;
            this.Name = source.Name;
            this.AccountId = source.Account.Id;
            this.OrgId = source.Org.Id;

            return this;
        }
    }

    public static class StaffViewModelExtensions
    {
        public static StaffViewModel ToStaffViewModel(this StaffEntity entity)
        {
            Args.NotNull(entity, nameof(entity));
            return new StaffViewModel().AssignFrom(entity);
        }
    }
}

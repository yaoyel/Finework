using System;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class StaffViewModel
    {
        public Guid Id { get; set; }

        public String Name { get; set; }

        public Guid AccountId { get; set; }

        public Guid OrgId { get; set; }

        public bool IsEnabled { get; set; }

        public string Department { get; set; }


        public virtual void AssignFrom(StaffEntity source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            this.Id = source.Id;
            this.IsEnabled = source.IsEnabled;
            this.Name = source.Name;
            this.AccountId = source.Account.Id;
            this.OrgId = source.Org.Id;
            this.Department = source.Department;
        }
    }

    public static class StaffViewModelExtensions
    {
        public static StaffViewModel ToViewModel(this StaffEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new StaffViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}
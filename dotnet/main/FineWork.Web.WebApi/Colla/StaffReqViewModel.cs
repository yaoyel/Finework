using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class StaffReqViewModel
    {
        public Guid StaffReqId { get; set; }

        public Guid OrgId { get; set; }

        public AccountViewModel Account { get; set; }

        public string Message { get; set; }

        public short ReviewStatus { get; set; }

        public DateTime CreateAt { get; set; }

        public virtual void AssignFrom(StaffReqEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            StaffReqId = entity.Id;
            OrgId = entity.Org.Id;
            Account = entity.Account.ToViewModel();
            Message = entity.Message;
            ReviewStatus = (short)entity.ReviewStatus;
            CreateAt = entity.CreatedAt;
        }

    }
    public static class StaffReqViewModelExtensions
    {
        public static StaffReqViewModel ToViewModel(this StaffReqEntity entity)
        {
            var result = new StaffReqViewModel();
            result.AssignFrom(entity);
            return result; 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskAnnouncementViewModel
    {
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public StaffViewModel Account { get; set; }

        public virtual void AssignFrom(TaskAnnouncementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt;
            this.Account = entity.Staff.ToViewModel(true,false);
        }
    }

    public static class TaskAnnouncementExtensions
    {
        public static TaskAnnouncementViewModel ToViewModel(this TaskAnnouncementEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskAnnouncementViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

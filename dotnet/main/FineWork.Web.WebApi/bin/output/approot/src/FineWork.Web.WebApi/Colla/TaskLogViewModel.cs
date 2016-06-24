using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskLogViewModel
    {
        public Guid Id { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public AccountViewModel Account { get; set; } 

        public virtual void AssignFrom(TaskLogEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt;
            this.Account = entity.Staff.Account.ToViewModel(); 
        }
    }

    public static class TaskLogViewModelExtensions
    {
        public static TaskLogViewModel ToViewModel(this TaskLogEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskLogViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskNoteViewModel
    {
        public Guid NoteId { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public StaffViewModel Account { get; set; }

        public virtual void AssignFrom(TaskNoteEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.NoteId = entity.Id;
            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt;
            this.Account = entity.Staff.ToViewModel(true,false);
        }
    }

    public static class TaskNoteViewModelExtensions
    {
        public static TaskNoteViewModel ToViewModel(this TaskNoteEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new TaskNoteViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

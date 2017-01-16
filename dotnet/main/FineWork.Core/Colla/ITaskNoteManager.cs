using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface ITaskNoteManager
    {
        TaskNoteEntity CreateTaskNote(CreateTaskNoteModel taskNoteModel);

        IEnumerable<TaskNoteEntity> FetchTaskNotesByTaskId(Guid taskId );

        IEnumerable<TaskNoteEntity> FetchTaskNotesByStaffId(Guid staffId );

        TaskNoteEntity FindTaskNoteById(Guid noteId);

        void DeleteTaskNote(TaskNoteEntity taskNote);

        void UpdateTaskNote(TaskNoteEntity taskNote);
    }
}
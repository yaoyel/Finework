using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class TaskNoteExistsResult : FineWorkCheckResult
    {
        public TaskNoteExistsResult(bool isSucceed, string message, TaskNoteEntity taskNote)
            : base(isSucceed, message)
        {
            this.TaskNote = taskNote;
        }

        public TaskNoteEntity TaskNote { get; private set; }

        public static TaskNoteExistsResult Check(ITaskNoteManager taskNoteManager, Guid taskNoteId)
        {
            if (taskNoteManager == null) throw new ArgumentException(nameof(taskNoteManager));

            var taskNote = taskNoteManager.FindTaskNoteById(taskNoteId);
            return Check(taskNote, "任务不存在对应的纪要.");
        }

        private static TaskNoteExistsResult Check(TaskNoteEntity taskNote, string message)
        {
            if (taskNote == null)
                return new TaskNoteExistsResult(false, message, null);
            return new TaskNoteExistsResult(true, null, taskNote);
        }
    }
}

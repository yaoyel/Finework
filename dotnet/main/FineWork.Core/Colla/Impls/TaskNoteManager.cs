using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public class TaskNoteManager : AefEntityManager<TaskNoteEntity, Guid>, ITaskNoteManager
    {
        public TaskNoteManager(ISessionProvider<AefSession> dbContextProvider,
            ITaskManager taskManager, IPartakerManager partakerManager,
            ITaskLogManager taskLogManager, IStaffManager staffManager,
            IIMService imservice)
            : base(dbContextProvider)
        {
            if (dbContextProvider == null) throw new ArgumentException(nameof(dbContextProvider));
            if (taskManager == null) throw new ArgumentException(nameof(taskManager));
            if (partakerManager == null) throw new ArgumentException(nameof(partakerManager));
            if (taskLogManager == null) throw new ArgumentException(nameof(taskLogManager));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));

            m_TaskManager = taskManager;
            m_PartakerManager = partakerManager;
            m_TaskLogManager = taskLogManager;
            m_StaffManager = staffManager;
            m_IMService = imservice;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IStaffManager m_StaffManager; 
        private readonly IIMService m_IMService;

        public TaskNoteEntity CreateTaskNote(CreateTaskNoteModel taskNoteModel)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskNoteModel.TaskId).ThrowIfFailed().Task;
            var staff =
                StaffExistsResult.Check(this.m_StaffManager, taskNoteModel.StaffId).ThrowIfFailed().Staff;

            var partaker =
                AccountIsPartakerResult.Check(task, staff.Account.Id).ThrowIfFailed().Partaker;

            var taskNote=new TaskNoteEntity();
            taskNote.Id = Guid.NewGuid();
            taskNote.Staff = partaker.Staff;
            taskNote.Task = task;
            taskNote.Message = taskNoteModel.Message;
            taskNote.CreatedAt = taskNoteModel.CreatedAt;
            this.InternalInsert(taskNote);

            //记入日志  
            m_TaskLogManager.CreateTaskLog(task.Id, taskNoteModel.StaffId, taskNote.GetType().FullName,
                taskNote.Id, ActionKinds.InsertTable, $"创建了一个纪要");


            //发送群通知
            var imMessage = string.Format($"{staff.Name}创建了一个纪要", staff.Name);
            m_IMService.SendTextMessageByConversationAsync(task.Id, staff.Account.Id, task.Conversation.Id, task.Name, imMessage); 

            return taskNote;
        }


        public IEnumerable<TaskNoteEntity> FetchTaskNotesByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId);
             
        }

        public IEnumerable<TaskNoteEntity> FetchTaskNotesByStaffId(Guid staffId)
        {
            return   this.InternalFetch(p => p.Staff.Id == staffId); 
        }

        public TaskNoteEntity FindTaskNoteById(Guid noteId)
        {
            return this.InternalFind(noteId);
        }

        public void DeleteTaskNote(TaskNoteEntity taskNote)
        {
            if(taskNote==null) return;
            this.InternalDelete(taskNote);
        }

        public void UpdateTaskNote(TaskNoteEntity taskNote)
        {
            Args.NotNull(taskNote, nameof(taskNote));

            this.InternalUpdate(taskNote);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Files;
using System.Data.Entity;
using System.Security.Cryptography;
using AppBoot.Security.Crypto;
using FineWork.Common;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class TaskSharingManager : AefEntityManager<TaskSharingEntity, Guid>, ITaskSharingManager
    {
        public TaskSharingManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IStaffManager staffManager,
            IFileManager fileManager,
            IIMService imService,
            ITaskLogManager taskLogManager,
            IConfiguration config)
            : base(sessionProvider)
        {
            Args.NotNull(sessionProvider, nameof(sessionProvider));
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(taskLogManager, nameof(taskLogManager));
            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_FileMamager = fileManager;
            m_IMService = imService;
            m_TaskLogManager = taskLogManager;
            m_Config = config;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IFileManager m_FileMamager;
        private readonly IIMService m_IMService;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IConfiguration m_Config;

        private string GetTaskSharingDirectory(TaskSharingEntity taskSharing)
        {
            ////同一个人可以多次上传同一个文件，路径中加入时间的ticks区别
            //return $"tasks/{taskSharing.Task.Id}/sharings/{taskSharing.Staff.Id}-{taskSharing.CreatedAt.Ticks}/{taskSharing.FileName}";
            //用content-md5作为路径值，方便实现秒传
            return $"tasks/sharings/{taskSharing.ContentMd5}";
        }

        public TaskSharingEntity CreateTaskSharing(Guid taskId, Guid staffId, string fileName, string contentType,
            Stream fileStream)
        {
            Args.NotEmpty(fileName, nameof(fileName));
            Args.NotNull(fileStream, nameof(fileStream));
            var task = TaskExistsResult.Check(this.m_TaskManager, taskId).ThrowIfFailed().Task;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;

            var taskSharing = new TaskSharingEntity();
            taskSharing.Id = Guid.NewGuid();
            taskSharing.Staff = staff;
            taskSharing.Task = task;
            taskSharing.FileName = fileName;
            taskSharing.ContentMd5 = MD5.Create().CalculateHash(fileStream);
            taskSharing.ContentType = contentType;
            taskSharing.Size = fileStream.Length;
            this.InternalInsert(taskSharing);

            UploadTaskSharing(taskSharing, contentType, fileStream);

            var message = $"创建了一个共享";

            var imMesasge =  string.Format(m_Config["LeanCloud:Messages:Task:Sharing"], staff.Name);

            //发送群通知
            m_IMService.SendTextMessageByConversationAsync(task.Id,staff.Account.Id,task.ConversationId,task.Name, imMesasge);

            //记入日志
            m_TaskLogManager.CreateTaskLog(task.Id, staff.Id, taskSharing.GetType().FullName, taskSharing.Id, ActionKinds.InsertTable,message);
            return taskSharing;
        }

        public IEnumerable<TaskSharingEntity> FetchTaskSharingsByTask(Guid taskId)
        {
            var taskSharings = this.InternalFetch(p => p.Where(w => w.Task.Id == taskId).Include(s => s.Staff));
            return taskSharings;
        }

        public void DeleteTaskSharing(Guid taskSharingId)
        {
            var taskSharing = TaskSharingExistsResult.Check(this, taskSharingId).TaskSharing;
            if (taskSharing == null) return;

            InternalDelete(taskSharing);
            if (!TaskSharingExistsResult.Check(this, taskSharing.ContentMd5).IsSucceed)
                m_FileMamager.DeleteFile(GetTaskSharingDirectory(taskSharing));
        }

        private void UploadTaskSharing(TaskSharingEntity taskSharing, string contentType, Stream stream)
        {
            try
            {
                if (m_FileMamager.FileIsExists(GetTaskSharingDirectory(taskSharing))) return;
                this.m_FileMamager.CreateFile(GetTaskSharingDirectory(taskSharing), contentType, stream);
            }
            catch
            {
                InternalDelete(taskSharing);
            }

        }

        public TaskSharingEntity FindTaskSharing(Guid taskSharingId)
        {
            return this.InternalFind(taskSharingId);
        }

        public IEnumerable<TaskSharingEntity> FetchTaskSharingByContentMd5(string contentMd5)
        {
            return InternalFetch(p => p.ContentMd5 == contentMd5);
        }

        public void DownloadTaskSharing(TaskSharingEntity taskSharing, Stream stream)
        {
            Args.NotNull(taskSharing, nameof(taskSharing));

            var pathName = GetTaskSharingDirectory(taskSharing);
            if (!m_FileMamager.FileIsExists(pathName))
                throw new FineWorkException("文件不存在。");

            m_FileMamager.DownloadToStream(pathName, stream);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public class TaskAnnouncementManager : AefEntityManager<TaskAnnouncementEntity, Guid>, ITaskAnnouncementManager
    {
        public TaskAnnouncementManager(ISessionProvider<AefSession> dbContextProvider,
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

        public TaskAnnouncementEntity CreateTaskAnnouncement(CreateTaskAnnouncementModel taskAnnouncementModel)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskAnnouncementModel.TaskId).ThrowIfFailed().Task;
            var staff =
                StaffExistsResult.Check(this.m_StaffManager, taskAnnouncementModel.StaffId).ThrowIfFailed().Staff;

            var partaker =
                AccountIsPartakerResult.Check(task, staff.Account.Id).ThrowIfFailed().Partaker;

            var taskAnnouncement = new TaskAnnouncementEntity();
            taskAnnouncement.Id = Guid.NewGuid();
            taskAnnouncement.IsGoodNews = taskAnnouncementModel.IsGoodNews;
            taskAnnouncement.Staff = partaker.Staff;
            taskAnnouncement.Task = task;
            taskAnnouncement.Message = taskAnnouncementModel.Message;
            taskAnnouncement.AnnounceKind = taskAnnouncementModel.AnnouncementKind;
            this.InternalInsert(taskAnnouncement);

            //记入日志
            var announcementKind = taskAnnouncement.AnnounceKind.GetLabel();

            m_TaskLogManager.CreateTaskLog(task.Id, taskAnnouncementModel.StaffId, taskAnnouncement.GetType().FullName,
                taskAnnouncement.Id, ActionKinds.InsertTable, $"创建了一个{announcementKind}");


            //发送群通知
            var imMessage = string.Format($"{staff.Name}创建了一个承诺", staff.Name);
            m_IMService.SendTextMessageByConversationAsync(task.Id, staff.Account.Id, task.ConversationId, task.Name, imMessage); 

            return taskAnnouncement;
        }


        public IEnumerable<TaskAnnouncementEntity> FetchTaskAnnouncementByTaskId(Guid taskId, bool? isGoodNews,
            AnnouncementKinds announcementKind = AnnouncementKinds.All)
        {
            var taskAnnouncements = this.InternalFetch(p => p.Task.Id == taskId);

            return FilterAnnouncements(taskAnnouncements, isGoodNews, announcementKind);
        }

        public IEnumerable<TaskAnnouncementEntity> FetchTaskAnnouncementByStaffId(Guid staffId, bool? isGoodNews,
            AnnouncementKinds announcementKind = AnnouncementKinds.All)
        {
            var taskAnnouncements = this.InternalFetch(p => p.Staff.Id == staffId);

            return FilterAnnouncements(taskAnnouncements, isGoodNews, announcementKind);
        }

        private IEnumerable<TaskAnnouncementEntity> FilterAnnouncements(IEnumerable<TaskAnnouncementEntity> source,
            bool? isGoodNews, AnnouncementKinds kind)
        {
            if (isGoodNews != null)
                source = source.Where(p => p.IsGoodNews == isGoodNews).ToList();
            if (kind != AnnouncementKinds.All)
                source = source.Where(p => p.AnnounceKind == kind).ToList();

            return source.OrderBy(p => p.CreatedAt);
        }
    }
}

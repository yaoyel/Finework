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
using FineWork.Common;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class TaskNewsManager : AefEntityManager<TaskNewsEntity, Guid>, ITaskNewsManager
    {
        public TaskNewsManager(ISessionProvider<AefSession> dbContextProvider,
            ITaskManager taskManager, IPartakerManager partakerManager,
            ITaskLogManager taskLogManager, IStaffManager staffManager,
            IIMService imservice, IConfiguration config,
            IAccessTimeManager accessTimeManager)
            : base(dbContextProvider)
        {
            if (dbContextProvider == null) throw new ArgumentException(nameof(dbContextProvider));
            if (taskManager == null) throw new ArgumentException(nameof(taskManager));
            if (partakerManager == null) throw new ArgumentException(nameof(partakerManager));
            if (taskLogManager == null) throw new ArgumentException(nameof(taskLogManager));
            if (staffManager == null) throw new ArgumentException(nameof(staffManager));
            if (imservice == null) throw new ArgumentException(nameof(imservice));
            if (accessTimeManager == null) throw new ArgumentException(nameof(accessTimeManager));

            m_TaskManager = taskManager;
            m_PartakerManager = partakerManager;
            m_TaskLogManager = taskLogManager;
            m_StaffManager = staffManager;
            m_IMService = imservice;
            m_Config = config;
            m_AccessTimeManager = accessTimeManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IIMService m_IMService;
        private readonly IConfiguration m_Config;
        private readonly IAccessTimeManager m_AccessTimeManager;


        public TaskNewsEntity CreateTaskNews(CreateTaskNewsModel taskNewsModel)
        {
            var task = TaskExistsResult.Check(this.m_TaskManager, taskNewsModel.TaskId).ThrowIfFailed().Task;
            var staff =
                StaffExistsResult.Check(this.m_StaffManager, taskNewsModel.StaffId).ThrowIfFailed().Staff;

            var partaker =
                AccountIsPartakerResult.Check(task, staff.Account.Id).ThrowIfFailed().Partaker;

            var taskNews= new TaskNewsEntity();
            taskNews.Id = Guid.NewGuid();
            taskNews.Staff = partaker.Staff;
            taskNews.Task = task;
            taskNews.Message = taskNewsModel.Message; 
            this.InternalInsert(taskNews);
             

            var message = $"创建了一个好消息";
            m_TaskLogManager.CreateTaskLog(task.Id, taskNewsModel.StaffId, taskNews.GetType().FullName,
                taskNews.Id, ActionKinds.InsertTable, message);

            //发送群通知
            var imMessage = string.Format(m_Config["LeanCloud:Messages:Task:News"], staff.Name);
            m_IMService.SendTextMessageByConversationAsync(task.Id,staff.Account.Id, task.ConversationId, task.Name, imMessage);
            return taskNews;
        }


        public IEnumerable<TaskNewsEntity> FetchTaskNewsesByTaskId(Guid taskId)
        {
            var taskNewses = this.InternalFetch(p => p.Task.Id == taskId);  
            return taskNewses;
        }

        public IEnumerable<TaskNewsEntity> FetchTaskNewsesByStaffId(Guid staffId)
        {

            var tasks = m_TaskManager.FetchTasksByStaffId(staffId).Select(p => p.Id);

            var taskNewses = this.InternalFetch(p => tasks.Contains(p.Task.Id));
            m_AccessTimeManager.UpdateLastViewNewsTime(staffId, DateTime.Now);

            return taskNewses;
        }

        public TaskNewsEntity FindTaskNewsById(Guid taskNewsId)
        {
            return this.InternalFind(taskNewsId);
        }

    

        public void DeleteTaskNewsById(Guid taskNewsId)
        {
            var taskNews = TaskNewsExistsResult.Check(this, taskNewsId).ThrowIfFailed().TaskNews;
            this.InternalDelete(taskNews);
        }


        public Tuple<int, IEnumerable<Guid>> FetchUnReadNewsesByStaffId(Guid staffId)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staffId).AccessTime;

            var news = this.InternalFetch(p => p.Task.Partakers.Any(a => a.Staff.Id == staffId));

            var item2 = accessTime?.LastViewNewsAt == null
                ? news
                : news.Where(p => p.CreatedAt >= accessTime.LastViewNewsAt.Value); 

            return new Tuple<int, IEnumerable<Guid>>(news.Count(),item2.Select(p=>p.Id));
        }

    }
}

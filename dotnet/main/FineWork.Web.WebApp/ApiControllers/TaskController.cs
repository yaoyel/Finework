using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Security;
using FineWork.Security.Repos.Aef;
using FineWork.Web.WebApp.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;

namespace FineWork.Web.WebApp.ApiControllers
{
    [Route("api/task")]
    public class TaskController : Controller
    {
        public TaskController(ISessionScopeFactory sessionScopeFactory, ITaskManager taskManager
            , IPartakerManager partakerManager, IAccountManager accountManager,
             IStaffManager staffManager)
        {
            if (sessionScopeFactory == null) throw new ArgumentException("sessionScopeFactory");
            if (taskManager == null) throw new ArgumentException("TaskManager");
            m_SessionScopeFactory = sessionScopeFactory;
            m_TaskManager = taskManager;
            m_PartakerManager = partakerManager;
            m_AccountManager = accountManager;
            m_StaffManager = staffManager;
        }

        private readonly ISessionScopeFactory m_SessionScopeFactory;
        private readonly ITaskManager m_TaskManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IAccountManager m_AccountManager;
        private readonly IStaffManager m_StaffManager;
        /// <summary>
        /// 获取个人所有任务
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        [Route("fetchbystaff")]
        public PersonalTaskViewModel FindTaskByStaff(Guid staffId)
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var partakers = m_PartakerManager.FetchPartakersByStaff(staffId).ToList();

                var model = new PersonalTaskViewModel()
                {
                    Leader = new List<TaskViewModel>(),
                    Collabrator = new List<TaskViewModel>(),
                    Recipient = new List<TaskViewModel>(),
                    Mentor = new List<TaskViewModel>()
                };

 
                var taskForLeader = partakers.Where(p => p.Kind == PartakerKinds.Leader)
                    .Select(p => new TaskViewModel()
                    {
                        Id = p.Task.Id,
                        Name = p.Task.Name,
                        Leader = m_PartakerManager.FetchPartakersByStaff(p.Task.Id)
                        .Where(k => k.Kind == PartakerKinds.Leader)
                        .Select(k => k.Staff)
                        .Select(k => k.Name).ToList()
                    });

                var taskForCollabrator = partakers.Where(p => p.Kind == PartakerKinds.Collaborator)
               .Select(p => new TaskViewModel()
               {
                   Id = p.Task.Id,
                   Name = p.Task.Name,
                   Leader = m_PartakerManager.FetchPartakersByStaff(p.Task.Id)
                   .Where(k => k.Kind == PartakerKinds.Leader)
                   .Select(k => k.Staff)
                   .Select(k => k.Name).ToList()
               });


                var taskForMentor = partakers.Where(p => p.Kind == PartakerKinds.Mentor)
               .Select(p => new TaskViewModel()
               {
                   Id = p.Task.Id,
                   Name = p.Task.Name,
                   Leader = m_PartakerManager.FetchPartakersByStaff(p.Task.Id)
                   .Where(k => k.Kind == PartakerKinds.Leader)
                   .Select(k => k.Staff)
                   .Select(k => k.Name).ToList()
               });

                model.Leader = taskForLeader.ToList();
                model.Collabrator = taskForCollabrator.ToList();
                model.Mentor = taskForMentor.ToList();
                return model;
            }
        }

        /// <summary>
        /// 获取该机构下所有任务
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [Route("fetchbyorg")]
        public IEnumerable<OrganizationTaskViewModel> FetchTasksByOrg(Guid orgId)
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var staffs = m_StaffManager.FetchStaffsByOrg(orgId).Select(p => p.Id);

                var tasks = m_TaskManager.FetchTasksByStaff(staffs.ToArray());

                var result = tasks.Select(s => new OrganizationTaskViewModel
                {
                    TaskName = s.Name,
                    TaskId = s.Id, 
                    Leader = m_PartakerManager.FetchPartakersByStaff(s.Id) 
                    .Where(p => p.Kind == PartakerKinds.Leader)
                    .Select(p => p.Staff.Account.Name).ToList()
                }).ToList();

                return result;

            }

        }

        [Route("findbyid")]
        public TaskViewModel FindTaskById(Guid taskId)
        {
            using (var sessionScope = m_SessionScopeFactory.CreateScope())
            {
                var task = m_TaskManager.FindTask(taskId);

                var temp = m_PartakerManager.FetchPartakersByStaff(task.Creator.Id)
                    .Join(m_AccountManager.FetchAccounts(),
                        p => p.Staff.Account.Id,
                        a => a.Id,
                        (p, a) => new
                        {
                            AccountName = a.Name,
                            AccountId = a.Id,
                            Kind = p.Kind,
                        }).ToList();

                var model = new TaskViewModel()
                {
                    Id = task.Id,
                    Name = task.Name,
                    Collabrator = new List<string>(),
                    Leader = new List<string>(),
                    Recipient = new List<string>(),
                    Mentor = new List<string>()
                };

                temp.ForEach(p =>
                {
 
                    if (p.Kind == PartakerKinds.Collaborator)
                        model.Collabrator.Add(p.AccountName);
                    if (p.Kind == PartakerKinds.Leader)
                        model.Leader.Add(p.AccountName);
                    if (p.Kind == PartakerKinds.Recipient) 
                        model.Recipient.Add(p.AccountName);
                    if(p.Kind==PartakerKinds.Mentor && !model.Mentor.Contains(p.AccountName))
                        model.Mentor.Add(p.AccountName);
                });
                return model;
            }
        }

    }
}

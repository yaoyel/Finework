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
using FineWork.Common;
using FineWork.Net.IM;
using Microsoft.Extensions.Configuration;

namespace FineWork.Colla.Impls
{
    public class AnnouncementManager: AefEntityManager<AnnouncementEntity, Guid>, IAnnouncementManager
    {
        public AnnouncementManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            IStaffManager staffManager, 
            IAnncIncentiveManager anncIncentiveManager,
            IAnncAttManager anncAttManager,
            ITaskSharingManager taskSharingManager,
            IIncentiveManager incentiveManager,
            IPartakerManager partakerManager,
            IIMService imService,
            ITaskLogManager taskLogManager,
            IConfiguration config
            ) : base(sessionProvider)
        {
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(anncIncentiveManager, nameof(anncIncentiveManager));
            Args.NotNull(anncAttManager, nameof(anncAttManager));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));
            Args.NotNull(incentiveManager, nameof(incentiveManager));
            Args.NotNull(partakerManager, nameof(partakerManager));
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(taskLogManager, nameof(taskLogManager));
            Args.NotNull(config, nameof(config));

            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_AnncIncentiveManager = anncIncentiveManager;
            m_AnncAttManager = anncAttManager;
            m_TaskSharingManager = taskSharingManager;
            m_IncentiveManager = incentiveManager;
            m_PartakerManager = partakerManager;
            m_ImService = imService;
            m_TaskLogManager = taskLogManager;
            m_Config = config;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IAnncIncentiveManager m_AnncIncentiveManager;
        private readonly IAnncAttManager m_AnncAttManager;
        private readonly ITaskSharingManager m_TaskSharingManager;
        private readonly IIncentiveManager m_IncentiveManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IIMService m_ImService;
        private readonly ITaskLogManager m_TaskLogManager;
        private readonly IConfiguration m_Config;
        public AnnouncementEntity CreateAnnc(CreateAnncModel createAnncModel)
        {
            var staff = StaffExistsResult.Check(m_StaffManager, createAnncModel.StaffId).ThrowIfFailed().Staff;
            var task = TaskExistsResult.Check(m_TaskManager, createAnncModel.TaskId).ThrowIfFailed().Task;

            var anncEntity=new AnnouncementEntity();
            anncEntity.Id = Guid.NewGuid();
            anncEntity.Staff = staff;
            anncEntity.Task = task;
            anncEntity.Content = createAnncModel.Content;
            anncEntity.IsNeedAchv = createAnncModel.IsNeedAchv; 
            anncEntity.EndAt = createAnncModel.EndAt;

            this.InternalInsert(anncEntity);
            //处理激励
            if (createAnncModel.Incentives!=null && createAnncModel.Incentives.Any())
                 foreach (var incentive in createAnncModel.Incentives)
                 {
                     if (incentive.Item2 > 0)
                         m_AnncIncentiveManager.CreateOrUpdateAnncIncentive(anncEntity.Id, incentive.Item1,
                             incentive.Item2);
                 }

            //处理资源
            if(createAnncModel.Atts!=null && createAnncModel.Atts.Any())
                foreach (var att in createAnncModel.Atts)
                {
                    m_AnncAttManager.CreateAnncAtt(anncEntity.Id, att, false);
                }

            var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
            var message=String.Format(m_Config["LeanCloud:Messages:Task:Annc"],leader.Staff.Name,staff.Name)  ;
            m_ImService.SendTextMessageByConversationAsync(task.Id, staff.Account.Id, task.ConversationId, task.Name,message);

            m_TaskLogManager.CreateTaskLog(task.Id, staff.Id, anncEntity.GetType().FullName, anncEntity.Id,
                ActionKinds.InsertTable, "创建了一个里程碑");
           
            return anncEntity;
        }

        public void UpdateAnnc(UpdateAnncModel updateAnncModel)
        {
            Args.NotNull(updateAnncModel, nameof(updateAnncModel));

            var staff = StaffExistsResult.Check(m_StaffManager, updateAnncModel.StaffId).ThrowIfFailed().Staff;
            var annc = AnncExistsResult.Check(this, updateAnncModel.Id).ThrowIfFailed().Annc;

            annc.Content = updateAnncModel.Content;
            annc.EndAt = updateAnncModel.EndAt;
            annc.IsNeedAchv = updateAnncModel.IsNeedAchv;
            annc.Staff = staff;

            if (updateAnncModel.Incentives != null && updateAnncModel.Incentives.Any())
            {
                foreach (var incentive in updateAnncModel.Incentives)
                {
                    this.m_AnncIncentiveManager.CreateOrUpdateAnncIncentive(annc.Id, incentive.Item1,
                        incentive.Item2);
                }
            } 
            this.m_AnncAttManager.UpdateAnncAtts(annc, updateAnncModel.Atts);

            this.InternalUpdate(annc); 

        }

        public AnnouncementEntity FindAnncById(Guid anncId)
        {
            return this.InternalFind(anncId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId).OrderBy(p=>p.EndAt);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncByStatus(Guid staffId,ReviewStatuses status=ReviewStatuses.Unspecified)
        {
            //获取staff参与的任务
            var taskIds = this.m_PartakerManager.FetchPartakersByStaff(staffId).Select(p => p.Task.Id).ToArray(); 
            
            return
                this.InternalFetch(
                    p =>p.EndAt<=DateTime.Now && taskIds.Contains(p.Task.Id) && !p.Reviews.Any());
        }

        public IEnumerable<AnnouncementEntity> FetchAnncByEndTime(DateTime endAt)
        {
            var endAtFormat =new DateTime(endAt.Year,endAt.Month,endAt.Day,endAt.Hour,endAt.Minute,0);
            return this.InternalFetch(p => p.EndAt == endAtFormat && !p.Reviews.Any());
        }

        public void DeleteAnnc(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this, anncId).Annc;
            if (annc != null)
            {
                this.m_AnncAttManager.DeleteAnncAttByAnncId(anncId,true);
                this.m_AnncAttManager.DeleteAnncAttByAnncId(anncId, false);
                this.m_AnncIncentiveManager.DeleteIncentiveByAnncId(anncId);
                this.InternalDelete(annc);
            }
        }
    }
}

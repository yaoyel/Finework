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
            IIncentiveManager incentiveManager
            ) : base(sessionProvider)
        {
            Args.NotNull(taskManager, nameof(taskManager));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(anncIncentiveManager, nameof(anncIncentiveManager));
            Args.NotNull(anncAttManager, nameof(anncAttManager));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));
            Args.NotNull(incentiveManager, nameof(incentiveManager));

            m_TaskManager = taskManager;
            m_StaffManager = staffManager;
            m_AnncIncentiveManager = anncIncentiveManager;
            m_AnncAttManager = anncAttManager;
            m_TaskSharingManager = taskSharingManager;
            m_IncentiveManager = incentiveManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly IStaffManager m_StaffManager;
        private readonly IAnncIncentiveManager m_AnncIncentiveManager;
        private readonly IAnncAttManager m_AnncAttManager;
        private readonly ITaskSharingManager m_TaskSharingManager;
        private readonly IIncentiveManager m_IncentiveManager;

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
            anncEntity.ReviewStatus=ReviewStatuses.Unspecified;
            anncEntity.EndAt = createAnncModel.EndAt;

            this.InternalInsert(anncEntity);
            //处理激励
            if (createAnncModel.Incentives!=null && createAnncModel.Incentives.Any())
                 foreach (var incentive in createAnncModel.Incentives)
                 { 
                     m_AnncIncentiveManager.CreateOrUpdateAnncIncentive(anncEntity.Id, incentive.Item1, incentive.Item2);
                 }

            //处理资源
            if(createAnncModel.Atts!=null && createAnncModel.Atts.Any())
                foreach (var att in createAnncModel.Atts)
                {
                    m_AnncAttManager.CreateAnncAtt(anncEntity.Id, att, false);
                }

           
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

            if (annc.Atts != null && annc.Atts.Any())
            {
              this.m_AnncAttManager.UpdateAnncAtts(annc,annc.Atts.Select(p=>p.TaskSharing.Id).ToArray());
            }
            this.InternalUpdate(annc);


        }

        public AnnouncementEntity FindAnncById(Guid anncId)
        {
            return this.InternalFind(anncId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByStaffId(Guid staffId)
        {
            return this.InternalFetch(p => p.Staff.Id == staffId);
        }

        public IEnumerable<AnnouncementEntity> FetchAnncsByStatus(ReviewStatuses reviewStatus)
        {
            return this.InternalFetch(p => p.ReviewStatus == reviewStatus);
        } 
        public void ChangeAnncStatus(AnnouncementEntity annc, ReviewStatuses reviewStatus)
        {
            Args.NotNull(annc, nameof(annc));

            if(annc.ReviewStatus==ReviewStatuses.Approved)
                throw new FineWorkException($"Invalid ReviewStatus {reviewStatus}.");

            annc.ReviewStatus = reviewStatus;

            this.InternalUpdate(annc);

            var leader = annc.Task.Partakers.First(p => p.Kind == PartakerKinds.Leader).Staff;
            //兑现激励
            if (reviewStatus == ReviewStatuses.Approved)
                foreach (var incentive in annc.AnncIncentives)
                { 
                    m_IncentiveManager.CreateIncentive(annc.Task.Id, incentive.IncentiveKind.Id, leader.Id,
                        annc.Staff.Id, incentive.Amount);
                }
            
        }

        public void DeleteAnnc(Guid anncId)
        {
            var annc = AnncExistsResult.Check(this, anncId).Annc;
            if (annc != null)
            {
                this.m_AnncAttManager.DeleteAnncAttByAnncId(anncId);
                this.m_AnncIncentiveManager.DeleteIncentiveByAnncId(anncId);
                this.InternalDelete(annc);
            }
        }
    }
}

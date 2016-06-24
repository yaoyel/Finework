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
using FineWork.Common;

namespace FineWork.Colla.Impls
{
    public class AnncIncentiveManager: AefEntityManager<AnncIncentiveEntity, Guid>, IAnncIncentiveManager
    {
        public AnncIncentiveManager(ISessionProvider<AefSession> sessionProvider,
            IAnnouncementManager announcementManager,
            ITaskIncentiveManager taskIncentiveManager,
            IIncentiveKindManager incentiveKindManager) : base(sessionProvider)
        {
            Args.NotNull(announcementManager, nameof(announcementManager));
            Args.NotNull(taskIncentiveManager, nameof(taskIncentiveManager));
            Args.NotNull(incentiveKindManager, nameof(incentiveKindManager));
            m_AnnouncementManager = announcementManager;
            m_TaskIncentiveManager = taskIncentiveManager;
            m_IncentiveKindManager = incentiveKindManager;
        }

        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly ITaskIncentiveManager m_TaskIncentiveManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;


        public AnncIncentiveEntity CreateOrUpdateAnncIncentive(Guid anncId, int incentiveKindId, decimal amount)
        {
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;
            var anncIncentive =
                AnncIncentiveExistsResult.CheckByAnncIdAndKind(this, anncId, incentiveKindId).AnncIncentiveEntity;

            //激励数不能大于任务的激励值
            var taskIncentive =
                TaskIncentiveExistsResult.Check(this.m_TaskIncentiveManager, annc.Task.Id, incentiveKindId).TaskIncentive;

            var incentiveKind = IncentiveKindExistsResult.Check(m_IncentiveKindManager, incentiveKindId).ThrowIfFailed().IncentiveKind;

            if (taskIncentive.Amount == 0)
                throw new FineWorkException($"[{annc.Task.Name}]未设置{taskIncentive.IncentiveKind.Name}值.");
            if (amount > taskIncentive.Amount)
                throw new FineWorkException($"值不能大于任务设置的{taskIncentive.IncentiveKind.Name}值.");

            if (anncIncentive != null)
            {
                this.InternalUpdate(anncIncentive);
                return anncIncentive;
            }

            var anncIncentiveEntity = new AnncIncentiveEntity();
            anncIncentiveEntity.Id = Guid.NewGuid();
            anncIncentiveEntity.Amount = amount;
            anncIncentiveEntity.Announcement = annc;
            anncIncentiveEntity.IncentiveKind = incentiveKind;
            
            this.InternalInsert(anncIncentiveEntity);
            return anncIncentiveEntity;

        }

        public AnncIncentiveEntity FindAnncIncentiveByAnncIdAndKind(Guid anncId, int incentiveKind)
        {
            return
                this.InternalFetch(p => p.Announcement.Id == anncId && p.IncentiveKind.Id == incentiveKind)
                    .FirstOrDefault();
        }

        public IEnumerable<AnncIncentiveEntity> FetchAnncIncentivesByAnncId(Guid anncId)
        {
            return this.InternalFetch(p => p.Announcement.Id == anncId);
        }
    }
}

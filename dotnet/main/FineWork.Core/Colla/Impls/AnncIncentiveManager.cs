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
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class AnncIncentiveManager: AefEntityManager<AnncIncentiveEntity, Guid>, IAnncIncentiveManager
    {
        public AnncIncentiveManager(ISessionProvider<AefSession> sessionProvider,
            LazyResolver<IAnnouncementManager> announcementManagerResolver,
            ITaskIncentiveManager taskIncentiveManager,
            IIncentiveKindManager incentiveKindManager,
            IIncentiveManager incentiveManager) : base(sessionProvider)
        {
            Args.NotNull(announcementManagerResolver, nameof(announcementManagerResolver));
            Args.NotNull(taskIncentiveManager, nameof(taskIncentiveManager));
            Args.NotNull(incentiveKindManager, nameof(incentiveKindManager));
            Args.NotNull(incentiveManager, nameof(incentiveManager));
            m_AnnouncementManagerResolver = announcementManagerResolver;
            m_TaskIncentiveManager = taskIncentiveManager;
            m_IncentiveKindManager = incentiveKindManager;
            m_IncentiveManager = incentiveManager;
        }

        private readonly LazyResolver<IAnnouncementManager> m_AnnouncementManagerResolver;
        private readonly ITaskIncentiveManager m_TaskIncentiveManager;
        private readonly IIncentiveKindManager m_IncentiveKindManager;
        private readonly IIncentiveManager m_IncentiveManager;

        private    IAnnouncementManager AnnouncementManager {
            get { return m_AnnouncementManagerResolver.Required; }
        }

        public AnncIncentiveEntity CreateOrUpdateAnncIncentive(Guid anncId, int incentiveKindId, decimal amount)
        {
            var annc = AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed().Annc;
            var anncIncentive =
                AnncIncentiveExistsResult.CheckByAnncIdAndKind(this, anncId, incentiveKindId).AnncIncentiveEntity;
            
            //任务已经发出的激励数
            var taskIncentiveGross = m_IncentiveManager.FetchIncentiveByTaskId(annc.Task.Id)
                .Where(p => p.TaskIncentive.IncentiveKind.Id == incentiveKindId).Sum(p => p.Quantity);

            //已经发出的预算
            var reportIncentiveGross = this.InternalFetch(p=>p.Announcement.Task.Id==annc.Task.Id && p.IncentiveKind.Id == incentiveKindId)
                .Sum(p => p.Amount);

            //激励数不能大于任务剩余的激励值
            var taskIncentive =
                TaskIncentiveExistsResult.Check(this.m_TaskIncentiveManager, annc.Task.Id, incentiveKindId).TaskIncentive;

            var incentiveKind = IncentiveKindExistsResult.Check(m_IncentiveKindManager, incentiveKindId).ThrowIfFailed().IncentiveKind;

            if (amount>0 && (taskIncentive==null || taskIncentive.Amount == 0))
                throw new FineWorkException($"[{annc.Task.Name}]未设置激励.");
            if (amount>0 && amount > (taskIncentive.Amount-taskIncentiveGross-reportIncentiveGross))
                throw new FineWorkException($"不能大于任务{taskIncentive.IncentiveKind.Name}的余额.");

            if (anncIncentive != null)
            {
                anncIncentive.Amount = amount;
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

        public void DeleteIncentiveByAnncId(Guid anncId)
        {
            AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed();

            var incentives = this.InternalFetch(p => p.Announcement.Id == anncId).ToList();

            if (incentives.Any())
            {
                foreach (var incentive in incentives)
                {
                    this.InternalDelete(incentive);
                }
            }
        }
    }
}

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
    public class AnncIncentiveManager : AefEntityManager<AnncIncentiveEntity, Guid>, IAnncIncentiveManager
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

        private IAnnouncementManager AnnouncementManager
        {
            get { return m_AnnouncementManagerResolver.Required; }
        }

        public AnncIncentiveEntity CreateOrUpdateAnncIncentive(Guid anncId, int incentiveKindId, decimal amount,
            bool isGrant)
        {
            var annc = AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed().Annc;
            var anncIncentive =
                AnncIncentiveExistsResult.CheckByAnncIdAndKind(this, anncId, incentiveKindId).AnncIncentiveEntity;


            var taskIncentive =
                TaskIncentiveExistsResult.Check(this.m_TaskIncentiveManager, annc.Task.Id, incentiveKindId)
                    .TaskIncentive;

            var incentiveKind =
                IncentiveKindExistsResult.Check(m_IncentiveKindManager, incentiveKindId).ThrowIfFailed().IncentiveKind;

            if (amount > 0 && (taskIncentive == null || taskIncentive.Amount == 0))
                throw new FineWorkException($"[{annc.Task.Name}]未设置激励.");

           var balance= IncentiveBalanceResult.Check(this.m_TaskIncentiveManager, this, this.m_IncentiveManager, annc.Task.Id,
                incentiveKindId, anncId).ThrowIfFailed().Balance;
            if(balance<amount)
                throw new FineWorkException($"任务{taskIncentive.IncentiveKind.Name}余额不足.");

            if (anncIncentive != null)
            {
                if (isGrant)
                    anncIncentive.Grant = amount;
                else
                    anncIncentive.Amount = amount;
                this.InternalUpdate(anncIncentive);
                return anncIncentive;
            }

            var anncIncentiveEntity = new AnncIncentiveEntity();
            anncIncentiveEntity.Id = Guid.NewGuid();
            if (isGrant)
                anncIncentiveEntity.Grant = amount;
            else
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

        public IEnumerable<AnncIncentiveEntity> FetchAnncIncentiveByTaskIdAndKind(Guid taskId, int incentiveKind)
        {
            return this.InternalFetch(p => p.Announcement.Task.Id == taskId && p.IncentiveKind.Id == incentiveKind);
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
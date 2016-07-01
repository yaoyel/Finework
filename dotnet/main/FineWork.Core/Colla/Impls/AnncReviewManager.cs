using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class AnncReviewManager: AefEntityManager<AnncReviewEntity, Guid>, IAnncReviewManager
    {
        public AnncReviewManager(ISessionProvider<AefSession> sessionProvider,
            LazyResolver<IAnnouncementManager> announcementLazyResolver,
            IAnncIncentiveManager anncIncentiveManager,
            IIncentiveManager incentiveManager) : base(sessionProvider)
        {
            Args.NotNull(announcementLazyResolver, nameof(announcementLazyResolver));
            Args.NotNull(anncIncentiveManager, nameof(anncIncentiveManager));
            Args.NotNull(incentiveManager, nameof(incentiveManager));
            m_AnnouncementLazyResolver = announcementLazyResolver;
            m_AnncIncentiveManager = anncIncentiveManager;
            m_IncentiveManager = incentiveManager;
        }

        private readonly LazyResolver<IAnnouncementManager> m_AnnouncementLazyResolver;
        private readonly IAnncIncentiveManager m_AnncIncentiveManager;
        private readonly IIncentiveManager m_IncentiveManager;

        private IAnnouncementManager AnnouncementManager
        {
            get { return m_AnnouncementLazyResolver.Required; }
        }

        public AnncReviewEntity CreateAnncReivew(Guid anncId, ReviewStatuses reviewStatus)
        { 
            var annc = AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed().Annc;

            var lastReviewStatus = annc.Reviews.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if(lastReviewStatus!=null && lastReviewStatus.Reviewstatus==  ReviewStatuses.Approved)
                throw new FineWorkException($"该通告于{lastReviewStatus.CreatedAt.ToString("yyyy-MM-dd")}已验证通过，不可重新验证.");

            var anncReviewEnitty=new AnncReviewEntity();
            anncReviewEnitty.Id = Guid.NewGuid();
            anncReviewEnitty.Reviewstatus = reviewStatus;
            anncReviewEnitty.Annc = annc;
            this.InternalInsert(anncReviewEnitty);


            var leader = annc.Task.Partakers.First(p => p.Kind == PartakerKinds.Leader).Staff;
            //兑现激励
            if (reviewStatus == ReviewStatuses.Approved)
                foreach (var incentive in annc.AnncIncentives)
                {
                    m_IncentiveManager.CreateIncentive(annc.Task.Id, incentive.IncentiveKind.Id, leader.Id,
                        annc.Staff.Id, incentive.Grant ?? incentive.Amount);
                }
       
            return anncReviewEnitty;
        }

        public IEnumerable<AnncReviewEntity> FetchAnncReviewByAnncId(Guid anncId)
        {
            return this.InternalFetch(p => p.Annc.Id == anncId);
        }
    }
}
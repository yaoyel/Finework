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
            IIncentiveManager incentiveManager) : base(sessionProvider)
        {
            Args.NotNull(announcementLazyResolver, nameof(announcementLazyResolver)); 
            Args.NotNull(incentiveManager, nameof(incentiveManager));
            m_AnnouncementLazyResolver = announcementLazyResolver; 
            m_IncentiveManager = incentiveManager;
        }

        private readonly LazyResolver<IAnnouncementManager> m_AnnouncementLazyResolver;
     
        private readonly IIncentiveManager m_IncentiveManager;

        private IAnnouncementManager AnnouncementManager
        {
            get { return m_AnnouncementLazyResolver.Required; }
        }

        public AnncReviewEntity CreateAnncReivew(Guid anncId, AnncStatus reviewStatus, DateTime? delayAt=null)
        {
            var annc = AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed().Annc;

            var lastReviewStatus = annc.Reviews.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
            if (lastReviewStatus != null && lastReviewStatus.Reviewstatus == AnncStatus.Approved)
                throw new FineWorkException($"该计划于{lastReviewStatus.CreatedAt.ToString("yyyy-MM-dd")}已验证通过，不可重新验证.");
            if (reviewStatus == AnncStatus.Abandon && annc.Reviews.Any(p => p.Reviewstatus == AnncStatus.Abandon))
                throw new FineWorkException($"该计划已经被放弃,不可重复改操作.");

            var anncReviewEnitty = new AnncReviewEntity(); 
  
            if (reviewStatus == AnncStatus.Delay)
            {
                if (!delayAt.HasValue)
                    throw new FineWorkException("请设置延期时间.");
                if (delayAt.HasValue && delayAt.Value < annc.EndAt)
                    throw new FineWorkException("延期时间不能小于计划的结束时间");
                anncReviewEnitty.DelayAt = delayAt;
            }
            anncReviewEnitty.Id = Guid.NewGuid();
            anncReviewEnitty.Reviewstatus = reviewStatus;
            anncReviewEnitty.Annc = annc;


            this.InternalInsert(anncReviewEnitty);
            return anncReviewEnitty;
        }

        public IEnumerable<AnncReviewEntity> FetchAnncReviewByAnncId(Guid anncId)
        {
            return this.InternalFetch(p => p.Annc.Id == anncId);
        }
    }
}
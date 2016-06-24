using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Security.Crypto;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class AnncAttManager : AefEntityManager<AnncAttEntity, Guid>, IAnncAttManager
    {
        public AnncAttManager(ISessionProvider<AefSession> sessionProvider,
            IAnnouncementManager anncManager, ITaskSharingManager taskSharingManager) : base(sessionProvider)
        {
            Args.NotNull(anncManager, nameof(anncManager));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));
            m_AnnouncementManager = anncManager;
            m_TaskSharingManager = taskSharingManager;
        }

        private readonly IAnnouncementManager m_AnnouncementManager;
        private readonly ITaskSharingManager m_TaskSharingManager;
      

        public AnncAttEntity CreateAnncAtt(Guid anncId, Guid taskSharingId, bool isAchv)
        {
            var annc = AnncExistsResult.Check(this.m_AnnouncementManager, anncId).ThrowIfFailed().Annc;
            var taskSharing =
                TaskSharingExistsResult.Check(this.m_TaskSharingManager, taskSharingId).ThrowIfFailed().TaskSharing;

            var anncAtt=new AnncAttEntity();
            anncAtt.Id = Guid.NewGuid();
            anncAtt.Announcement = annc;
            anncAtt.TaskSharing = taskSharing;
            anncAtt.IsAchv = isAchv; 

            this.InternalInsert(anncAtt);
            return anncAtt;
        }



        public IEnumerable<AnncAttEntity> FetchAnncAttsByAnncId(Guid anncId, bool isAchv)
        {
            return this.InternalFetch(p => p.Announcement.Id == anncId && p.IsAchv == isAchv);
        }



        public void DeleteAnncAtt(Guid anncAttId)
        {
            var anncAtt = this.InternalFind(anncAttId);
            if (anncAtt == null) return;
            this.InternalDelete(anncAtt);
        }
    }

}
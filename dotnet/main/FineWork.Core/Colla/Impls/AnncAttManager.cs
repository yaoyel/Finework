using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Security.Crypto;
using FineWork.Colla.Checkers;
using FineWork.Common;
using FineWork.Core;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class AnncAttManager : AefEntityManager<AnncAttEntity, Guid>, IAnncAttManager
    {
        public AnncAttManager(ISessionProvider<AefSession> sessionProvider,
            LazyResolver<IAnnouncementManager> anncManagerResolver, ITaskSharingManager taskSharingManager) : base(sessionProvider)
        {
            Args.NotNull(anncManagerResolver, nameof(anncManagerResolver));
            Args.NotNull(taskSharingManager, nameof(taskSharingManager));
            m_AnnouncementManagerResolver = anncManagerResolver;
            m_TaskSharingManager = taskSharingManager;
        }

        private readonly LazyResolver<IAnnouncementManager> m_AnnouncementManagerResolver;
        private readonly ITaskSharingManager m_TaskSharingManager;

        private IAnnouncementManager AnnouncementManager
        {
            get { return m_AnnouncementManagerResolver.Required; }
        }

        public AnncAttEntity CreateAnncAtt(Guid anncId, Guid taskSharingId, bool isAchv)
        {
            var annc = AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed().Annc;
            var taskSharing =
                TaskSharingExistsResult.Check(this.m_TaskSharingManager, taskSharingId).ThrowIfFailed().TaskSharing;

           var anncAtt= AnncAttExistsResult.Check(this, anncId, taskSharingId, isAchv).AnncAtt;

            if (anncAtt != null) return anncAtt;

            var anncAttEntity=new AnncAttEntity();
            anncAttEntity.Id = Guid.NewGuid();
            anncAttEntity.Announcement = annc;
            anncAttEntity.TaskSharing = taskSharing;
            anncAttEntity.IsAchv = isAchv; 

            this.InternalInsert(anncAttEntity);
            return anncAttEntity;
        } 


        public IEnumerable<AnncAttEntity> FetchAnncAttsByAnncId(Guid anncId, bool isAchv)
        {
            return this.InternalFetch(p => p.Announcement.Id == anncId && p.IsAchv == isAchv);
        }

        public IEnumerable<AnncAttEntity> FetchAnncAttsBySharingId(Guid taskSharingId)
        {
            return this.InternalFetch(p => p.TaskSharing.Id == taskSharingId);
        }

        public void DeleteAnncAtt(Guid anncAttId)
        {
            var anncAtt = this.InternalFind(anncAttId);
            if (anncAtt == null) return;
            this.InternalDelete(anncAtt);
        }

        public void DeleteAnncAttByAnncId(Guid anncId)
        {
            AnncExistsResult.Check(this.AnnouncementManager, anncId).ThrowIfFailed();
            var atts = this.InternalFetch(p => p.Announcement.Id == anncId).ToList();
            if (atts.Any())
            {
                foreach (var att in atts)
                {
                    this.InternalDelete(att);
                }
            }
        }

        public AnncAttEntity FindAnncAttByAnncAndSharingId(Guid anncId, Guid taskSharingId, bool isAchv)
        {
            return
                this.InternalFetch(
                    p => p.TaskSharing.Id == taskSharingId && p.IsAchv == isAchv && p.Announcement.Id == anncId)
                    .FirstOrDefault();
        }

        public void UpdateAnncAtts(AnnouncementEntity annc, Guid[] taskSharingIds)
        {
            Args.NotNull(annc, nameof(annc));
            var atts = annc.Atts.Select(p => p.TaskSharing.Id).ToArray();

            var newAtts = taskSharingIds.Except(atts).ToArray();

            if (newAtts.Any())
            {
                foreach (var att in newAtts)
                {
                    this.CreateAnncAtt(annc.Id, att, false);
                }
            }

            var diffAtts = atts.Except(taskSharingIds).ToArray();

            if (diffAtts.Any())
            {
                foreach (var att in diffAtts)
                {
                    var attEntity = AnncAttExistsResult.Check(this, annc.Id, att, false).AnncAtt;
                    if (attEntity != null)
                        this.InternalDelete(attEntity);
                }
            }

        }
    }

}
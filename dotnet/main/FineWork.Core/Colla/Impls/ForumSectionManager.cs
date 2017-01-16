using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class ForumSectionManager : AefEntityManager<ForumSectionEntity, Guid>, IForumSectionManager
    {
        public ForumSectionManager(ISessionProvider<AefSession> sessionProvider,
            IStaffManager staffManager,
            IAccessTimeManager accessTimeManager,
            ILazyResolver<IForumTopicManager> forumTopicManagerResolver,
            ILazyResolver<IForumCommentLikeManager> forumCommentLikeManageResolver,
            ILazyResolver<IForumLikeManager> forumLikeManageResolver,
            ILazyResolver<IForumCommentManager> forumCommentManageResolver) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));

            m_StaffManager = staffManager;
            m_AccessTimeManager = accessTimeManager;
            m_ForumCommentLikeManageResolver = forumCommentLikeManageResolver;
            m_ForumLikeManageResolver = forumLikeManageResolver;
            m_ForumTopicManagerResolver = forumTopicManagerResolver;
            m_ForumCommentManageResolver = forumCommentManageResolver;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly IAccessTimeManager m_AccessTimeManager;
        private readonly ILazyResolver<IForumTopicManager> m_ForumTopicManagerResolver;
        private readonly ILazyResolver<IForumCommentLikeManager> m_ForumCommentLikeManageResolver;
        private readonly ILazyResolver<IForumLikeManager> m_ForumLikeManageResolver;
        private readonly ILazyResolver<IForumCommentManager> m_ForumCommentManageResolver; 

        private IForumTopicManager ForumTopicManager
        {
            get { return m_ForumTopicManagerResolver.Required; }
        }

        private IForumCommentLikeManager ForumCommentLikeManager
        {
            get { return m_ForumCommentLikeManageResolver.Required; }
        }

        private IForumLikeManager ForumLikeManager
        {
            get { return m_ForumLikeManageResolver.Required; }
        }

        private IForumCommentManager ForumCommentManager
        {
            get { return m_ForumCommentManageResolver.Required; }
        }
         

        public ForumSectionEntity CreateForumSetcion(CreateForumSectionModel forumSectionModel)
        {
            Args.NotNull(forumSectionModel, nameof(forumSectionModel));

            var staff = StaffExistsResult.Check(this.m_StaffManager, forumSectionModel.StaffId).ThrowIfFailed().Staff;
            var forumSectionEntity = new ForumSectionEntity();
            forumSectionEntity.Content = forumSectionModel.Content;
            forumSectionEntity.Staff = staff;
            forumSectionEntity.Section = forumSectionModel.SectionId;
            forumSectionEntity.Id = Guid.NewGuid();

            this.InternalInsert(forumSectionEntity);
            return forumSectionEntity;
        }

        public ForumSectionEntity FetchForumSectionByStaffId(Guid staffId, ForumSections section,
            string version)
        {
            var forumSections = this.InternalFetch(
                p => p.Staff.Id == staffId && p.Section == section)
                .OrderBy(p => p.CreatedAt)
                .ToList();

            if (string.IsNullOrEmpty(version))
                return forumSections.LastOrDefault();


            var versionSplit = version.Substring(0, version.Length - 1).Split('年');
            var yearOfVersion = int.Parse(versionSplit[0]);
            var orderOfVersion = int.Parse(versionSplit[1]);

            var forumSectionsByVersion = forumSections.Where(p => p.CreatedAt.Year == yearOfVersion).ToList();

            if (orderOfVersion > forumSectionsByVersion.Count) return null;

            return forumSectionsByVersion[orderOfVersion - 1];
        }

        public IEnumerable<ForumSectionEntity> FetchForumSectionByOrgIdWithSection(Guid orgId, ForumSections section)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId && p.Section == section).OrderBy(p => p.CreatedAt);
        }

        public IEnumerable<ForumSectionEntity> FetchForumSectionByOrgId(Guid orgId)
        {
            return this.InternalFetch(p => p.Staff.Org.Id == orgId).OrderBy(p => p.CreatedAt);
        }

        public ForumSectionEntity FindById(Guid forumSectionId)
        {
            return this.InternalFind(forumSectionId);
        }

        public ForumSections[] HasUnReadForumByStaffId(Guid staffId)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).Staff;

            if (staff != null)
            {
                var accessTime = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staffId).AccessTime;
                var forumTopics = ForumTopicManager.FetchForumTopicByOrgId(staff.Org.Id).ToList();
                var forumSection = this.FetchForumSectionByOrgId(staff.Org.Id).ToList();

                if ((accessTime?.LastViewForumAt == null) && forumTopics.Any())
                    return forumTopics.Select(p => p.ForumSection.Section).ToArray();

                if (accessTime?.LastViewForumAt != null)
                {
                    var unReadSections =
                        forumSection.Where(p => p.Staff.Id != staffId)
                            .ToList()
                            .Where(p=> p.ViewStaffs.All(a => a.Staff.Id != staffId))
                            .Select(p => p.Section).ToArray();

                    var unReadTopicSections =
                        forumTopics.Where(p => p.Staff.Id != staffId)
                            .ToList()
                            .Where(p => p.CreatedAt > GetLastViewForumSectionTime(staffId, p.ForumSection.Section))
                            .Select(p => p.ForumSection.Section)
                            .ToArray();

                    var unReadCommentSections = ForumCommentManager.FetchCommentsByTopicOrCommentCreator(staff.Id)
                        .Where(p => p.Staff.Id != staffId).ToList()
                        .Where(
                            p => p.CreatedAt > GetLastViewForumSectionTime(staffId, p.ForumTopic.ForumSection.Section))
                        .Select(p => p.ForumTopic.ForumSection.Section)
                        .ToArray();

                    var unReadLikeSections = ForumLikeManager.FetchForumLikesByTopicCreatorId(staffId)
                        .Where(p => p.Staff.Id != staffId).ToList()
                        .Where(
                            p => p.CreatedAt > GetLastViewForumSectionTime(staffId, p.ForumTopic.ForumSection.Section))
                        .Select(p => p.ForumTopic.ForumSection.Section)
                        .ToArray();

                    return unReadLikeSections.Union(unReadTopicSections)
                        .Union(unReadCommentSections)
                        .Union(unReadSections)
                        .Distinct().OrderBy(p => p).ToArray();
                }

            }
            return new ForumSections[] {};
        }

        public DateTime GetLastViewForumSectionTime(Guid staffId, ForumSections section)
        {
            var accessTime = AccessTimeExistsResult.CheckByStaff(this.m_AccessTimeManager, staffId).AccessTime;
            switch (section)
            {
                case ForumSections.Mission:
                    return accessTime.LastViewMissinAt ?? default(DateTime);
                case ForumSections.Vision:
                    return accessTime.LastViewVisionAt ?? default(DateTime);
                case ForumSections.Values:
                    return accessTime.LastViewValuesAt ?? default(DateTime);
                case ForumSections.Strategy:
                    return accessTime.LastViewStrategyAt ?? default(DateTime);
                case ForumSections.OrgGovernance:
                    return accessTime.LastViewOrgGovernanceAt ?? default(DateTime);
                default:
                    return default(DateTime);
            }
        }

        public string FetchVersionByForumSectionId(Guid forumSectionId)
        {
            var forumSection = ForumSectionExistsResult.Check(this, forumSectionId).ThrowIfFailed().ForumSection;
            if (forumSection != null)
            {
                var forumSectionInSameYear = this.InternalFetch(p => p.CreatedAt.Year == forumSection.CreatedAt.Year
                                                                     && p.Staff.Org.Id == forumSection.Staff.Org.Id &&
                                                                     p.Section == forumSection.Section).OrderBy(p=>p.CreatedAt).ToList();

                   var version = $"{forumSection.CreatedAt.Year}年{(forumSectionInSameYear.IndexOf(forumSection)+1).ToString().PadLeft(2, '0')}版";

                return version;
            }

            return string.Empty; 
        } 

        public Tuple<Guid[], int[]>  FetchIrrelevantForumsByStaff(Guid staffId,params ForumSections[] sections)
        {
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).Staff;
            if (staff != null)
            {
                var forumSectionIds = new List<Guid>();
                var sectionIds=new List<int>();
                var irrelevates = this.InternalFetch(p => p.Staff.Org.Id == staff.Org.Id && sections.Contains(p.Section))
                    .Select(p =>
                    { 
                        var viewTime = p.ViewStaffs.FirstOrDefault(f => f.Staff.Id == staff.Id);

                        if (viewTime == null || p.CreatedAt>=viewTime.CreatedAt)
                        {
                            if (p.Staff.Id == staffId) return p.Id;
                            forumSectionIds.Add(p.Id); 
                            sectionIds.Add((int)p.Section);
                            return p.Id;
                        } 

                        if (p.ForumTopics.Any())
                            if (
                                p.ForumTopics.Any(
                                    a =>
                                        a.Staff.Id != staffId && !forumSectionIds.Contains(a.ForumSection.Id) &&
                                        a.CreatedAt >= viewTime.CreatedAt))
                            {
                                forumSectionIds.Add(p.Id);
                                sectionIds.Add((int)p.Section);
                            }

                        if (p.ForumTopics.Any(a => a.ForumLikes.Any()))
                            if (p.ForumTopics.Where(w => !forumSectionIds.Contains(w.ForumSection.Id))
                                .Any(
                                    a =>
                                        a.ForumLikes.Any(u => u.CreatedAt > viewTime.CreatedAt && u.Staff.Id != staffId)))
                            {
                                forumSectionIds.Add(p.Id);
                                sectionIds.Add((int)p.Section);
                            }

                        if (p.ForumTopics.Any(a => a.ForumComments.Any()))
                            if (p.ForumTopics.Where(w => !forumSectionIds.Contains(w.ForumSection.Id))
                                .Any(
                                    a =>
                                        a.ForumComments.Any(
                                            u => u.CreatedAt > viewTime.CreatedAt && (u.Staff.Id != staffId
                                                                                      ||
                                                                                      (u.TargetComment != null &&
                                                                                       u.TargetComment.Staff.Id !=
                                                                                       staff.Id)))))
                            {
                                forumSectionIds.Add(p.Id);
                                sectionIds.Add((int)p.Section);
                            }

                        if (p.ForumTopics.Any(f => f.ForumComments.Any(a => a.Likes.Any())))
                            if (p.ForumTopics.Where(w => !forumSectionIds.Contains(w.ForumSection.Id))
                                .Any(
                                    a =>
                                        a.ForumComments.Any(
                                            c =>
                                                c.Likes.Any(
                                                    u => u.CreatedAt > viewTime.CreatedAt && u.Staff.Id != staffId))))
                            {
                                forumSectionIds.Add(p.Id);
                                sectionIds.Add((int)p.Section);
                            }

                        return p.Id;
                    }).ToList();


                return new Tuple<Guid[], int[]>(forumSectionIds.Distinct().ToArray(), sectionIds.Distinct().ToArray());
            }
            return null;
        }
    }
}
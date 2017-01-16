using System;
using System.Collections.Generic;
using System.Linq;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class AnncAlarmRecManager : AefEntityManager<AnncAlarmRecEntity, Guid>, IAnncAlarmRecManager
    {
        public AnncAlarmRecManager(ISessionProvider<AefSession> sessionProvider,
            ILazyResolver<IAnncAlarmManager> anncAlarmLazyResolver,
            IStaffManager staffManager) : base(sessionProvider)
        {
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(anncAlarmLazyResolver, nameof(anncAlarmLazyResolver));

            m_StaffManager = staffManager;
            m_AnncAlarmLazyResolver = anncAlarmLazyResolver;
        }

        private readonly IStaffManager m_StaffManager;
        private readonly ILazyResolver<IAnncAlarmManager> m_AnncAlarmLazyResolver;

        private IAnncAlarmManager AnncAlarmManager
        {
            get { return m_AnncAlarmLazyResolver.Required; }
        }

        public AnncAlarmRecEntity CreateAnncAlarmRec(Guid anncAlarmId, Guid staffId, AnncRoles anncRole)
        {
            var anncAlarm = AnncAlarmExistsResult.Check(this.AnncAlarmManager, anncAlarmId).ThrowIfFailed().AnncAlarm;
            var staff = StaffExistsResult.Check(this.m_StaffManager, staffId).ThrowIfFailed().Staff;
            var rec = new AnncAlarmRecEntity();
            rec.Id = Guid.NewGuid();
            rec.AnncAlarm = anncAlarm;
            rec.Staff = staff;
            rec.AnncRole = anncRole;

            this.InternalInsert(rec);
            return rec;
        }

        public void DeleteRecByAnncId(Guid anncId, Guid staffId, AnncRoles anncRole)
        {
            var recs =
                this.InternalFetch(p => p.AnncAlarm.Annc.Id == anncId && p.Staff.Id == staffId && p.AnncRole == anncRole);
            if (recs.Any())
                foreach (var rec in recs)
                {
                    var anncAlarm = rec.AnncAlarm;

                    this.InternalDelete(rec);
                    if (!anncAlarm.Recs.Any())
                        AnncAlarmManager.DeleteAnncAlarm(anncAlarm.Id);
                }
        }

        public IEnumerable<AnncAlarmRecEntity> FetchRecsByAnncAlarmId(Guid anncAlarmId)
        {
            return this.InternalFetch(p => p.AnncAlarm.Id == anncAlarmId);
        }

        public void DeleteAnncAlarmRecByAlarmId(Guid anncAlarmId)
        {
            var recs = this.InternalFetch(p => p.AnncAlarm.Id == anncAlarmId);

            if (recs.Any())
                foreach (var rec in recs)
                {
                    this.InternalDelete(rec);
                }
        }

        public IEnumerable<AnncAlarmRecEntity> FetchRecsByAnncIdWithStaffId(Guid anncId, Guid staffId, AnncRoles role)
        {
            return
                this.InternalFetch(p => p.AnncAlarm.Annc.Id == anncId && p.AnncRole == role && p.Staff.Id == staffId);
        }

        public IEnumerable<AnncAlarmRecEntity> FetchRecsByAnncIdWithRole(Guid anncId, AnncRoles role)
        {
            return
               this.InternalFetch(p => p.AnncAlarm.Annc.Id == anncId && p.AnncRole == role);
        }

        public void UpdateAnncAlarmRec(AnncAlarmRecEntity rec)
        {
            Args.NotNull(rec, nameof(rec));
            this.InternalUpdate(rec);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;

namespace FineWork.Colla.Impls
{
    public class AnncAlarmManager : AefEntityManager<AnncAlarmEntity, Guid>, IAnncAlarmManager
    {
        public AnncAlarmManager(ISessionProvider<AefSession> sessionProvider,
            ILazyResolver<IAnnouncementManager>  announcementManagerResolver,
            IStaffManager staffManager,
            IAnncAlarmRecManager anncAlarmRecManager) : base(sessionProvider)
        {
            Args.NotNull(announcementManagerResolver, nameof(announcementManagerResolver));
            Args.NotNull(staffManager, nameof(staffManager));
            Args.NotNull(anncAlarmRecManager, nameof(anncAlarmRecManager));

            m_AnnouncementManagerResolver = announcementManagerResolver;
            m_StaffManager = staffManager;
            m_AnncAlarmRecManager = anncAlarmRecManager;
        }

        private readonly ILazyResolver<IAnnouncementManager> m_AnnouncementManagerResolver;
        private readonly IStaffManager m_StaffManager;
        private readonly IAnncAlarmRecManager m_AnncAlarmRecManager;

        private IAnnouncementManager AnnouncementManager {
            get {return m_AnnouncementManagerResolver.Required; }
        }

        public AnncAlarmEntity CreateAnnAlarm(CreateAnncAlarmModel createAnncAlarmModel)
        {
            Args.NotNull(createAnncAlarmModel, nameof(createAnncAlarmModel)); 
            
            if((createAnncAlarmModel.Time==null && createAnncAlarmModel.BeforeStart==null) || createAnncAlarmModel.Time==default(DateTime))
                throw new FineWorkException("请输入正确的时间");
            if(createAnncAlarmModel.Recs==null || !createAnncAlarmModel.Recs.Any()) throw  new FineWorkException("请选择预警成员");
            if(!createAnncAlarmModel.AnncId.HasValue) throw new FineWorkException("请选择正确的计划");

            var annc =
                AnncExistsResult.Check(this.AnnouncementManager, createAnncAlarmModel.AnncId.Value).ThrowIfFailed().Annc;

            var anncAlarm=new AnncAlarmEntity();
            anncAlarm.Id = Guid.NewGuid();
            anncAlarm.Time = createAnncAlarmModel.Time;
            anncAlarm.Annc = annc;
            anncAlarm.Bell = createAnncAlarmModel.Bell;
            anncAlarm.IsEnabled = createAnncAlarmModel.IsEnabled;
            anncAlarm.BeforeStart = createAnncAlarmModel.BeforeMins;
            this.InternalInsert(anncAlarm);

            foreach (var rec in createAnncAlarmModel.Recs)
            {
                m_AnncAlarmRecManager.CreateAnncAlarmRec(anncAlarm.Id, rec.Item2, rec.Item1);
            } 
           
            return anncAlarm;
        }

        public AnncAlarmEntity UpdateAnncAlarm(UpdateAnncAlarmModel updateAnncAlarmModel)
        {
            Args.NotNull(updateAnncAlarmModel, nameof(updateAnncAlarmModel));

            if (updateAnncAlarmModel.Time == default(DateTime)) throw new FineWorkException("请输入正确的时间");
            if (!updateAnncAlarmModel.Recs.Any()) throw new FineWorkException("请选择预警成员");

            if (updateAnncAlarmModel.AnncAlarmId.HasValue)
            {
                var anncAlarm =
                    AnncAlarmExistsResult.Check(this, updateAnncAlarmModel.AnncAlarmId.Value).ThrowIfFailed().AnncAlarm;

                anncAlarm.Time = updateAnncAlarmModel.Time;
                anncAlarm.Bell = updateAnncAlarmModel.Bell;
                anncAlarm.IsEnabled = updateAnncAlarmModel.IsEnabled;

                this.InternalUpdate(anncAlarm);

                m_AnncAlarmRecManager.DeleteAnncAlarmRecByAlarmId(anncAlarm.Id);

                if (updateAnncAlarmModel.Recs != null && updateAnncAlarmModel.Recs.Any())
                    foreach (var rec in updateAnncAlarmModel.Recs)
                    {
                        m_AnncAlarmRecManager.CreateAnncAlarmRec(anncAlarm.Id, rec.Item2, rec.Item1);
                    }

                return anncAlarm;
            }
            return null;
        }

        public void DeleteAnncAlarm(Guid anncAlarmId)
        {
            var anncAlarm = AnncAlarmExistsResult.Check(this, anncAlarmId).AnncAlarm;

            if (anncAlarm != null)
            {
                m_AnncAlarmRecManager.DeleteAnncAlarmRecByAlarmId(anncAlarmId);
                this.InternalDelete(anncAlarm);
            } 
        }

        public AnncAlarmEntity FindById(Guid anncAlarmId)
        {
            return this.InternalFind(anncAlarmId);
        }

        public AnncAlarmEntity FindByAnncIdAndTime(Guid anncId, DateTime time)
        {
            return this.InternalFetch(p => p.Annc.Id == anncId && p.Time == time).FirstOrDefault();
        }

        public IEnumerable<AnncAlarmEntity> FetchAnncAlarmsByAnncId(Guid anncId)
        {
            return this.InternalFetch(p => p.Annc.Id == anncId);
        }

        public void DeleteAnncAlarmByAnncId(Guid anncId)
        {
            var alarms = this.InternalFetch(p => p.Annc.Id == anncId);
            if(alarms.Any())
                foreach (var alarm in alarms)
                {
                 this.DeleteAnncAlarm(alarm.Id);   
                }
        }

        public IEnumerable<AnncAlarmEntity> FetchAnncAlarmsByTime(DateTime time)
        {
            var context = this.Session.DbContext;
            var set = context.Set<AnncAlarmEntity>()
                .Include(p=>p.Annc.Task.Partakers.Select(s=>s.Staff.Org))
                .Include(p => p.Annc.Task.Partakers.Select(s => s.Staff.Account))
                .Include(p=>p.Annc.Creator)
                .Include(p=>p.Annc.Inspecter)
                .Include(p=>p.Recs.Select(s=>s.Staff.Org))
                .AsNoTracking().ToList();

            var anncSet = context.Set<AnnouncementEntity>();

            if (!set.Any()) return null;
            //换算成东八区时间 
            TimeZoneInfo local = TimeZoneInfo.Local;
            time = time.AddHours(8 - local.BaseUtcOffset.Hours);

            var timeOfDay = time.TimeOfDay;

            var timeFormat = new DateTime(time.Year, time.Month, time.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);

            var alarms =
                set.Where(p => p.Time == timeFormat ||
                               (p.BeforeStart.HasValue && AnncAlarmMatch(p.Annc, p.BeforeStart.Value, timeFormat)))
                    .ToList();

            var alarmsAtEndTime = anncSet.Where(p => p.EndAt.HasValue && p.EndAt.Value == timeFormat).ToList();
            var delayAnnc =
                set.Where(
                    p =>  DelayAnncAlarmMatch(p.Annc,timeFormat)).ToList(); 

            if (alarmsAtEndTime.Any())
            {
                if(alarms.Any() && alarms.All(p => p.Time != timeFormat)) 
                alarms.AddRange(alarmsAtEndTime.Select(p => new AnncAlarmEntity()
                {
                    Id = p.Id,
                    Time = timeFormat, 
                    Annc = p,
                    CreatedAt = DateTime.Now
                }));
            }

            if (delayAnnc.Any())
            {
                if (alarms.Any() && alarms.All(p => p.Time != timeFormat))
                    alarms.AddRange(delayAnnc.Select(p => new AnncAlarmEntity()
                    {
                        Time = timeFormat,
                        Annc = new AnnouncementEntity()
                        {
                            Id = p.Annc.Id,
                            EndAt = timeFormat,
                            Creator = p.Annc.Creator,
                            Executors = p.Annc.Executors,
                            Inspecter = p.Annc.Inspecter,
                            Task = p.Annc.Task
                        },
                        Id = p.Id,
                        CreatedAt = DateTime.Now
                    }));
            }
            return alarms;
        }

        bool AnncAlarmMatch(AnnouncementEntity annc, int beforeStart, DateTime time)
        { 
            return  annc.EndAt.HasValue && annc.Executors.Any() && annc.Inspecter!=null &&
                annc.StartAt.HasValue && annc.StartAt.Value.AddMinutes(-beforeStart) == time;
        }

        bool DelayAnncAlarmMatch(AnnouncementEntity annc,DateTime time)
        {
            if (!annc.Reviews.Any()) return false;
            var lastReviews = annc.Reviews.OrderByDescending(p => p.CreatedAt).First();
            if (lastReviews.Reviewstatus != AnncStatus.Delay) return false;

            if(lastReviews.DelayAt.HasValue)
            return lastReviews.DelayAt.Value == time;
            return false;
        }

        public void UpdateAnncAlarmStatus(Guid anncAlarmId, bool isEnabled)
        {
            var alarm = AnncAlarmExistsResult.Check(this, anncAlarmId).ThrowIfFailed().AnncAlarm;
            alarm.IsEnabled = isEnabled;
            this.InternalUpdate(alarm);
        }

        public IEnumerable<AnncAlarmEntity> FetchAnncAlarmsByStaffId(Guid staffId)
        {  
         var alarms= this.InternalFetch(p =>!p.Annc.Reviews.Any(a=>a.Reviewstatus==AnncStatus.Approved || a.Reviewstatus==AnncStatus.Abandon) && p.Annc.Executors.Any(a=>a.Staff.Id==staffId) || p.Annc.Inspecter.Id == staffId)
                .ToList();
            var endAnncAlarms = this.InternalFetch(p => !p.Annc.Reviews.Any(a => a.Reviewstatus == AnncStatus.Approved || a.Reviewstatus == AnncStatus.Abandon) && (p.Annc.Creator.Id == staffId
                                                         || p.Annc.Inspecter.Id == staffId ||
                                                         p.Annc.Executors.Any(s => s.Id == staffId)) &&
                                                        p.Annc.EndAt <= DateTime.Now).ToList();

            alarms = alarms.Except(endAnncAlarms).ToList();
            if(endAnncAlarms.Any())
                alarms.AddRange(endAnncAlarms.Where(p=>p.Annc.EndAt.HasValue)
                    .Select(p=>new AnncAlarmEntity()
                {
                    Annc = p.Annc,
                    CreatedAt = p.Annc.EndAt.Value,
                    Time = p.Annc.EndAt
                }));

            var result= alarms.OrderByDescending(p=>p.CreatedAt).GroupBy(p=>p.Annc).Select(p=>p.First());
            return result;
        }


    }
}
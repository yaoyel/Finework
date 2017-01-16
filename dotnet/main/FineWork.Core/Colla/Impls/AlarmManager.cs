using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla.Checkers;
using System.Data.Entity;
using System.IO;
using FineWork.Colla.Models;
using FineWork.Common;
using FineWork.Core;
using FineWork.Files;

namespace FineWork.Colla.Impls
{
    public class AlarmManager : AefEntityManager<AlarmEntity, Guid>, IAlarmManager
    {
        public AlarmManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            ITaskAlarmManager taskAlarmManager,
            IPartakerManager partakerManager,
            IAlarmTempManager alarmTempManager,
            ILazyResolver<IPushLogManager> pushLogManagerResolver )
            : base(sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentException(nameof(sessionProvider));
            if (taskManager == null) throw new ArgumentException(nameof(TaskManager));
            if (taskAlarmManager == null) throw new ArgumentException(nameof(taskAlarmManager));

            m_TaskManager = taskManager;
            m_TaskAlarmManager = taskAlarmManager;
            m_PartakerManager = partakerManager;
            m_AlarmTempManager = alarmTempManager;
            m_PushLogManageResolver = pushLogManagerResolver;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly IPartakerManager m_PartakerManager;
        private readonly IAlarmTempManager m_AlarmTempManager;
        private readonly ILazyResolver<IPushLogManager> m_PushLogManageResolver;

        private IPushLogManager PushLogManager
        {
            get { return m_PushLogManageResolver.Required; }
        }

        public AlarmEntity CreateAlarmPeriodByTemp(Guid alarmTempId)
        {
            var alarmTemp =
                AlarmTempExistsResult.Check(this.m_AlarmTempManager, alarmTempId).ThrowIfFailed().AlarmPeriod;

            this.InternalInsert(alarmTemp);

            m_AlarmTempManager.DeleteAlarmTemps(alarmTempId);
            return alarmTemp;

        }

        public AlarmEntity CreateAlarmPeriod(CreateAlarmPeriodModel createAlarmPeriodModel)
        {
            if (createAlarmPeriodModel.ShortTime == null)
                throw new ArgumentException(nameof(createAlarmPeriodModel.ShortTime));

            var task =
                TaskExistsResult.Check(this.m_TaskManager, createAlarmPeriodModel.TaskId.Value).ThrowIfFailed().Task;

            var alarmPeriod = new AlarmEntity();
            alarmPeriod.Id = Guid.NewGuid();
            if (createAlarmPeriodModel.Weekdays.HasValue)
                alarmPeriod.Weekdays = createAlarmPeriodModel.Weekdays.Value;
            alarmPeriod.Content = createAlarmPeriodModel.Content;
            alarmPeriod.ShortTime = createAlarmPeriodModel.ShortTime;
            alarmPeriod.Task = task;
            alarmPeriod.Bell = createAlarmPeriodModel.Bell;
            alarmPeriod.IsEnabled = createAlarmPeriodModel.IsEnabled;
            alarmPeriod.IsRepeat = createAlarmPeriodModel.IsRepeat;
            alarmPeriod.DaysInMonth = createAlarmPeriodModel.DaysInMonth;
            alarmPeriod.NoRepeatTime = createAlarmPeriodModel.NoRepeatTime;
            alarmPeriod.TempletKind = createAlarmPeriodModel.TempletKind;
            alarmPeriod.AttSize = createAlarmPeriodModel.AttSize;

            if (createAlarmPeriodModel.ReceiverStaffIds != null && createAlarmPeriodModel.ReceiverStaffIds.Any())
            {
                alarmPeriod.ReceiverStaffIds = string.Join(",", createAlarmPeriodModel.ReceiverStaffIds);
            }
            else
            {
                if (createAlarmPeriodModel.ReceiverKinds != null && createAlarmPeriodModel.ReceiverKinds.Any())
                    alarmPeriod.ReceiverKinds = string.Join(",",
                        createAlarmPeriodModel.ReceiverKinds.OrderBy(p => p).Select(p => (int) p));
            }

            this.InternalInsert(alarmPeriod);

            return alarmPeriod;
        }

        public AlarmEntity UpdateAlarmPeriodTime(UpdateAlarmPeriodModel updateAlarmPeriodModel)
        {
            var alarmPeriod = AlarmExistsResult.Check(this, updateAlarmPeriodModel.AlarmId).ThrowIfFailed().AlarmPeriod;

            alarmPeriod.ShortTime = updateAlarmPeriodModel.ShortTime;
            if (updateAlarmPeriodModel.Weekdays.HasValue)
                alarmPeriod.Weekdays = updateAlarmPeriodModel.Weekdays.Value;
            alarmPeriod.Bell = updateAlarmPeriodModel.Bell;
            alarmPeriod.Content = updateAlarmPeriodModel.Content;
            alarmPeriod.DaysInMonth = updateAlarmPeriodModel.DaysInMonth;
            alarmPeriod.NoRepeatTime = updateAlarmPeriodModel.NoRepeatTime;
            alarmPeriod.IsRepeat = updateAlarmPeriodModel.IsRepeat;
            alarmPeriod.TempletKind = updateAlarmPeriodModel.TempletKind;
            alarmPeriod.AttSize = updateAlarmPeriodModel.AttSize;
            //更新接受者 
            if (updateAlarmPeriodModel.ReceiverStaffIds != null && updateAlarmPeriodModel.ReceiverStaffIds.Any())
            {
                alarmPeriod.ReceiverStaffIds = string.Join(",", updateAlarmPeriodModel.ReceiverStaffIds);
            }
            else
            {
                if (updateAlarmPeriodModel.ReceiverKinds != null && updateAlarmPeriodModel.ReceiverKinds.Any())
                    alarmPeriod.ReceiverKinds = string.Join(",",
                        updateAlarmPeriodModel.ReceiverKinds.OrderBy(p => p).Select(p => (int) p));
            }
            this.InternalUpdate(alarmPeriod);
            return alarmPeriod;
        }

        public AlarmEntity UpdateAlarmPeriodEnabled(Guid alarmPeriodId, bool isEnabled)
        {
            var alarmPeriod = AlarmExistsResult.Check(this, alarmPeriodId).ThrowIfFailed().AlarmPeriod;

            alarmPeriod.IsEnabled = isEnabled;
            this.InternalUpdate(alarmPeriod);
            return alarmPeriod;

        }

        public void DeleteAlarmPeriod(AlarmEntity alarmPeriod)
        {
            this.PushLogManager.DeletePushLogsByAlarm(alarmPeriod.Id);
            this.InternalDelete(alarmPeriod);
        }

        public void DeleteAlarmPeriodsByTaskId(Guid taskId)
        {
            var alarmPeriods = this.InternalFetch(p => p.Task.Id == taskId).ToList();

            if (alarmPeriods.Any())
                alarmPeriods.ForEach(InternalDelete);
        }

        public AlarmEntity FindAlarmPeriod(Guid alarmPeriodId)
        {
            return this.InternalFind(alarmPeriodId);
        }

        public IEnumerable<AlarmEntity> FetchAllAlarmPeriods()
        {
            return this.InternalFetchAll();
        }

        public IEnumerable<AlarmEntity> FetchAlarmPeriodsByTaskId(Guid taskId)
        {
            return this.InternalFetch(p => p.Task.Id == taskId
                                           && (p.Task.IsDeserted == null || p.Task.IsDeserted.Value == false))
                .OrderBy(p => p.ShortTime);
        }

        public IEnumerable<AlarmEntity> FetchAlarmPeriodsByTime(DateTime? date)
        {
            var context = this.Session.DbContext;
            var set = context.Set<AlarmEntity>().Include(p=>p.Task.Partakers.Select(s=>s.Staff.Org))
                .Include(p=>p.Task.Creator.Org)
                .Include(p => p.Task.Partakers.Select(s => s.Staff.Account)).AsNoTracking();

            //换算成东八区时间 
            TimeZoneInfo local = TimeZoneInfo.Local;
            var time = DateTime.Now.AddHours(8 - local.BaseUtcOffset.Hours);

            var timeOfDay = time.TimeOfDay;
            var timeOfMonth = time.Day;
            var timeFormat = new DateTime(time.Year, time.Month, time.Day, timeOfDay.Hours, timeOfDay.Minutes, 0);

            var dayOfWeek = date.HasValue
                ? (int) date.Value.DayOfWeek
                : (int) time.DayOfWeek;

            var shortTime = date?.ToShortTimeString() ??
                            string.Concat(timeOfDay.Hours.ToString().PadLeft(2, '0'), ":",
                                timeOfDay.Minutes.ToString().PadLeft(2, '0'));

            //转换成系统中存放的值 
            var weekDay = (int) Math.Pow(2, dayOfWeek);

            var repeatOnWeekAlarm = set.Where(p => p.Weekdays > 0 && p.IsRepeat && (weekDay & p.Weekdays) == weekDay
                                        && p.ShortTime == shortTime && p.Task.IsDeserted == null
                                        && p.Task.Progress < 100);

            var repeatOnMonthAlarm = set.Where(p => p.DaysInMonth != "" && p.IsRepeat && p.ShortTime == shortTime).ToList();

            if(repeatOnMonthAlarm.Any())
            repeatOnMonthAlarm =
                repeatOnMonthAlarm.Where(p =>!string.IsNullOrEmpty(p.DaysInMonth) && p.DaysInMonth.Split(',').Any(a => a == timeOfMonth.ToString())).ToList();

            var noRepeatAlarm = this.InternalFetch(p => p.IsRepeat == false).ToList();

            if(noRepeatAlarm.Any())
            noRepeatAlarm= noRepeatAlarm.Where(p =>    p.NoRepeatTime.HasValue &&p.NoRepeatTime.Value
            .ToShortDateString()== timeFormat.ToShortDateString() && p.ShortTime==timeFormat.ToShortTimeString()).ToList();   
 
     
            return repeatOnWeekAlarm.Union(repeatOnMonthAlarm).Union(noRepeatAlarm);
        }

        public IEnumerable<AlarmEntity> FetchUntreatedAlarmPeriodByStaff(Guid staffId)
        {
            //获取员工已经处理掉的最新的定时预警
            var treatedAlarms = m_TaskAlarmManager.FetchTaskAlarmsByCreatorId(staffId, true) 
                .OrderByDescending(p => p.CreatedAt);

            //查找应该处理的预警
            var alarmPeriods =
                this.InternalFetch(
                    p => p.Task.Progress < 100 &&
                         p.Task.IsDeserted == null && p.Task.Partakers.Count() > 1 &&
                         (p.Task.Partakers.Where(w => w.Staff.Id == staffId)
                             .Any(a => p.ReceiverKinds.Contains(((int) a.Kind).ToString())) ||
                          p.ReceiverStaffIds.Contains(staffId.ToString())))
                    .OrderByDescending(p => p.CreatedAt) 
                    .Where(AlarmPeriodShortTimeMatcher).ToList();

            var treatedAlarmPeriod = alarmPeriods.Join(treatedAlarms, u => u.Id, c => c.AlarmId, (u, c) => u).GroupBy(p=>p).Select(p=>p.First()).ToList();

            var result = alarmPeriods.Except(treatedAlarmPeriod).OrderByDescending(p => p.CreatedAt); 
            return result;
        }

        private bool AlarmPeriodShortTimeMatcher(AlarmEntity alarmPeriod)
        {
            if (!alarmPeriod.IsRepeat)
                return alarmPeriod.NoRepeatTime <= DateTime.Now; 

            IDictionary<string, DateTime> previousAndNextAlarmPeriod;
            if (alarmPeriod.IsRepeat && alarmPeriod.Weekdays > 0)
                previousAndNextAlarmPeriod = GetPreviousAndNextAlarmPeriodFromWeek(alarmPeriod);
            else
                previousAndNextAlarmPeriod = GetPreviousAndNextAlarmPeriodFromMonth(alarmPeriod);
            if (DateTime.Now >= previousAndNextAlarmPeriod["Previous"]
                && DateTime.Now <= previousAndNextAlarmPeriod["Next"]
                && alarmPeriod.CreatedAt < previousAndNextAlarmPeriod["Previous"]) 
            {
                return true;
            }

            return false;
        }

        private IDictionary<string, DateTime> GetPreviousAndNextAlarmPeriodFromWeek(AlarmEntity alarmPeriod)
        {
            var result = new Dictionary<string, DateTime>();
            var index = 0;
            var count = 0;
            var enumToShortList = new List<WeekDays>();
            var gap = 0;
            DateTime alarmPeriodTime;
            var weekdays = (WeekDays) Enum.Parse(typeof(WeekDays), alarmPeriod.Weekdays.ToString());

            weekdays.ToString().Split(',').ToList().ForEach(
                p => enumToShortList.Add((WeekDays) Enum.Parse(typeof(WeekDays), p)));


            var currentAlarmPeriod = (WeekDays) ((int) Math.Pow(2, (int) DateTime.Now.DayOfWeek));

            var shortTime = alarmPeriod.ShortTime.Split(':');

            if (enumToShortList.Contains(currentAlarmPeriod))
            {
                alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                    int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                index = enumToShortList.IndexOf(currentAlarmPeriod);
                count = enumToShortList.Count();

                if (alarmPeriodTime < DateTime.Now)
                {
                    result.Add("Previous", alarmPeriodTime);
                    if (enumToShortList.Count() == 1)
                        result.Add("Next", alarmPeriodTime.AddDays(7));
                    else
                    {
                        alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                        if (index == count - 1)
                        {
                            result.Add("Next",
                                alarmPeriodTime.AddDays((int.Parse(enumToShortList[0].GetOrder()) -
                                                         int.Parse(enumToShortList[index].GetOrder()) + 7)));
                        }
                        else
                        {
                            result.Add("Next",
                                alarmPeriodTime.AddDays(int.Parse(enumToShortList[index + 1].GetOrder()) -
                                                        int.Parse(enumToShortList[index].GetOrder())));
                        }
                    }

                }
                else
                {
                    result.Add("Next", alarmPeriodTime);
                    result.Add("Previous",
                        enumToShortList.Count() == 1
                            ? alarmPeriodTime.AddDays(-7)
                            : alarmPeriodTime.AddDays(int.Parse(enumToShortList[count - 1].GetOrder()) - 7));
                }
            }
            else
            {
                enumToShortList.Add(currentAlarmPeriod);
                enumToShortList = enumToShortList.OrderBy(p => p).ToList();
                index = enumToShortList.IndexOf(currentAlarmPeriod);
                count = enumToShortList.Count();

                if (index == 0)
                {
                    gap = int.Parse(enumToShortList[1].GetOrder()) -
                          int.Parse(enumToShortList[0].GetOrder());

                    alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                    alarmPeriodTime = alarmPeriodTime.AddDays(gap);

                    result.Add("Next", alarmPeriodTime);
                    var lastPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                        DateTime.Now.AddDays(int.Parse(enumToShortList[count - 1].GetOrder()) -
                                             int.Parse(enumToShortList[0].GetOrder())).Day, int.Parse(shortTime[0]),
                        int.Parse(shortTime[1]), 0);

                    result.Add("Previous", lastPeriodTime.AddDays(-7));
                }
                else
                {
                    gap = int.Parse(enumToShortList[index - 1].GetOrder()) -
                          int.Parse(enumToShortList[index].GetOrder());

                    alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                    alarmPeriodTime = alarmPeriodTime.AddDays(gap);
                    result.Add("Previous", alarmPeriodTime);

                    alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                    if (index == count - 1)
                    {
                        result.Add("Next",
                            alarmPeriodTime.AddDays((int.Parse(enumToShortList[0].GetOrder()) -
                                                     int.Parse(enumToShortList[index].GetOrder()) + 7)));
                    }
                    else
                    {
                        result.Add("Next",
                            alarmPeriodTime.AddDays(int.Parse(enumToShortList[index + 1].GetOrder()) -
                                                    int.Parse(enumToShortList[index].GetOrder())));
                    }
                }

            }

            return result;
        }

        private IDictionary<string, DateTime> GetPreviousAndNextAlarmPeriodFromMonth(AlarmEntity alarmPeriod)
        {
            var result = new Dictionary<string, DateTime>();
            var index = 0;
            var count = 0;
            var dayList = Array.ConvertAll(alarmPeriod.DaysInMonth.Split(',').ToArray(), int.Parse).ToList();
            var gap = 0;
            var shortTime = alarmPeriod.ShortTime.Split(':');
            DateTime alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);
            var currentDay = DateTime.Now.Day;

            if (dayList.Contains(currentDay))
            {
                index = dayList.IndexOf(currentDay);
                count = dayList.Count();

                if (alarmPeriodTime < DateTime.Now)
                {
                    result.Add("Previous", alarmPeriodTime);
                    if (dayList.Count() == 1)
                        result.Add("Next", alarmPeriodTime.AddMonths(1));
                    else
                    {
                        if (index == count - 1)
                        {
                            DateTime nextAarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, dayList[0],
                                int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0)
                                .AddMonths(1);
                            result.Add("Next", nextAarmPeriodTime);
                        }
                        else
                        {
                            DateTime nextAarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                dayList[index + 1], int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);
                            result.Add("Next", nextAarmPeriodTime);
                        }
                    }
                }
                else
                {
                    result.Add("Next", alarmPeriodTime);
                    if (index == 0)
                    {
                        DateTime previousAarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                            dayList[count - 1], int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0)
                            .AddMonths(-1);
                        result.Add("Previous", previousAarmPeriodTime);
                    }
                    else
                    {
                        DateTime previousAarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                            dayList[index - 1], int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);
                        result.Add("Previous", previousAarmPeriodTime);
                    }
                }
            }
            else
            {
                dayList.Add(currentDay);
                dayList = dayList.OrderBy(p => p).ToList();
                index = dayList.IndexOf(currentDay);
                count = dayList.Count();

                if (index == 0)
                {
                    gap = dayList[1] - dayList[0];

                    alarmPeriodTime = alarmPeriodTime.AddDays(gap);

                    result.Add("Next", alarmPeriodTime);
                    var nextPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, dayList[count - 1],
                        int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                    result.Add("Previous", nextPeriodTime.AddMonths(-1));
                }
                else
                {
                    gap = dayList[index - 1] - dayList[index];


                    alarmPeriodTime = alarmPeriodTime.AddDays(gap);
                    result.Add("Previous", alarmPeriodTime);

                    if (index == count - 1)
                    {
                        DateTime nextAarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, dayList[0],
                            int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0)
                            .AddMonths(1);
                        result.Add("Next", nextAarmPeriodTime);
                    }
                    else
                    {
                        DateTime nextAarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                            dayList[index + 1], int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);
                        result.Add("Next", nextAarmPeriodTime);
                    }
                }

            }

            return result;
        }

    }
}

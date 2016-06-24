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
using FineWork.Common;

namespace FineWork.Colla.Impls
{
    public class AlarmManager: AefEntityManager<AlarmEntity, Guid>, IAlarmManager
    { 
        public AlarmManager(ISessionProvider<AefSession> sessionProvider,
            ITaskManager taskManager,
            ITaskAlarmManager taskAlarmManager) 
            : base(sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentException(nameof(sessionProvider));
            if (taskManager == null) throw new ArgumentException(nameof(TaskManager));
            if (taskAlarmManager == null) throw new ArgumentException(nameof(taskAlarmManager));

            m_TaskManager = taskManager;
            m_TaskAlarmManager = taskAlarmManager;
        }

        private readonly ITaskManager m_TaskManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;

        public AlarmEntity CreateAlarmPeriod(TaskEntity task, int weekdays, string shortTime,string bell,bool isEnabled=true)
        {
            if (shortTime == null) throw new ArgumentException(nameof(shortTime));
            if (task == null) throw new ArgumentException(nameof(task)); 

            var alarmPeriod = new AlarmEntity();
            alarmPeriod.Id = Guid.NewGuid();
            alarmPeriod.Weekdays = weekdays;
            alarmPeriod.ShortTime = shortTime;
            alarmPeriod.Task = task;
            alarmPeriod.Bell = bell;
            alarmPeriod.IsEnabled = isEnabled;

            this.InternalInsert(alarmPeriod);
            return alarmPeriod; 
        }

        public AlarmEntity UpdateAlarmPeriodTime(Guid alarmPeriodId,int weekdays, string shortTime,string bell)
        { 
            var alarmPeriod = AlarmExistsResult.Check(this, alarmPeriodId).ThrowIfFailed().AlarmPeriod;
             
            alarmPeriod.ShortTime = shortTime;
            alarmPeriod.Weekdays = weekdays;
            alarmPeriod.Bell = bell;
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
            this.InternalDelete(alarmPeriod);
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
            return this.InternalFetch(p => p.Task.Id == taskId).OrderByDescending(p => p.ShortTime);
        }

        public IEnumerable<AlarmEntity> FetchAlarmPeriodsByDate(DateTime? date)
        {
            //换算成东八区时间 
            TimeZoneInfo local = TimeZoneInfo.Local;
            var time=DateTime.Now.AddHours(8 - local.BaseUtcOffset.Hours); 
   
            var timeOfDay = time.TimeOfDay;

            var dayOfWeek = date.HasValue
                ? (int) date.Value.DayOfWeek
                : (int)time.DayOfWeek;

            var shortTime = date?.ToShortTimeString() ?? string.Concat(timeOfDay.Hours.ToString().PadLeft(2,'0'), ":", timeOfDay.Minutes.ToString().PadLeft(2,'0'));

            //转换成系统中存放的值 
            var weekDay = (int)Math.Pow(2, dayOfWeek); 

            return this.InternalFetch(p => (weekDay & p.Weekdays) == weekDay
                                           && p.ShortTime == shortTime);
        }

        public IEnumerable<AlarmEntity> FetchUntreatedAlarmPeriodByStaff(Guid staffId)
        {
            //获取员工已经处理掉的最新的定时预警
            var treatedAlarms = m_TaskAlarmManager.FetchTaskAlarmsByStaffId(staffId, true)
                .OrderByDescending(p => p.CreatedAt)
                .GroupBy(p => p.Task);

            var treatedAlarm = treatedAlarms.Select(p => p.First()).ToList();

            //查找应该处理的预警
            var alarmPeriods = this.InternalFetch(p => p.Task.Partakers.Count()>1 && p.Task.Partakers.Any(s => s.Staff.Id == staffId))
                .OrderByDescending(p=>p.CreatedAt) 
                .GroupBy(p => p.Task).Select(p => p.First())
                .Where(AlarmPeriodShortTimeMatcher).ToList(); 
     

            var alarmPeriod = alarmPeriods.Join(treatedAlarm, u => u.Task, c => c.Task, (u, c) =>
            {
                var previousAndNextAlarmPeriod = GetPreviousAndNextAlarmPeriod(u);
                if (c.CreatedAt>=previousAndNextAlarmPeriod["Previous"]
                 && c.CreatedAt <= previousAndNextAlarmPeriod["Next"]) 
                return null;
                return u;
            }).ToList();

            var result=alarmPeriod.Where(p=>p!=null).Union(
                alarmPeriods.Where(
                    p => alarmPeriods.Select(a => a.Task).Except(treatedAlarm.Select(t => t.Task)).Contains(p.Task)))
                    .OrderByDescending(p=>p.CreatedAt);

            return result;
        }

        private bool AlarmPeriodShortTimeMatcher(AlarmEntity alarmPeriod)
        {  
            var previousAndNextAlarmPeriod = GetPreviousAndNextAlarmPeriod(alarmPeriod);  

            if (DateTime.Now >= previousAndNextAlarmPeriod["Previous"]
                && DateTime.Now <= previousAndNextAlarmPeriod["Next"]
                && alarmPeriod.CreatedAt < previousAndNextAlarmPeriod["Previous"])
                 
            {
                return true;
            }

            return false;
        }

        private IDictionary<string, DateTime> GetPreviousAndNextAlarmPeriod(AlarmEntity alarmPeriod)
        {
            var result = new Dictionary<string, DateTime>();
            var index = 0;
            var count = 0;
            var enumToShortList = new List<WeekDays>();
            var gap = 0;
            DateTime alarmPeriodTime;
            var weekdays = (WeekDays) Enum.Parse(typeof (WeekDays), alarmPeriod.Weekdays.ToString());
            weekdays.ToString().Split(',').ToList().ForEach(
                p => enumToShortList.Add((WeekDays)Enum.Parse(typeof (WeekDays), p)));

            var currentAlarmPeriod = (WeekDays)( (int)Math.Pow(2, (int)DateTime.Now.DayOfWeek)); 

            var shortTime = alarmPeriod.ShortTime.Split(':'); 

            if (enumToShortList.Contains(currentAlarmPeriod))
            {
                alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);
                 

                index = enumToShortList.IndexOf(currentAlarmPeriod);
                count = enumToShortList.Count();

                if (alarmPeriodTime < DateTime.Now)
                {
                    result.Add("Previous", alarmPeriodTime);
                    if (enumToShortList.Count() == 1)
                        result.Add("Next", alarmPeriodTime.AddDays(7));
                    else
                    {
                        alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

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
                    var lastPeriodTime= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(int.Parse(enumToShortList[count - 1].GetOrder()) - int.Parse(enumToShortList[0].GetOrder())).Day, int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                    result.Add("Previous", lastPeriodTime.AddDays(- 7));
                }
                else
                {
                    gap = int.Parse(enumToShortList[index - 1].GetOrder()) -
                     int.Parse(enumToShortList[index].GetOrder());

                    alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                        int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

                    alarmPeriodTime = alarmPeriodTime.AddDays(gap);
                    result.Add("Previous", alarmPeriodTime);

                    alarmPeriodTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(shortTime[0]), int.Parse(shortTime[1]), 0);

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

    }
}

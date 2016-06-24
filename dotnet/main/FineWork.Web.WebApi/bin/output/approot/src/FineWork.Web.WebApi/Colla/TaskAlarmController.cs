using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos.Ambients;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using AppBoot.Checks;
namespace FineWork.Web.WebApi.Colla
{
    [Route("api/TaskAlarms")]
    [Authorize("Bearer")]
    public class TaskAlarmController : FwApiController
    {
        public TaskAlarmController(ISessionScopeFactory sessionScopeFactory,
            IAlarmPeriodManager alarmPeriodManager,
            ITaskAlarmManager taskAlarmManager,
            IStaffManager staffManager)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentNullException(nameof(sessionScopeFactory));
            if (alarmPeriodManager == null) throw new ArgumentNullException(nameof(alarmPeriodManager));
            if (staffManager == null) throw new ArgumentNullException(nameof(staffManager));

            m_AlarmPeriodManager = alarmPeriodManager;
            m_TaskAlarmManager = taskAlarmManager;
            m_StaffManager = staffManager;
        }

        private readonly IAlarmPeriodManager m_AlarmPeriodManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;
        private readonly IStaffManager m_StaffManager;

        [HttpPost("CreateAlarmPeriod")]
        [DataScoped(true)]
        public AlarmPeriodViewModel CreateAlarmPeriod(Guid taskId, int weekdays, string shortTime)
        {
            if (string.IsNullOrEmpty(shortTime)) throw new ArgumentException(nameof(shortTime));

            var alarmPeriod=this.m_AlarmPeriodManager.CreateAlarmPeriod(taskId, weekdays, shortTime);
            return alarmPeriod.ToViewModel();
        }

        [HttpPost("DeleteAlarmPeriod")]
        [DataScoped(true)]
        public void DeleteAlarmPeriod(Guid alarmPeriodId)
        {
            var alarmPeriod = this.m_AlarmPeriodManager.FindAlarmPeriod(alarmPeriodId);
             
            this.m_AlarmPeriodManager.DeleteAlarmPeriod(alarmPeriod);
        }

        [HttpPost("UpdateAlarmPeriodTime")]
        [DataScoped(true)] 
        public AlarmPeriodViewModel UpdateAlarmPeriodTime(Guid alarmPeriodId, bool isEnabled, int weekdays, string shortTime)
        {

            if (string.IsNullOrEmpty(shortTime)) throw new ArgumentException(nameof(shortTime));
            if (weekdays<=0) throw new ArgumentException(nameof(weekdays));

            var alarmPeriod = this.m_AlarmPeriodManager.UpdateAlarmPeriodTime(alarmPeriodId, weekdays, shortTime);
            return alarmPeriod.ToViewModel();
        }

        [HttpPost("UpdateAlarmPeriodEnabled")]
        [DataScoped(true)]
        public AlarmPeriodViewModel UpdateAlarmPeriodEnabled(Guid alarmPeriodId, bool isEnabled)
        {  
            var alarmPeriod = this.m_AlarmPeriodManager.UpdateAlarmPeriodEnabled(alarmPeriodId,isEnabled);
            return alarmPeriod.ToViewModel();
        }


        [HttpGet("FetchAlarmPeriodByTaskId")] 
        public IActionResult FetchAlarmPeriodByTaskId(Guid taskId)
        {
            var alarmPeriods = this.m_AlarmPeriodManager.FetchAlarmPeriodsByTaskId(taskId);

            return alarmPeriods != null
                ? new ObjectResult(alarmPeriods.Select(p => p.ToViewModel()))
                : new HttpNotFoundObjectResult(taskId);
        }

        [HttpGet("FetchUntreatedAlarmPeriodByStaff")]
        public IActionResult FetchUntreatedAlarmPeriodByStaff(Guid staffId)
        {
            var result = this.m_AlarmPeriodManager.FetchUntreatedAlarmPeriodByStaff(staffId).ToList();
            if (result.Any()) return new ObjectResult(result.Select(p=>p.ToViewModel()));
            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("FindAlarmPeriod")] 
        public IActionResult FindAlarmPeriod(Guid alarmPeriodId)
        {
            var alarmPeriod = this.m_AlarmPeriodManager.FindAlarmPeriod(alarmPeriodId);

            return alarmPeriod != null
                ? new ObjectResult(alarmPeriod.ToViewModel())
                : new HttpNotFoundObjectResult(alarmPeriodId);
        }

        [HttpPost("CreateTaskAlarm")]
        [DataScoped(true)]
        public Tuple <TaskAlarmViewModel,string> CreateTaskAlarm(Guid taskId, Guid staffId, TaskAlarmKinds alarmKind)
        {
            var alarm = m_TaskAlarmManager.CreateTaskAlarm(taskId, staffId, alarmKind);
            return new Tuple<TaskAlarmViewModel, string>(alarm.Item1.ToViewModel(), alarm.Item2);
        }

        [HttpPost("ChangeResolvedStatus")]
        [DataScoped(true)]
        public TaskAlarmViewModel ChangeResolvedStatus(Guid taskAlarmId, string comment)
        {   
            var alarm = m_TaskAlarmManager.ChangeResolvedStatus(taskAlarmId,this.AccountId, comment);
            return alarm.ToViewModel();
        }

        [HttpGet("FetchAlarmsByResolvedStatus")]
        public StaffAlarmsViewModel FetchAlarmsByResolvedStatus(Guid taskId, Guid staffId, bool isResolved)
        { 
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByResolvedStatus(taskId, staffId,isResolved); 
            return alarms.ToViewModel();
        }

        [HttpGet("FetchAlarmsByStaffGroupByKind")]
        public IActionResult FetchAlarmsByStaffGroupByKind(Guid staffId)
        {
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByStaffId(staffId).ToList();
            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId);

          
            return new ObjectResult(alarms.ToViewModelGroupByKind());
        }

        [HttpGet("FetchAlarmsByStaffGroupByTask")]
        public IActionResult FetchAlarmsByStaffGroupByTask(Guid staffId)
        {
            var alarms = m_TaskAlarmManager.FetchTaskAlarmsByStaffId(staffId).ToList();
            if (!alarms.Any()) return new HttpNotFoundObjectResult(staffId); 

            return new ObjectResult(alarms.ToViewModelGroupByTask());
        }

        [HttpGet("FetchAlarmsWithPartakerByStaffAndKind")]
        public IActionResult FetchAlarmsWithPartakerByStaffAndKind(Guid staffId, TaskAlarmKinds? alarmKind)
        {
            var alarms = m_TaskAlarmManager.FetchAlarmsWithPartakerByStaffAndKind(staffId,alarmKind).ToList();
            if (!alarms.Any()) return new HttpNotFoundObjectResult($"{staffId},{alarmKind}");

            return new ObjectResult(alarms.ToViewModelWithPartakers());
        }

        public IActionResult UpdateTaskAlarmContent(Guid taskAlarmId, string content)
        {
            Args.NotEmpty(content, nameof(content));
            var taskAlarm = TaskAlarmExistsResult.Check(this.m_TaskAlarmManager, taskAlarmId).ThrowIfFailed().TaskAlarm;
            taskAlarm.Content = content;
            this.m_TaskAlarmManager.UpdateTaskAlarm(taskAlarm);
            return new HttpStatusCodeResult(200);
        }
    }
}

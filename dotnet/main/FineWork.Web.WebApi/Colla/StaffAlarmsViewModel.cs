using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Colla.Models;
using FineWork.Web.WebApi.Security;

namespace FineWork.Web.WebApi.Colla
{

    #region StaffAlarmsViewModel
 
    public class StaffAlarmsViewModel
    {
        public List<TaskAlarmViewModel> SendOuts { get; set; }

        public List<TaskAlarmViewModel> Receiveds { get; set; }

        public List<TaskAlarmViewModel> Others { get; set; }

        public virtual void AssignFrom(StaffAlarmsModel model, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            SendOuts = model.SendOuts?.Select(p => p.ToViewModel(isShowhighOnly,isShowLow)).ToList();
            Receiveds = model.Receiveds?.Select(p => p.ToViewModel(isShowhighOnly, isShowLow)).ToList();
            Others = model.Others?.Select(p => p.ToViewModel(isShowhighOnly, isShowLow)).ToList();
        }
    }

    public static class StaffAlarmsViewModelExtensions
    {
        public static StaffAlarmsViewModel ToViewModel(this StaffAlarmsModel model)
        {
            var result = new StaffAlarmsViewModel();
            result.AssignFrom(model);
            return result;
        }

    }

    #endregion

    #region StaffAlarmsGroupByKindViewModel
    public class StaffAlarmsGroupByKindViewModel
    {
        public TaskAlarmKinds TaskAlarmKindId { get; set; }

        public string TaskAlarmKindName { get; set; }

        public IList<TaskAlarmViewModel> TaskAlarms { get; set; }

        public virtual IList<StaffAlarmsGroupByKindViewModel> AssignFrom(IEnumerable<TaskAlarmEntity> alarms)
        {
            if (alarms == null) throw new ArgumentNullException(nameof(alarms));

            var alarmsGroupByKind = alarms.GroupBy(p => p.TaskAlarmKind)
                .OrderByDescending(p=>p.Count())
                .Select(p => new StaffAlarmsGroupByKindViewModel()
                {
                    TaskAlarmKindId = p.Key,
                    TaskAlarmKindName = p.Key.GetLabel(),
                    TaskAlarms = p.Where(w=>w.Staff.IsEnabled).Select(s => s.ToViewModel()).ToList()
                });

            return alarmsGroupByKind.Where(p=>p.TaskAlarms.Any()).ToList();
        }
    } 

    public static class StaffAlarmsGroupByKindViewModelExtensions
    {
        public static IList<StaffAlarmsGroupByKindViewModel> ToViewModelGroupByKind(
            this IEnumerable<TaskAlarmEntity> model)
        {
            var result = new StaffAlarmsGroupByKindViewModel();

            return result.AssignFrom(model);
        }

    }

    #endregion

    #region TaskAlarmsGroupByTaskViewModel
    public class TaskAlarmsGroupByTaskViewModel
    {
        public TaskViewModel Task { get; set; }

        public IList<StaffViewModel> PartakerStaffs { get; set; }

        public IList<TaskAlarmViewModel> TaskAlarms { get; set; }

        public virtual IList<TaskAlarmsGroupByTaskViewModel> AssignFrom(IList<TaskAlarmEntity> alarms)
        {
            if (alarms == null) throw new ArgumentNullException(nameof(alarms)); 

            var alarmsGroupByKind = alarms.GroupBy(p => p.Task)
                .OrderByDescending(p => p.Count())
                .Select(p => new TaskAlarmsGroupByTaskViewModel()
                {
                    Task = p.Key.ToViewModel(),
                    PartakerStaffs = p.Key.Partakers.Where(w => w.Staff.Alarms.Any(a => a.ResolveStatus!=ResolveStatus.Closed 
                     &&a.Task.Id==p.Key.Id && a.TaskAlarmKind!=TaskAlarmKinds.GreenLight) && w.Staff.IsEnabled)
                    .GroupBy(g => g.Staff).Select(s => s.Key.ToViewModel()).ToList(),

                    TaskAlarms =alarms.Where(s=>s.Task==p.Key && s.Staff.IsEnabled).Select(s=>s.ToViewModel()).ToList()
                });

            return alarmsGroupByKind.ToList();
        }
    }

    public static class TaskAlarmsGroupByTaskViewModelExtensions
    {
        public static IList<TaskAlarmsGroupByTaskViewModel> ToViewModelGroupByTask(
            this IEnumerable<TaskAlarmEntity> model)
        {
            var result = new TaskAlarmsGroupByTaskViewModel();

            return result.AssignFrom(model.ToList());
        }

    }
    #endregion

    #region TaskAlarmWithPartakersViewModel

    public class TaskAlarmsWithPartakersViewModel
    {
        public TaskViewModel Task { get; set; }

        public IList<TaskAlarmWithPartaker> TaskAlarmsWithPartakers { get; set; }

        public IList<TaskAlarmsWithPartakersViewModel>  AssignFrom(IList<TaskAlarmEntity> alarm)
        {
            if (!alarm.Any()) throw new ArgumentNullException(nameof(alarm));

            var tasks = alarm.Select(p => p.Task).Distinct().ToArray(); 
             
            var result = tasks.Select(p => new TaskAlarmsWithPartakersViewModel()
            {
                Task = p.ToViewModel(),
                TaskAlarmsWithPartakers = p.Partakers
                    .Select(x => new TaskAlarmWithPartaker()
                    {
                        Partaker = x.ToViewModel(),
                        PartakerStaff = x.Staff.ToViewModel(),
                        TaskAlarms = alarm.Where(w => w.Staff == x.Staff && w.Task==p) 
                            .Select(u => u.ToViewModel()).ToList()
                    }).ToList()
            });

            //var alarms = result.Select(p => p.TaskAlarmsWithPartakers)
            //    .SelectMany(p => p.Select(s => s.TaskAlarms))
            //    .SelectMany(p => p).Select(p=>p.Id).ToArray();
            //var dddd = alarm.Where(p => !alarms.Contains(p.Id)).ToList();

            return result.ToList();
        }
    }


    public class TaskAlarmWithPartaker
    {  
        public StaffViewModel PartakerStaff { get; set; }

        public PartakerViewModel Partaker { get; set; }

        public IList<TaskAlarmViewModel> TaskAlarms { get; set; }
    }


    public static class TaskAlarmsWithPartakersViewModelExtensions
    {
        public static IList<TaskAlarmsWithPartakersViewModel> ToViewModelWithPartakers(
            this IEnumerable<TaskAlarmEntity> model)
        {
            var result = new TaskAlarmsWithPartakersViewModel();

            return result.AssignFrom(model.ToList());
        }

    }
    #endregion

}

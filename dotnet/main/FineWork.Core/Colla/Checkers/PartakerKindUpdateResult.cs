using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla
{
    public class AlarmOrVoteExistsResult:FineWorkCheckResult
    {
        public AlarmOrVoteExistsResult(bool isSucceed, String message)
            : base(isSucceed, message)
        { 
        }

        public static AlarmOrVoteExistsResult Check(PartakerEntity partaker,Guid taskId,ITaskAlarmManager taskAlarmManager)
        {
            var alarmsAsCreateor =
                taskAlarmManager.FetchTaskAlarmsByStaffId(partaker.Staff.Id).Where(p => p.Task.Id == taskId && p.ResolveStatus!=ResolveStatus.Closed);
            var alarmsForPartakerKind = taskAlarmManager.FetchAlarmsByPartakerKind(taskId,partaker.Kind).Where(p=>p.ResolveStatus!=ResolveStatus.Closed);
      

            if(alarmsAsCreateor.Any() || alarmsForPartakerKind.Any())
                return Check($"{partaker.Staff.Name}存在未处理的预警.");

            if (partaker.Task.TaskVotes.Any(p=>p.Task.Id==taskId && p.Vote.IsApproved==null))
                return Check($"{partaker.Staff.Name}存在未处理的共识.");
            return new AlarmOrVoteExistsResult(true, null);
        }

        public static AlarmOrVoteExistsResult Check(ITaskManager taskManager, Guid taskId)
        {
            Args.NotNull(taskManager, nameof(taskManager));

            var task = taskManager.FindTask(taskId);
            if(task.Alarms.Any(p=>p.ResolveStatus!=ResolveStatus.Closed))
                return Check("该任务存在未处理的预警.");
            if(task.TaskVotes.Any(p=>p.Vote.IsApproved==null))
                return Check("该任务存在未处理的共识.");

            return new AlarmOrVoteExistsResult(true, null);
        }

        public static AlarmOrVoteExistsResult Check(IPartakerManager partakerManager,Guid staffId)
        {
            var partakers = partakerManager.FetchPartakersByStaff(staffId).ToList();
            if (partakers.Any(p=>p.Staff.Alarms.Any(a=> a.ResolveStatus != ResolveStatus.Closed)))
                return Check("该员工存在未处理的预警.");
            if (partakers.Any(p=>p.Staff.Votes.Any(a=> a.IsApproved == null)))
                return Check("该员工存在未处理的共识.");
            return new AlarmOrVoteExistsResult(true, null);
        }
         

        private static AlarmOrVoteExistsResult Check( String message)
        { 
            return new AlarmOrVoteExistsResult(false, message); 
        }

    }
}

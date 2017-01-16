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
    public class PartakerKindUpdateResult: FineWorkCheckResult
    {
        public PartakerKindUpdateResult(bool isSucceed, String message)
            : base(isSucceed, message)
        { 
        }

        public static PartakerKindUpdateResult Check(PartakerEntity partaker,Guid taskId,ITaskAlarmManager taskAlarmManager,bool checkAlarm=false)
        {
            var alarmsAsCreateor =
                taskAlarmManager.FetchTaskAlarmsByCreatorId(partaker.Staff.Id).Where(p => p.Task.Id == taskId && p.ResolveStatus!=ResolveStatus.Closed);

            if (checkAlarm)
            {
                var alarmsForPartakerKind = taskAlarmManager.FetchTaskAlarmsByStaffIdWithTaskId(taskId, partaker.Staff.Id).Where(p => p.ResolveStatus != ResolveStatus.Closed);
                if (alarmsAsCreateor.Any() || alarmsForPartakerKind.Any())
                    return Check($"{partaker.Staff.Name}存在未处理的预警.");
            }

            if (partaker.Task.TaskVotes.Any(p=>p.Vote.Creator.Id==partaker.Staff.Id && p.Vote.IsApproved==null))
                return Check($"{partaker.Staff.Name}存在未处理的共识.");

            var anncs =
                partaker.Task.Announcements.Where(
                    p => p.Executors.Any(a => a.Staff.Id == partaker.Staff.Id) || p.Inspecter.Id == partaker.Staff.Id)
                    .ToList();
            if (anncs.Any() && !anncs.Any(p => p.Reviews.Any(a=>a.Reviewstatus==AnncStatus.Approved)))
                return Check($"{partaker.Staff.Name}存在未处理的计划.");

            return new PartakerKindUpdateResult(true, null);
        }

        public static PartakerKindUpdateResult Check(ITaskManager taskManager, Guid taskId)
        {
            Args.NotNull(taskManager, nameof(taskManager));

            var task = taskManager.FindTask(taskId);
            if(task.Alarms.Any(p=>p.ResolveStatus!=ResolveStatus.Closed))
                return Check("该任务存在未处理的预警.");
            if(task.TaskVotes.Any(p=>p.Vote.IsApproved==null))
                return Check("该任务存在未处理的共识.");

            //var anncs = task.Announcements;
            //if (anncs.Any() && !anncs.Any(p => p.Reviews.Any()))
            //    return Check($"该任务存在未处理的计划.");

            return new PartakerKindUpdateResult(true, null);
        }
    
        public static PartakerKindUpdateResult Check(IPartakerManager partakerManager,Guid staffId)
        {
            var partakers = partakerManager.FetchPartakersByStaff(staffId).ToList();
            if (partakers.Any(p=>p.Staff.Alarms.Any(a=> a.ResolveStatus != ResolveStatus.Closed)))
                return Check("该员工存在未处理的预警.");
            if (partakers.Any(p=>p.Staff.Votes.Any(a=> a.IsApproved == null)))
                return Check("该员工存在未处理的共识.");

            var anncs = partakers.Select(p => p.Task.Announcements).SelectMany(p => p)
                .Where(p => p.Inspecter.Id == staffId || p.Executors.Any(a => a.Staff.Id == staffId)).ToList();

            if (anncs.Any() && !anncs.Any(p => p.Reviews.Any(a => a.Reviewstatus == AnncStatus.Approved)))
                return Check("该员工存在未处理的计划.");

            return new PartakerKindUpdateResult(true, null);
        }
         

        private static PartakerKindUpdateResult Check( String message)
        { 
            return new PartakerKindUpdateResult(false, message); 
        }

    }
}

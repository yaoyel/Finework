using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla
{
    public class PartakerKindUpdateResult:FineWorkCheckResult
    {
        public PartakerKindUpdateResult(bool isSucceed, String message)
            : base(isSucceed, message)
        { 
        }

        public static PartakerKindUpdateResult Check(PartakerEntity partaker,Guid taskId,ITaskAlarmManager taskAlarmManager)
        {
            var alarmsAsCreateor =
                taskAlarmManager.FetchTaskAlarmsByStaffId(partaker.Staff.Id).Where(p => p.Task.Id == taskId && p.ResolveStatus!=ResolveStatus.Closed);
            var alarmsForPartakerKind = taskAlarmManager.FetchAlarmsByPartakerKind(taskId,partaker.Kind).Where(p=>p.ResolveStatus!=ResolveStatus.Closed);
      

            if(alarmsAsCreateor.Any() || alarmsForPartakerKind.Any())
                return Check($"{partaker.Staff.Name}存在未处理的预警.");

            if (partaker.Staff.Votes.Any(p=>p.Task.Id==taskId && p.IsApproved==null))
                return Check($"{partaker.Staff.Name}存在未处理的共识.");
            return new PartakerKindUpdateResult(true, null);
        }

        public static PartakerKindUpdateResult Check(IPartakerManager partakerManager,Guid staffId)
        {
            var partakers = partakerManager.FetchPartakersByStaff(staffId).ToList();
            if (partakers.Any(p=>p.Staff.Alarms.Any(a=> a.ResolveStatus != ResolveStatus.Closed)))
                return Check("该员工存在未处理的预警.");
            if (partakers.Any(p=>p.Staff.Votes.Any(a=> a.IsApproved == null)))
                return Check("该员工存在未处理的共识.");
            return new PartakerKindUpdateResult(true, null);
        }
         

        private static PartakerKindUpdateResult Check( String message)
        { 
            return new PartakerKindUpdateResult(false, message); 
        }

    }
}

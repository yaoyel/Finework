using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
   public  interface IMomentManager
   {
       MomentEntity CreateMoment(CreateMomentModel momentModel);

       MomentEntity FindMomentById(Guid momentId);

       IEnumerable<MomentEntity> FetchMomentsByOrgId(Guid orgId);

       IEnumerable<MomentEntity> FetchMomentsByStaffId(Guid staffId);

       void DeleteMoment(Guid momentId);

      Task SendMomentMessageAsync(StaffEntity staff, MomentEntity moment);
   }
}

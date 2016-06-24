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
       MomentEntity CreateMement(CreateMomentModel mementModel);

       MomentEntity FindMementById(Guid mementId);

       IEnumerable<MomentEntity> FetchMementsByOrgId(Guid orgId);

       IEnumerable<MomentEntity> FetchMementsByStaffId(Guid staffId);

       void DeleteMement(Guid mementId);
   }
}

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
       IEnumerable<MomentEntity> FetchMomentsByContent(Guid orgId, string content);

        IEnumerable<MomentEntity> FetchMomentsByStaffId(Guid staffId);

       void DeleteMoment(Guid momentId);

      Task SendMomentMessageAsync(MomentEntity moment,string from,params string[] phoneNumbers);

       bool HasUnReadMoment(Guid staffId);

       Tuple<int, Guid?> FetchUnReadCommentCountByStaffId(Guid staffId);


   }
}

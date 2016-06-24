using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
   public interface IMomentLikeManager
   {
       MomentLikeEntity CreateMomentLike(Guid momentId, Guid staffId);

       void DeleteMomentLikeById(Guid momentLikeId);

       void DeleteMomentLikeByMomentId(Guid momentId);

       MomentLikeEntity FindMomentLikeById(Guid momentLikeId);

       MomentLikeEntity FindMomentLikeByStaffId(Guid momentId, Guid staffId);

       IEnumerable<MomentLikeEntity> FetchMomentLikeByStaffId(Guid staffId);



   }
}

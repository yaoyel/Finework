using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Colla.Models;

namespace FineWork.Colla
{
    public interface IMomentCommentManager
    {
        MomentCommentEntity CreateMomentComment(CreateMomentCommetModel createMomentCommetModel);

        void DeleteMomentCommentById(Guid commentId);

        void DeleteMomentCommentByMomentId(Guid momentId);

        MomentCommentEntity FineMomentCommentById(Guid momentCommentId);

        IEnumerable<MomentCommentEntity> FetchCommentByStaffId(Guid staffId);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IAccessTimeManager
    {
        AccessTimeEntity FindAccessTimeByStaffId(Guid staffId);

        AccessTimeEntity FindAccessTimeById(Guid accessTimeId);

        AccessTimeEntity CreateAccessTimeEntity(Guid staffId,DateTime? lastEntryOrgAt,DateTime?lastViewMomentAt,DateTime? lastViewCommentAt,DateTime? lastViewNewsAt);

        void UpdateLastVoewMomentTime( Guid staffId, DateTime lastViewMomentAt);

        void UpdateLastEnterOrgTime( Guid staffId, DateTime lastEntryOrgAt);

        void UpdateLastViewCommentTime( Guid staffId, DateTime lastViewCommentAt);

        void UpdateLastViewNewsTime(Guid staffId, DateTime lastViewNewsAt);


    }
}

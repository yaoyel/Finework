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

        AccessTimeEntity CreateAccessTimeEntity(Guid staffId, DateTime? lastEntryOrgAt, DateTime? lastViewMomentAt,
            DateTime? lastViewCommentAt, DateTime? lastViewNewsAt, DateTime? lastViewForumAt,
            DateTime? lastViewForumCommentAt,
            DateTime? lastViewMissinAt, DateTime? lastViewVisionAt, DateTime? lastViewValuesAt,
            DateTime? lastViewStrategyAt, DateTime? lastViewOrgGovernanceAt);

        void UpdateLastVoewMomentTime(Guid staffId, DateTime lastViewMomentAt);

        void UpdateLastEnterOrgTime(Guid staffId, DateTime lastEntryOrgAt);

        void UpdateLastViewCommentTime(Guid staffId, DateTime lastViewCommentAt);

        void UpdateLastViewNewsTime(Guid staffId, DateTime lastViewNewsAt);

        void UpdateLastViewForumTime(Guid staffId, DateTime lastViewForumAt);

        void UpdateLastViewMissinTime(Guid staffId, DateTime lastViewMissinAt);

        void UpdateLastViewVisionTime(Guid staffId, DateTime lastViewVisionAt);

        void UpdateLastViewValuesTime(Guid staffId, DateTime lastViewValuesAt);

        void UpdateLastViewStrategyTime(Guid staffId, DateTime lastViewStrategyAt);

        void UpdateLastViewOrgGovernanceTime(Guid staffId, DateTime lastViewOrgGovernanceAt); 

        void UpdateLastViewForumCommentTime(Guid staffId, DateTime lastViewForumCommentAt);
    }
}

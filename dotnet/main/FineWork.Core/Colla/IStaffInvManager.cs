using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Core.Colla.Models;
using FineWork.Security;

namespace FineWork.Colla
{

    public interface IStaffInvManager
    {
        void BulkCreateStuffInv(CreateStaffInvModel invStaff);

        StaffInvEntity CreateStaffInv(string phoneNumber, string staffName, string inviterName,Guid orgId);

        StaffInvEntity FindStaffInvByOrgAccount(Guid orgId,Guid accountId);

        StaffInvEntity FindStaffInvByOrgWithPhoneNumber(Guid orgId, string phoneNumber);

        ICollection<StaffInvEntity> FetchStaffInvsByAccount(Guid accountId);

        void ChangeReviewStatus(StaffInvEntity staffInv, ReviewStatuses newRevStatus); 

        StaffInvEntity FindStaffInvById(Guid staffInvId);
    }
}

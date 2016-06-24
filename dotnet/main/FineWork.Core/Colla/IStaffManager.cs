using System;
using System.Collections.Generic;

namespace FineWork.Colla
{
    public interface IStaffManager
    {
        StaffEntity CreateStaff(Guid orgId, Guid accountId, string newName);

        StaffEntity FindStaff(Guid staffId);

        IList<StaffEntity> FetchStaffsByIds(params Guid[] staffIds);

        StaffEntity FindStaffByNameInOrg(Guid orgId, String staffName);

        StaffEntity FindStaffByOrgAccount(Guid orgId, Guid accountId);

        IList<StaffEntity> FetchStaffsByOrg(Guid orgId,bool? isEnabled);

        IList<StaffEntity> FetchStaffsByAccount(Guid accountId); 

        IEnumerable<StaffEntity> FetchAllStaff();

        void UpdateStaff(StaffEntity staff);  

    }
}
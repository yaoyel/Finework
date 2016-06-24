using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace FineWork.Colla
{

    public interface IStaffReqManager
    { 
         StaffReqEntity FindStaffReqById(Guid StaffReqId); 

        StaffReqEntity CreateStaffReq(Guid accountId, Guid orgId, string message);

        IList<StaffReqEntity> FetchStaffReqsByAccount(Guid accountId);

        IList<StaffReqEntity> FetchStaffReqsByOrg(Guid orgId);
         
        StaffEntity ChangeReviewStatus(StaffReqEntity staffReq, ReviewStatuses newRevStatus);
    }
}

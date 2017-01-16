using System;
using System.Collections.Generic;
using FineWork.Security;
using System.IO;

namespace FineWork.Colla
{
    public interface IOrgManager
    {
        OrgEntity CreateOrg(IAccount account, String orgName, string avatarUrl);

        void ChangeOrgName(OrgEntity org, String newName);

        void ChangeOrgInvEnabled(OrgEntity org, bool newInvStatus);

        void ChangeOrgAdmin(OrgEntity org, Guid newAdminId);

        OrgEntity FindOrg(Guid orgId);

        OrgEntity FindOrgByName(String orgName);

        ICollection<OrgEntity> FetchOrgs();

        /// <summary> Finds a list of <see cref="OrgEntity"/> objects associated to an <see cref="IAccount"/>. </summary>
        IEnumerable<OrgEntity> FetchOrgsByAccount(Guid accountId, bool includeDisabled = false);
         
        void UploadOrgAvatar(Stream stream, Guid orgId, string contentType); 

    }
}
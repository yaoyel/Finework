using AppBoot.Repos.Ambients; 
using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using FineWork.Colla;
using FineWork.Message;
using FineWork.Net.Push;
using FineWork.Security;
using FineWork.Web.WebApi.Core;

namespace FineWork.Web.WebApi.Notification
{
    [Route("api/notifications")]
    [Authorize("Bearer")]
    public class NotificationController : FwApiController
    {
        public NotificationController(ISessionScopeFactory sessionScopeFactory,
            INotificationManager notificationManager,
            IAccountManager accountManager,
            IOrgManager orgManager)
            : base(sessionScopeFactory)
        {
            if (sessionScopeFactory == null) throw new ArgumentException(nameof(sessionScopeFactory));
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentException(nameof(orgManager));

            m_NotificationManager = notificationManager;
            m_OrgManager = orgManager;
            m_AccountManager = accountManager;  
        }
         
        private readonly INotificationManager m_NotificationManager;
        private readonly IAccountManager m_AccountManager;
        private readonly IOrgManager m_OrgManager;


        [HttpPost("CreateDeviceReg")]
        [DataScoped(true)]
        public void CreateDeiveDeviceRegistration([FromBody] DeviceRegistration deviceRegistration)
        {
            if (deviceRegistration == null) throw new ArgumentException(nameof(deviceRegistration));

            deviceRegistration.AccountId = this.AccountId;

            var account = m_AccountManager.FindAccount(this.AccountId);
            var org = m_OrgManager.FetchOrgsByAccount(this.AccountId).ToList();
            m_NotificationManager.CreateDeviceRegistrationAsync(deviceRegistration).Wait();

            if (org.Any())
            {
                var orgTags = new HashSet<string>(); 
                
                //org.ToList().ForEach(p => orgTags.Add(p.Id.ToString(GuidFormats.HyphenDigits.GetFormatString())));
                org.ToList().ForEach(p => orgTags.Add(p.Id.ToString("N")));
                m_NotificationManager.UpdateTagsAsync(deviceRegistration.RegistrationId, orgTags, null).Wait();
            }

            m_NotificationManager.UpdateAliasAsync(deviceRegistration.RegistrationId,
                account.PhoneNumber).Wait();


        }


        [HttpPost("DeleteDeviceReg")]
        [DataScoped(true)]
        public void DeleteDeiveDeviceRegistration(string registrationId)
        {
            if (string.IsNullOrEmpty(registrationId)) throw new ArgumentException(nameof(registrationId));

            m_NotificationManager.DeleteDeviceByRegistrationIdAsync(registrationId).Wait();

        }
    }
}
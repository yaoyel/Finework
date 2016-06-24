using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Transactions;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Message;
using FineWork.Net.IM;
using FineWork.Net.Push;
using FineWork.Security;
using FineWork.Web.WebApi.Core;
using Microsoft.AspNet.Http;

namespace FineWork.Web.WebApi.Message
{
    [Authorize("Bearer")]
    public class MessageController : FwApiController
    { 
        public MessageController(ISessionProvider<AefSession> sessionProvider,  
            INotificationManager notificationManager,
            IIMService imService, 
            IAccountManager accountManager,
            IOrgManager orgManager,
            ITaskManager taskManager)
            : base(sessionProvider)
        {
            if (notificationManager == null) throw new ArgumentException(nameof(notificationManager));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (orgManager == null) throw new ArgumentException(nameof(orgManager));
            if (taskManager == null) throw new ArgumentException(nameof(taskManager));
            if (imService == null) throw new ArgumentException(nameof(imService));

            m_NotificationManager = notificationManager;
            m_OrgManager = orgManager;
            m_AccountManager = accountManager;
            m_TaskManager = taskManager;
            m_IMService = imService;
        }
         
        private readonly INotificationManager m_NotificationManager;
        private readonly IAccountManager m_AccountManager;
        private readonly IOrgManager m_OrgManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;


        [HttpPost("api/notifications/CreateDeviceReg")]
        //[DataScoped(true)]
        public void CreateDeiveDeviceRegistration([FromBody] DeviceRegistration deviceRegistration)
        {
            if (deviceRegistration == null) throw new ArgumentException(nameof(deviceRegistration));

            var account = m_AccountManager.FindAccount(this.AccountId);

            using (var tx = TxManager.Acquire())
            {
                deviceRegistration.AccountId = this.AccountId; 
               
                m_NotificationManager.CreateDeviceRegistrationAsync(deviceRegistration).Wait();

                #region 暂无批量推送的业务
                //var org = m_OrgManager.FetchOrgsByAccount(this.AccountId).ToList();
                //if (org.Any())
                //{
                //    var orgTags = new HashSet<string>();

                //    //org.ToList().ForEach(p => orgTags.Add(p.Id.ToString(GuidFormats.HyphenDigits.GetFormatString())));
                //    org.ToList().ForEach(p => orgTags.Add(p.Id.ToString("N")));
                //    Task.Factory.StartNew(async () =>
                //    {
                //        await m_NotificationManager.UpdateTagsAsync(deviceRegistration.RegistrationId, orgTags, null);
                //    });
                //}
                #endregion   
                tx.Complete();
            }

            m_NotificationManager.UpdateAliasAsync(deviceRegistration.RegistrationId,
                account.PhoneNumber);
        }


        [HttpPost("api/notifications/DeleteDeviceReg")]
        [DataScoped(true)]
        public void DeleteDeiveDeviceRegistration(string registrationId)
        {
            if (string.IsNullOrEmpty(registrationId)) throw new ArgumentException(nameof(registrationId));

            m_NotificationManager.DeleteDeviceByRegistrationIdAsync(registrationId).Wait();

        }

        [HttpPost("api/im/SendImageMessage")]
        public bool SendImageMessage(Guid staffId, string conversationId, string name, Stream imageStream)
        {
            Args.NotNull(imageStream,nameof(imageStream));
            Args.NotEmpty(conversationId,nameof(conversationId));

            var result =
                m_IMService.SendImageMessageAsync(staffId.ToString(), conversationId, name, imageStream, null)
                    .Result;
            return result;


        }

        [HttpPost("api/im/SendAudioMessage")] 
        public bool SendAudioMessage([FromForm]MultimediaMessageModel audioMessage)
        {
            Args.NotNull(audioMessage, nameof(audioMessage));
            Args.NotNull(audioMessage.File, nameof(audioMessage.File));

            Args.NotEmpty(audioMessage.ConversationId, nameof(audioMessage.ConversationId));

            using (var reader = new StreamReader(audioMessage.File.OpenReadStream()))
            {
                var attrs = new Dictionary<string, object>()
                {
                    ["Kind"]=(int)audioMessage.Kind, 
                    ["AccountId"]=audioMessage.AccountId.ToString(),
                    ["Id"]=audioMessage.FromId.ToString()
                };
                var result =
                 m_IMService.SendAudioMessageAsync(audioMessage.StaffId.ToString(), audioMessage.ConversationId, audioMessage.Name,
                 reader.BaseStream, attrs)
                     .Result;
                return result;
            }

            
        }

        

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Checks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using cn.jpush.api;
using cn.jpush.api.push;
using cn.jpush.api.push.mode;
using FineWork.Message.Checkers;
using FineWork.Net.Push;
using FineWork.Security;
using FineWork.Security.Checkers;
using FineWork.Security.Repos.Aef;
using JetBrains.Annotations;

namespace FineWork.Message
{
    public class NotificationManager : AefEntityManager<DeviceRegistrationEntity, Guid>, INotificationManager
    {

        public NotificationManager(ISessionProvider<AefSession> dbContextProvider
            , JPushClient jPushClient,
            IAccountManager accountManager)
            : base(dbContextProvider)
        {
            if (dbContextProvider == null) throw new ArgumentNullException(nameof(dbContextProvider));
            if (jPushClient == null) throw new ArgumentNullException(nameof(jPushClient));
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            m_JPushClient = jPushClient;
            m_AccountManager = accountManager;
        }

        private readonly JPushClient m_JPushClient;

        private readonly IAccountManager m_AccountManager;

        public async Task CreateDeviceRegistrationAsync(DeviceRegistration deviceRegistration)
        {
            if (deviceRegistration == null)
                throw new ArgumentNullException(nameof(deviceRegistration));
            if (DeviceRegistrationExistsResult.Check(this, deviceRegistration.RegistrationId).IsSucceed)
                  this.DeleteDeviceByRegistrationIdAsync(deviceRegistration.RegistrationId).Wait();
            var account =
                AccountExistsResult.Check(this.m_AccountManager, deviceRegistration.AccountId).ThrowIfFailed().Account;


            await Task.Factory.StartNew(()=> this.InternalInsert(new DeviceRegistrationEntity()
            {
                Id = Guid.NewGuid(),
                Account = (AccountEntity) account,
                Platform = deviceRegistration.Platform,
                PlatformDescription = deviceRegistration.PlatformDescription,
                CreatedAt = DateTime.Now,
                RegistrationId = deviceRegistration.RegistrationId
            }));
        }

        public async Task DeleteDeviceByRegistrationIdAsync(string registrationId)
        {
            if (string.IsNullOrEmpty(registrationId))
                throw new ArgumentException("Value is null or empty", nameof(registrationId));

            var entity = this.InternalFetch(p => p.RegistrationId == registrationId).FirstOrDefault();
            if (entity == null)
            {
                await Task.FromResult(0);
                return;
            }
            await Task.Factory.StartNew(()=> this.InternalDelete(entity));
        }

        public async Task<PushResult> SendAsync(string title, string message,
            IDictionary<string, string> customizedValue,
            params string[] registrationIds)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("Value is null or empty", nameof(message));
            if (!registrationIds.Any())
                throw new ArgumentException("Value is null or empty", nameof(registrationIds));

            var audience = Audience.s_registrationId(registrationIds); 
            Func<PushResult> sendPush =
                () => m_JPushClient.SendPush(CreatePushPayload(title, message, customizedValue, audience)).ToResult();
            return await Task.Factory.StartNew(sendPush);
        }

        public async Task UpdateTagsAsync(string registrationId, HashSet<string> tagsToAdd, HashSet<string> tagsToRemove)
        {
            if (string.IsNullOrEmpty(registrationId))
                throw new ArgumentException("Value is null or empty", nameof(registrationId));

            Action updateDeviceTagAlias =
                () => m_JPushClient.updateDeviceTagAlias(registrationId, null, tagsToAdd, tagsToRemove);
            await Task.Factory.StartNew(updateDeviceTagAlias);
        }

        public async Task AddTagsAsync(string registrationId, params string[] items)
        {
            if (string.IsNullOrEmpty(registrationId))
                throw new ArgumentException("Value is null or empty", nameof(registrationId));
            if (!items.Any())
                throw new ArgumentException("Value is null or empty", nameof(items));

            Action updateDeviceTagAlias =
                () => m_JPushClient.updateDeviceTagAlias(registrationId,null, new HashSet<string>(items), null);

            await Task.Factory.StartNew(updateDeviceTagAlias);
        }

        public async Task DeleteTagsAsync(string registrationId, params string[] items)
        {
            if (string.IsNullOrEmpty(registrationId))
                throw new ArgumentException("Value is null or empty", nameof(registrationId));
            if (!items.Any())
                throw new ArgumentException("Value is null or empty", nameof(items));
            Action updateDeviceTagAlias =
                () => m_JPushClient.updateDeviceTagAlias(registrationId,null, null, new HashSet<string>(items));

            await Task.Factory.StartNew(updateDeviceTagAlias);
        }

        public async Task<PushResult> SendByTagsAsync(string title, [NotNull] string message,
            IDictionary<string, string> customizedValue,
            [NotNull] params string[] tags)
        {
            var audience = Audience.s_tag(tags);

            Func<PushResult> sendPush =
                () => m_JPushClient.SendPush(CreatePushPayload(title, message, customizedValue, audience)).ToResult();
            return await Task.Factory.StartNew(sendPush);
        }


        public async Task UpdateAliasAsync(string registrationId, string newAlias)
        {
            if (string.IsNullOrEmpty(registrationId))
                throw new ArgumentException("Value is null or empty", nameof(registrationId));

            //newalias 为空时则将alias清空
            await Task.Factory.StartNew(() => m_JPushClient.updateDeviceTagAlias(registrationId, newAlias, null, null));
        }

        public async Task<PushResult> SendByAliasAsync(string title, [NotNull] string message,
            IDictionary<string, string> customizedValue,
            [NotNull] params string[] aliases)
        {
            var existAliases = new List<string>();
            //删除不存在的aliases
            aliases.ToList().ForEach(p =>
            {
                var aliasDeviceList = m_JPushClient.getAliasDeviceList(p, null);
                if (aliasDeviceList.registration_ids.Any())
                    existAliases.Add(p);
            });
            if (!existAliases.Any())
            {
                var pushResult = new PushResult()
                {
                    IsSuccess = false,
                    ErrorInfo = "不存在对应的alias"
                };
                return await Task.FromResult(pushResult);
            }

            var audience = Audience.s_alias(existAliases.ToArray()); 
            Func<PushResult> sendPush =
                () => m_JPushClient.SendPush(CreatePushPayload(title, message, customizedValue, audience)).ToResult();
            return await Task.Factory.StartNew(sendPush);
        }


        public async Task<DeviceRegistrationEntity> FindDeviceRegistrationByIdAsync(string registrationId)
        {
            if (string.IsNullOrEmpty(registrationId))
                throw new ArgumentException("Value is null or empty", nameof(registrationId));

            var entity = this.InternalFetch(p => p.RegistrationId == registrationId).FirstOrDefault();

            return await Task.FromResult(entity);
        }

        public async Task<IList<DeviceRegistrationEntity>> FetchDeviceRegistrationsByIdsAsync(
            params string[] registrationIds)
        {
            if (!registrationIds.Any())
                throw new ArgumentException("Value is null or empty", nameof(registrationIds));

            var devices = await this.InternalFetchAsync(p => registrationIds.Contains(p.RegistrationId));

            return devices;
        }

        public async Task<DeviceRegistrationEntity> FindDeviceRegistraionByAccountIdAsync(Guid accountId)
        {
            var device = this.InternalFetch(p => p.Account.Id == accountId).FirstOrDefault();

            return await Task.FromResult(device);

        }

        public async Task<IList<DeviceRegistrationEntity>> FetchDeviceRegistrationsAsync(params Guid[] accountIds)
        {
            if (!accountIds.Any())
                throw new ArgumentException("Value is null or empty", nameof(accountIds));

            var devices = await this.InternalFetchAsync(p => accountIds.Contains(p.Account.Id));

            return devices;
        }



        private PushPayload CreatePushPayload(string title, string message,
            IDictionary<string, string> customizedValue, Audience audience)
        {
            var notification = new Notification();
            notification.AndroidNotification = new cn.jpush.api.push.notification.AndroidNotification();
            notification.IosNotification = new cn.jpush.api.push.notification.IosNotification();
            notification.WinphoneNotification = new cn.jpush.api.push.notification.WinphoneNotification();
            notification.setAlert(message).AddExtraToAll(customizedValue);
            notification.AndroidNotification
                .setTitle(title);
            notification.IosNotification.disableBadge();
            notification.IosNotification.incrBadge(-1);
            var pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();
            pushPayload.audience = audience;
            pushPayload.notification = notification;
            

            return pushPayload;
        }


        private void SetIosBadge(Notification notification,int badge)
        {
            notification.IosNotification.disableBadge();
            notification.IosNotification.incrBadge(badge);
        }
    }

    public static class NotificationExtensions
    {
        public static Notification AddExtraToAll(this Notification notification, IDictionary<string, string> extra)
        {
            if (extra == null || !extra.Any())
                return notification;

            extra.ToList().ForEach(p =>
            {
                notification.AndroidNotification.AddExtra(p.Key, p.Value);
                notification.IosNotification.AddExtra(p.Key, p.Value);
                notification.WinphoneNotification.AddExtra(p.Key, p.Value);
            });
            return notification;
        }

        public static PushResult ToResult([NotNull] this MessageResult result)
        {
            var pushResult = new PushResult();
            pushResult.MessageId = result.msg_id.ToString();
            pushResult.TraceId = result.sendno.ToString();
            pushResult.IsSuccess = result.isResultOK();
            return pushResult;
        }

    }
}

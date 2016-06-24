using System;
using AppBoot.Checks;
using FineWork.Security;
using JetBrains.Annotations;
using FineWork.Common;

namespace FineWork.Message.Checkers
{
    public class DeviceRegistrationExistsResult : CheckResult
    {
        public DeviceRegistrationExistsResult(bool isSucceed, String message, DeviceRegistrationEntity deviceRegistration)
            : base(isSucceed, message)
        {
            this.DeviceRegistrationEntity = deviceRegistration;
        }
         
        public DeviceRegistrationEntity DeviceRegistrationEntity { get; private set; }
         
        public static DeviceRegistrationExistsResult Check(INotificationManager notificationManager, string registrationId)
        {
             var deviceRegistration= notificationManager.FindDeviceRegistrationByIdAsync(registrationId).Result;
            return Check(deviceRegistration, $"deviceRegistration [RegistrationId: {registrationId}] does not exist.");
        }

 
        private static DeviceRegistrationExistsResult Check([NotNull]DeviceRegistrationEntity deviceRegistration, String message)
        { 
            return new DeviceRegistrationExistsResult(true, null, deviceRegistration);
        }

        public override Exception CreateException(string message)
        {
            return new FineWorkException(message);
        }
    }
}

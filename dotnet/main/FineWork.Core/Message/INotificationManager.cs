using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Net.Push;

namespace FineWork.Message
{
    public interface INotificationManager:IPushService,ITagsService,IAliasService 
    {
        Task CreateDeviceRegistrationAsync(DeviceRegistration deviceRegistration);

        Task DeleteDeviceByRegistrationIdAsync(string registrationId);

        Task<DeviceRegistrationEntity> FindDeviceRegistrationByIdAsync(string registrationId);

        Task<IList<DeviceRegistrationEntity>> FetchDeviceRegistrationsByIdsAsync(params string[] registrationId);

        Task<DeviceRegistrationEntity> FindDeviceRegistraionByAccountIdAsync(Guid accountId);

        Task<IList<DeviceRegistrationEntity>> FetchDeviceRegistrationsAsync(params Guid[] accountIds);

    }
}

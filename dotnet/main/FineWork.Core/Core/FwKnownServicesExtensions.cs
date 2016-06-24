using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using FineWork.Colla;
using FineWork.Message;
using FineWork.Net.IM;
using FineWork.Security;
using Microsoft.Extensions.DependencyInjection; 

namespace FineWork.Core
{
    /// <summary> 用于将常见的服务添加为扩展方法而不污染 <see cref="IServiceProvider"/>. </summary>
    public class FwWrappedServices : DisposableBase,IServiceProvider
    {
        public FwWrappedServices(IServiceProvider wrapped)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            this.Wrapped = wrapped;
        }

        public IServiceProvider Wrapped { get; private set; }

        protected override void DoDispose(bool disposing)
        {
            if (disposing)
            {
                (this.Wrapped as IDisposable)?.Dispose();
            }
            base.DoDispose(disposing);
        }

        public object GetService(Type serviceType)
        {
            return this.Wrapped.GetService(serviceType);
        }
    }

    public static class FwKnownServicesExtensions
    {
        public static FwWrappedServices FwWrapped(this IServiceProvider wrapped)
        {
            if (wrapped == null) throw new ArgumentNullException(nameof(wrapped));
            var result = wrapped as FwWrappedServices;
            return result ?? new FwWrappedServices(wrapped);
        }

        public static ISessionProvider<AefSession> SessionProvider(this FwWrappedServices services)
        {
            return services.GetRequiredService<ISessionProvider<AefSession>>();
        }

        public static IAccountManager AccountManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IAccountManager>();
        }

        public static IOrgManager OrgManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IOrgManager>();
        }

        public static IStaffManager StaffManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IStaffManager>();
        }

        public static IStaffInvManager StaffInvManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IStaffInvManager>();
        }

        public static IStaffReqManager StaffReqManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IStaffReqManager>();
        }

        public static ITaskManager TaskManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<ITaskManager>();
        }

        public static IPartakerManager PartakerManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IPartakerManager>();
        }

        public static IPartakerReqManager PartakerReqManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IPartakerReqManager>();
        }

        public static IPartakerInvManager PartakerInvManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IPartakerInvManager>();
        }

        public static ITaskAlarmManager TaskAlarmManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<ITaskAlarmManager>();
        }

        public static IAlarmManager AlarmPeriodManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<IAlarmManager>();
        }

        public static INotificationManager NotificationManager(this FwWrappedServices services)
        {
            return services.GetRequiredService<INotificationManager>();
        }

        public static IIMService IMService(this FwWrappedServices services)
        {
            return services.GetRequiredService<IIMService>();
        }
    }
}

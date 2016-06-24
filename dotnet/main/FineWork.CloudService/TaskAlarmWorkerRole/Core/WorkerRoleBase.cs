using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Core;
using Microsoft.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure;

namespace TaskAlarmWorkerRole.Core
{
    public class WorkerRoleBase
    {
        private readonly string m_ConnectionString;
        private readonly string m_PushKey;
        private readonly string m_PushMaster;
        private readonly string m_StorageConnectionString;

        private ServiceCollection ServiceCollection { get; } = new ServiceCollection();
        public FwWrappedServices Services { get; }


        public WorkerRoleBase()
        {
            m_ConnectionString = CloudConfigurationManager.GetSetting("ConnectionString");
            m_PushKey = CloudConfigurationManager.GetSetting("PushKey"); ;
            m_PushMaster = CloudConfigurationManager.GetSetting("PushSecret");
            m_StorageConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            BuildService(); 
            Services = new FwWrappedServices(ServiceCollection.BuildServiceProvider());
        }

        private void BuildService()
        {
            ServiceCollection.AddFileManager(m_StorageConnectionString)
               .AddDbSession(m_ConnectionString)
               .AddSessionProvider()
               .AddFineWorkCoreServices()
               .AddJPushClient(m_PushKey, m_PushMaster)
               .AddImClient(null)
               .AddSmsService(null,null);

            ServiceCollection.AddScoped<IConfiguration>(s=>new ConfigurationBuilder().Build());
        }
    }
}

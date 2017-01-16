using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Core;
using Microsoft.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure;

namespace TaskAlarmWorkerRole.Core
{
    public class WorkerRoleBase
    {
        private readonly string m_ConnectionString;
        private readonly string m_PushKey;
        private readonly string m_PushMaster;
        private readonly string m_LcId;
        private readonly string m_LcKey;
        private readonly string m_LcMaster; 
        private readonly string m_StorageConnectionString;
        private readonly string m_ApnsProduction;

        private ServiceCollection ServiceCollection { get; } = new ServiceCollection();
        public FwWrappedServices Services { get; }


        public WorkerRoleBase()
        {
            m_ConnectionString = CloudConfigurationManager.GetSetting("ConnectionString");
            m_PushKey = CloudConfigurationManager.GetSetting("PushKey"); ;
            m_PushMaster = CloudConfigurationManager.GetSetting("PushSecret");
            m_StorageConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            m_LcId = CloudConfigurationManager.GetSetting("LcId");
            m_LcKey = CloudConfigurationManager.GetSetting("LcKey");
            m_LcMaster = CloudConfigurationManager.GetSetting("LcMaster");
            m_ApnsProduction = CloudConfigurationManager.GetSetting("apns_production");
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
               .AddImClient(m_LcId,m_LcKey,m_LcMaster)
               .AddSmsService(null,null);

            var configBuilder=new ConfigurationBuilder();

            var provier = new MemoryConfigurationProvider();
            provier.Add("JPush:apns_production", m_ApnsProduction);
            configBuilder.Add(provier);
            ServiceCollection.AddScoped<IConfiguration>(s=> configBuilder.Build());
        }
    }
}

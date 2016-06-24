using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Core;
using FineWork.Message; 
using Microsoft.WindowsAzure.ServiceRuntime; 
using TaskAlarmWorkerRole.Core; 
using  AppBoot.Transactions;
using Microsoft.WindowsAzure.Storage;  

namespace TaskAlarmWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent m_RunCompleteEvent = new ManualResetEvent(false);
        private readonly FwWrappedServices m_Services = new WorkerRoleBase().Services;
        private IAlarmManager m_AlarmPeriodManager;
        private ITaskManager m_TaskManager;
        private INotificationManager m_NotificationManager;
        private IStaffManager m_StaffManager;


        public override void Run()
        {
            Trace.TraceInformation("TaskAlarmWorkerRole is running");

            try
            {
                this.RunAsync(this.m_CancellationTokenSource.Token).Wait();
            }
            catch (Exception ex)
            {
                var container = CloudStorageAccount.Parse(
                    @"TableEndpoint=https://chrd.table.core.chinacloudapi.cn;BlobEndpoint=https://chrd.blob.core.chinacloudapi.cn;AccountName=chrd;AccountKey=rScgb7xsEq44A0Ex+5NajKE2xeYg/la1bbZ0LrG8R0iC6OqLkQ6zobFgsmJdrFEWOmR1F4Z9i6KJbYwlCe6qEQ==")
                    .CreateCloudBlobClient().GetContainerReference("errors");
                container.CreateIfNotExists();
                var blob = container.GetBlockBlobReference(string.Format("error-{0}-{1}",
                    RoleEnvironment.CurrentRoleInstance.Id, DateTime.UtcNow.Ticks));
                blob.UploadText(ex.ToString()); 
            }
            finally
            {
                this.m_RunCompleteEvent.Set();
               
            }
        }

        public override bool OnStart()
        {
            //设置最大并发连接数
            ServicePointManager.DefaultConnectionLimit = 12;

            m_AlarmPeriodManager = m_Services.AlarmPeriodManager();
            m_TaskManager = m_Services.TaskManager();
            m_StaffManager = m_Services.StaffManager();
            m_NotificationManager = m_Services.NotificationManager();

            // 有关处理配置更改的信息，
            // 请参见 http://go.microsoft.com/fwlink/?LinkId=166357 上的 MSDN 主题。

            bool result = base.OnStart();

            Trace.TraceInformation("TaskAlarmWorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("TaskAlarmWorkerRole is stopping");

            this.m_CancellationTokenSource.Cancel();
            this.m_RunCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("TaskAlarmWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
     
            while (!cancellationToken.IsCancellationRequested)
            { 
                using (var tx = TxManager.Acquire())
                {
                    var alarmPeriod = m_AlarmPeriodManager.FetchAlarmPeriodsByDate(null).ToList();
                     
                    if (!alarmPeriod.Any())
                    {
                        await Task.Delay(10000, cancellationToken);
                        continue;
                    } 
                    
                    await Task.Factory.StartNew(() =>
                    {

                        alarmPeriod.ForEach(p =>
                        {
                            var task = p.Task;
                            var partakers = task.Partakers;
                            if (partakers.Count() == 1) return;

                            partakers.ToList().ForEach(s =>
                            {
                                var customizedValue = new Dictionary<string, string>()
                                {
                                    ["TaskId"] = task.Id.ToString(),
                                    ["TaskName"] = task.Name,
                                    ["AlarmPeriodId"] = p.Id.ToString(),
                                    ["PathTo"] = "TaskAlarm",
                                    ["Bell"] = p.Bell,
                                    ["StaffId"] = s.Staff.Id.ToString(),
                                    ["OrgId"] = s.Staff.Org.Id.ToString()
                                };

                                m_NotificationManager.SendByAliasAsync(null, $"您收到一个定时预警信息。", customizedValue,s.Staff.Account.PhoneNumber);
                            });

                            Trace.TraceInformation(
                                $"{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}任务[{task.Name}]定时预警，共发出{partakers.Count()}条预警。");
                        });
                    }, cancellationToken);  
                    tx.Complete();
                }

                await Task.Delay(60000, cancellationToken);
            }
        }
    }
}

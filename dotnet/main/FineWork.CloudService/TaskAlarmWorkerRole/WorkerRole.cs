using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using AppBoot.Transactions;
using FineWork.Net.IM;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using AppBoot.Security.Crypto;
using FineWork.Colla.Checkers;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json; 

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
        private IAnnouncementManager m_AnnouncementManager;
        private IAnncAlarmManager m_AnncAlarmManager;
        private IPlanAlarmManager m_PlanAlarmManager;
        private IIMService m_ImService;
        private IMemberManager m_MemberManager;
        private IConversationManager m_ConversationManager;
        private IPushLogManager m_PushLogManager;
        private ITaskVoteManager m_TaskVoteManager;

        CloudTable GetTable()
        {
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("convlogs");
            if (!table.Exists())
                table.CreateIfNotExists();
            return table;
        }

        public override void Run()
        {
            Trace.TraceInformation("TaskAlarmWorkerRole is running");
            try
            {
                Task.Factory.StartNew(
                    () =>
                    {
                        FetchConvMessagesAsync(GetTable(), m_CancellationTokenSource.Token).ContinueWith((t) => { });
                    });

                this.RunAsync(this.m_CancellationTokenSource.Token).Wait();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Run error: " + ex);
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
            m_AnnouncementManager = m_Services.AnnouncementManager();
            m_AnncAlarmManager = m_Services.AnncAlarmManager();
            m_PlanAlarmManager = m_Services.PlanAlarmManager();
            m_ImService = m_Services.ImService();
            m_MemberManager = m_Services.MemberManager();
            m_ConversationManager = m_Services.ConversationManager();
            m_PushLogManager = m_Services.PushLogManager();
            m_TaskVoteManager = m_Services.TaskVoteManager();

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
                var alarmPeriod = m_AlarmPeriodManager.FetchAlarmPeriodsByTime(null).ToList();

                var anncAlarms = m_AnncAlarmManager.FetchAnncAlarmsByTime(DateTime.Now).ToList();

                var planAlarms = m_PlanAlarmManager.FetchPlanAlarmsByTime(DateTime.Now).ToList();

                var votes = m_TaskVoteManager.FetchVotesByTime(DateTime.Now).ToList();

                if (!alarmPeriod.Any() && !anncAlarms.Any() && !planAlarms.Any() && !votes.Any())
                {
                    await Task.Delay(10000, cancellationToken);
                    continue;
                }

                var delay = Task.Delay(60000, cancellationToken);
                await SendAlarmMessage(alarmPeriod, cancellationToken);
                await SendAnncMessage(anncAlarms, cancellationToken);
                await SendPlanMessage(planAlarms, cancellationToken);
                await SendVoteMessage(votes, cancellationToken); 
                 
                await delay;
            }
        }

        private async Task FetchConvMessagesAsync(CloudTable table,CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var time = DateTime.Now.ToUniversalTime();
                    long unixTimestamp = (long)(time.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
                    var logs =
                        JsonConvert.DeserializeObject<List<ConvMessageModel>>(
                            m_ImService.FetchMessageAsync(100, unixTimestamp).Result);

                    while (logs.Any())
                    {
                        var lastLog = logs.Last();
                        var firstLog = logs.First(); 
                        var firstBase64ParKey = Convert.ToBase64String(Encoding.Default.GetBytes(firstLog.ConvId));
                        var firstBase64RowKey = (firstLog.TimeStamp ^ long.MaxValue).ToString();

                        if (
                            table.Execute(TableOperation.Retrieve<ConvMessageModel>(firstBase64ParKey, firstBase64RowKey))
                                .Result != null)
                            logs = JsonConvert.DeserializeObject<List<ConvMessageModel>>(m_ImService.FetchMessageAsync(100, lastLog.TimeStamp).Result,new JsonSerializerSettings()
                            {
                                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
                            });
                        else
                        { 
                            var logGroup = logs.GroupBy(p => p.ConvId);
                            foreach (var items in logGroup)
                            {
                          
                                TableBatchOperation batchOperation = new TableBatchOperation();
                                foreach (var item in items)
                                {
                                    var msgData = JsonConvert.DeserializeObject<MsgData>(item.Data);
                                    var base64ParKey = Convert.ToBase64String(Encoding.Default.GetBytes(item.ConvId));
                                    //主键倒序 方便查询
                                    var base64RowKey = item.TimeStamp ^ long.MaxValue;
                                    item.PartitionKey = base64ParKey;
                                    item.RowKey = base64RowKey.ToString();
                                    item.Time = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(item.TimeStamp)
                                        .AddHours(8);
                                    item.To =(msgData==null  || msgData.LcAttrs==null ||  string.IsNullOrEmpty(msgData.LcAttrs.To) )? item.To : msgData.LcAttrs.To;
                                  
                                    batchOperation.InsertOrReplace(item);

                                    //如果是私聊 To值为staffId,创建聊天室和成员
                                    Guid staffToId;
                                    Guid staffFromId;
                                    if (Guid.TryParse(item.To, out staffToId) && Guid.TryParse(item.From,out staffFromId))
                                    {
                                        var staffTo = StaffExistsResult.Check(this.m_StaffManager, staffToId).Staff;
                                        var staffFrom= StaffExistsResult.Check(this.m_StaffManager, staffFromId).Staff;
                                       
                                        if (staffTo != null && staffFrom != null)
                                        {
                                            var services = new WorkerRoleBase().Services;
                                            services.ConversationManager().CreateConversation(item.ConvId.Substring(0,24),true);
                                            services.MemberManager().CreateMember(item.ConvId.Substring(0,24), staffToId, staffFromId);
                                        }

                                    } 
                                }
                                table.ExecuteBatch(batchOperation);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    // ignored
                }


                await Task.Delay(10000);
            }
        } 

        private async Task SendAlarmMessage(List<AlarmEntity> alarmPeriod, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {

                alarmPeriod.ForEach(p =>
                {
                    var task = p.Task;
                    var partakers = task.Partakers.ToList();

                    if (!string.IsNullOrEmpty(p.ReceiverStaffIds))
                    {
                        var receiveStaffs = Array.ConvertAll(p.ReceiverStaffIds.Split(','), Guid.Parse);

                        partakers = partakers.Where(w => receiveStaffs.Contains(w.Staff.Id)).ToList();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(p.ReceiverKinds))
                        {
                            var receiveKinds = Array.ConvertAll(p.ReceiverKinds.Split(','), int.Parse);

                            partakers = partakers.Where(w => receiveKinds.Contains((int)w.Kind)).ToList();
                        }
                    }

                    var customizedValue = new Dictionary<string, string>()
                    {
                        ["TaskId"] = task.Id.ToString(),
                        ["TaskName"] = task.Name,
                        ["AlarmPeriodId"] = p.Id.ToString(),
                        ["PathTo"] = "TaskAlarm",
                        ["Bell"] = p.Bell, 
                        ["OrgId"] = p.Task.Creator.Org.Id.ToString()
                    };

                    partakers.ToList().ForEach(s =>
                    {
                        m_PushLogManager.CreatePushLog(s.Staff.Id, p.Id, PushKinds.Alarm);
                    });

                    m_NotificationManager.SendByAliasAsync(null, $"您收到一个任务定时提醒信息。", customizedValue,
                         partakers.Select(s=>s.Staff.Account.PhoneNumber).ToArray());

                    Trace.TraceInformation(
                        $"{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}任务[{task.Name}]定时提醒，共发出{partakers.Count()}条提醒。");
                });
            }, cancellationToken);
        }

        private async Task SendAnncMessage(List<AnncAlarmEntity> anncAlarms,CancellationToken cancellationToken )
        {
            await Task.Factory.StartNew(() =>
            {
                anncAlarms.ForEach(p =>
                {
                    List<StaffEntity> receivers;
                    string message, path;
                    if (p.Time.HasValue && p.Annc.EndAt == p.Time.Value)
                    {
                        receivers = p.Annc.Executors.Select(s => s.Staff).ToList();
                        receivers.Add(p.Annc.Creator);
                        receivers.Add(p.Annc.Inspecter);
                        receivers = receivers.GroupBy(g => g.Id).Select(s => s.First()).ToList();
                        message = "您收到一个任务计划验收信息";
                        path = "PersonalCenter/Notice";
                        foreach (var rec in receivers)
                        {
                            m_PushLogManager.CreatePushLog(rec.Id, p.Annc.Id, PushKinds.Annc);
                        }
                    }
                    else
                    {
                        receivers = p.Recs.Select(s => s.Staff).ToList();
                        message = "您收到一个任务计划提醒信息";
                        path = "Annc";
                    }

                    var customizedValue = new Dictionary<string, string>()
                    {
                        ["AnncId"] = p.Annc.Id.ToString(),
                        ["PathTo"] = path, 
                        ["TaskId"] = p.Annc.Task.Id.ToString(),
                        ["OrgId"] = p.Annc.Creator.Org.Id.ToString()
                    };

                    m_NotificationManager.SendByAliasAsync(null, message, customizedValue,
                        receivers.Select(s=>s.Account.PhoneNumber).ToArray());

                    Trace.TraceInformation(
                       $"{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}发送{receivers.Count}个任务计划提醒消息 。");
            
                });

            }, cancellationToken);
        }

        private async Task SendPlanMessage(List<PlanAlarmEnitty> planAlarms,CancellationToken cancellationToken )
        {
            await Task.Factory.StartNew(() =>
            {

                planAlarms.ForEach(p =>
                {
                    var creator = p.Plan.Creator;

                    var customizedValue = new Dictionary<string, string>()
                    {
                        ["PlanId"] = p.Plan.Id.ToString(),
                        ["StaffId"] = creator.Id.ToString(),
                        ["PathTo"] = "Plan",
                        ["Bell"] = p.Bell,
                        ["OrgId"] = creator.Org.Id.ToString(),
                    };

                    m_NotificationManager.SendByAliasAsync(null, $"您收到一个计划定时提醒信息。", customizedValue,
                        creator.Account.PhoneNumber);

                    Trace.TraceInformation(
                        $"{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}个人计划[{p.Plan.Content}]定时提醒");
                });
            }, cancellationToken);

        }

        private async Task SendVoteMessage(List<TaskVoteEntity> votes,CancellationToken cancellationToken )
        {
            await Task.Factory.StartNew(() =>
            {
                votes.ForEach(p =>
                {
                    var partakers = p.Task.Partakers.ToList();

                    var customizedValue = new Dictionary<string, string>()
                    {
                        ["TaskId"] = p.Task.Id.ToString(),
                        ["TaskName"] = p.Task.Name,
                        ["VoteId"] = p.Id.ToString(),
                        ["PathTo"] = "Vote", 
                        ["OrgId"] = p.Task.Creator.Org.Id.ToString()
                    };

                
                    partakers.ForEach(f =>
                    { 
                        m_PushLogManager.CreatePushLog(f.Staff.Id, p.Vote.Id, PushKinds.Vote); 
                    });

                    m_NotificationManager.SendByAliasAsync(null, $"您收到一个待标记的共识信息。", customizedValue,
                        partakers.Select(s=>s.Staff.Account.PhoneNumber).ToArray()); 

                    Trace.TraceInformation(
                        $"{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}任务[{p.Task.Name}]共识提醒，共发出{partakers.Count()}条提醒。");
                });
            },cancellationToken);
        }
         
      
    }
}

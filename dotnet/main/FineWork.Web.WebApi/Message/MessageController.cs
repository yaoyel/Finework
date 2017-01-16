using FineWork.Web.WebApi.Common;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using FineWork.Colla.Models;

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
            ITaskManager taskManager,
            IMemberManager memberManager,
            IConversationManager conversationManager,
            IConfiguration configuration,
            IStaffManager staffManager,
            ITaskAlarmManager taskAlarmManager)
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
            m_MemberManager = memberManager;
            m_Config = configuration;
            m_ConversationManager = conversationManager;
            m_StaffManager = staffManager;
            m_TaskAlarmManager = taskAlarmManager;
        }

        private readonly INotificationManager m_NotificationManager;
        private readonly IAccountManager m_AccountManager;
        private readonly IOrgManager m_OrgManager;
        private readonly ITaskManager m_TaskManager;
        private readonly IIMService m_IMService;
        private readonly IMemberManager m_MemberManager;
        private readonly IConfiguration m_Config;
        private readonly IConversationManager m_ConversationManager;
        private readonly IStaffManager m_StaffManager;
        private readonly ITaskAlarmManager m_TaskAlarmManager;

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
            Args.NotNull(imageStream, nameof(imageStream));
            Args.NotEmpty(conversationId, nameof(conversationId));

            var result =
                m_IMService.SendImageMessageAsync(staffId.ToString(), conversationId, name, imageStream, null)
                    .Result;
            return result;


        }

        [HttpPost("api/im/SendAudioMessage")]
        public bool SendAudioMessage([FromForm] MultimediaMessageModel audioMessage)
        {
            Args.NotNull(audioMessage, nameof(audioMessage));
            Args.NotNull(audioMessage.File, nameof(audioMessage.File));

            Args.NotEmpty(audioMessage.ConversationId, nameof(audioMessage.ConversationId));

            using (var reader = new StreamReader(audioMessage.File.OpenReadStream()))
            {
                var attrs = new Dictionary<string, object>()
                {
                    ["Kind"] = (int) audioMessage.Kind,
                    ["AccountId"] = audioMessage.AccountId.ToString(),
                    ["Id"] = audioMessage.FromId.ToString(),
                    ["MsgType"] = audioMessage.MsgType,
                    ["CustomId"] = audioMessage.CustomId
                };
                var result =
                    m_IMService.SendAudioMessageAsync(audioMessage.StaffId.ToString(), audioMessage.ConversationId,
                        audioMessage.Name,
                        reader.BaseStream, attrs)
                        .Result;
                return result;
            }


        }

        [HttpGet("api/im/FetchMessagesByStaffId")]
        public IActionResult FetchMessagesByStaffId(Guid staffId, string content, int? page, int? pageSize)
        {
            var staff = StaffExistsResult.Check(m_StaffManager, staffId).Staff;
            if (staff == null) return new HttpNotFoundObjectResult(staffId);

            var result = new List<ConvMessageModel>();
            var staffInOrgs = m_StaffManager.FetchStaffsByOrg(staff.Org.Id, null);
            var members = m_MemberManager.FetchMembersByStaffId(staffId).ToList();
            var membersWithClearTime = members.Where(p => p.ClearLogAt != null).GroupBy(p => p.Conversation).ToList();
            var membersWithoutClearTime =
                members.Where(p => !p.ClearLogAt.HasValue).Select(p => p.Conversation.Id).Distinct().ToList();


            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(m_Config["AzureSettings:StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("convlogs");

            if (membersWithClearTime.Any())
            {
                membersWithClearTime.ForEach((t) =>
                {
                    var base64ParKey =
                        Convert.ToBase64String(Encoding.Default.GetBytes(t.Key.Id.PadRight(32, '0')));

                    var keyFilter = TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal, base64ParKey);
                    var bgFilter = TableQuery.GenerateFilterCondition("From",
                        QueryComparisons.NotEqual, "Background");
                    var combine = TableQuery.CombineFilters(keyFilter, TableOperators.And, bgFilter);
                    if (t.First() != null && t.First().ClearLogAt != null)
                    {
                        var ts = GetTimestamp(t.First().ClearLogAt.Value);
                        combine = TableQuery.CombineFilters(combine, TableOperators.And,
                            TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.GreaterThan, ts));
                    }

                    var query = new TableQuery<ConvMessageModel>().Where(combine);
                    var message = table.ExecuteQuery(query).ToList();
                    result.AddRange(message);
                });
            }

            if (membersWithoutClearTime.Any())
            {
                for (var i = 0; i <= membersWithoutClearTime.Count/100; i++)
                {
                    var pageItem = membersWithoutClearTime.Skip(i*100).Take(100).ToList();
                    if (pageItem.Count == 0) continue;

                    var bgFilter = TableQuery.GenerateFilterCondition("From",
                        QueryComparisons.NotEqual, "Background");

                    var firItem = pageItem.First();
                    var filters = TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal, firItem);

                    pageItem.Remove(firItem);

                    pageItem.ForEach((t) =>
                    {
                        var base64ParKey =
                            Convert.ToBase64String(Encoding.Default.GetBytes(t.PadRight(32, '0')));

                        var keyFilter = TableQuery.GenerateFilterCondition("PartitionKey",
                            QueryComparisons.Equal, base64ParKey);

                        filters = TableQuery.CombineFilters(filters, TableOperators.Or, keyFilter);
                    });

                    filters = TableQuery.CombineFilters(filters, TableOperators.And, bgFilter);
                    var query = new TableQuery<ConvMessageModel>().Where(filters);
                    var message = table.ExecuteQuery(query).ToList();
                    result.AddRange(message);
                }

            }

            result =
                result.Where(p => p.Data.Contains("\"Alarm\":null") && p.Data.Contains("\"_lctype\":-1"))
                    .OrderByDescending(p => p.Time)
                    .ToList();
            if (!string.IsNullOrEmpty(content))
                result =
                    result.Where(p => JsonConvert.DeserializeObject<MsgData>(p.Data).LcText.Contains(content))
                        .ToList();
            if (result.Any())
                return
                    new ObjectResult(
                        result.AsQueryable().ToPagedList(page, pageSize).Data.Select(p => p.ToViewModel(staffInOrgs)));

            return new HttpNotFoundObjectResult(staffId);
        }

        [HttpGet("api/im/FetchMessagesByConvId")]
        public IActionResult FetchMessagesByConvId(Guid staffId, string convId, int? pageSize, DateTime? time,
            bool ltTime = false)
        {
            var staff = StaffExistsResult.Check(m_StaffManager, staffId).Staff;
            if (staff == null) return new HttpNotFoundObjectResult(staffId);

            var staffInOrgs = m_StaffManager.FetchStaffsByOrg(staff.Org.Id, null);

            var member = ConvrMemberExistsResult.Check(this.m_MemberManager, staffId, convId.Substring(0, 24)).Member;

            if (member == null)
                return new HttpNotFoundObjectResult(convId);

            if (pageSize.HasValue && !time.HasValue)
                time = DateTime.Now;

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(m_Config["AzureSettings:StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("convlogs");

            var base64ParKey = Convert.ToBase64String(Encoding.Default.GetBytes(convId.PadRight(32, '0')));

            var keyFilter = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, base64ParKey);
            var bgFilter = TableQuery.GenerateFilterCondition("From",
                QueryComparisons.NotEqual, "Background");
            var combine = TableQuery.CombineFilters(keyFilter, TableOperators.And, bgFilter);
            if (member.ClearLogAt != null)
            {
                long ts = GetTimestamp(member.ClearLogAt.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.GreaterThan, ts));
            }

            if (time.HasValue)
            {
                long ts = GetTimestamp(time.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp",
                        ltTime ? QueryComparisons.LessThan : QueryComparisons.GreaterThan, ts));
            }
            var query = new TableQuery<ConvMessageModel>().Where(combine);

            var result = table.ExecuteQuery(query).ToList();

            result = result.OrderBy(p => p.TimeStamp).ToList();
            
            if (pageSize.HasValue)
                result = result.Take(pageSize.Value).ToList(); 

            if (result.Any())
            {
                return new ObjectResult(result.Select(p => p.ToViewModel(staffInOrgs)));
            }

            return new HttpNotFoundObjectResult(convId);
        }

        [HttpGet("api/im/FetchMessagesByAlarmId")]
        public IActionResult FetchMessagesByAlarmId(Guid alarmId, Guid staffId, int? pageSize, DateTime? time)
        { 
            var alarm = TaskAlarmExistsResult.Check(this.m_TaskAlarmManager, alarmId).TaskAlarm;
            if (alarm == null) return new HttpNotFoundObjectResult(alarmId);

            var conv = ConversationExistsResult.Check(this.m_ConversationManager, alarm.Conversation.Id).Conversation;
            if (conv == null) return new HttpNotFoundObjectResult(alarmId);

            var member = ConvrMemberExistsResult.Check(this.m_MemberManager, staffId, conv.Id).Member;
            if (member == null) return new HttpNotFoundObjectResult(alarmId);

            var staffInOrgs = m_StaffManager.FetchStaffsByOrg(alarm.Staff.Org.Id, null);  

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(m_Config["AzureSettings:StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("convlogs");

            var base64ParKey = Convert.ToBase64String(Encoding.Default.GetBytes(conv.Id.PadRight(32, '0')));
            var beginTs = GetTimestamp(alarm.CreatedAt);
            var keyFilter = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, base64ParKey);
            var bgFilter = TableQuery.GenerateFilterCondition("From",
                QueryComparisons.NotEqual, "Background");
            var beginTimeFilter = TableQuery.GenerateFilterConditionForLong("TimeStamp",
                QueryComparisons.GreaterThan, beginTs);

            var combine = TableQuery.CombineFilters(keyFilter, TableOperators.And, bgFilter);

            combine = TableQuery.CombineFilters(combine, TableOperators.And, beginTimeFilter);

            if (member.ClearLogAt != null)
            {
                long ts = GetTimestamp(member.ClearLogAt.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.GreaterThanOrEqual, ts));
            }

            if (alarm.ClosedAt.HasValue)
            {
                long ts = GetTimestamp(alarm.ClosedAt.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.LessThanOrEqual, ts));
            }

            if (time.HasValue)
            {
                long ts = GetTimestamp(time.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.GreaterThanOrEqual, ts));
            }

            var query = new TableQuery<ConvMessageModel>().Where(combine);

            if (pageSize.HasValue)
                query = query.Take(pageSize);

            var result = table.ExecuteQuery(query).ToList(); 

            if (result.Any())
            {
                return new ObjectResult(result.OrderBy(p => p.Time).Select(p => p.ToViewModel(staffInOrgs)));
            }

            return new HttpNotFoundObjectResult(alarmId);
        } 

        [HttpGet("api/im/FetchMessagesByConvIdWithContent")]
        public IActionResult FetchMessagesByConvIdWithContent(Guid staffId, string convId, string content, int? pageSize,
            DateTime? time)
        {
            if (convId == "undefined" || convId == "null")
                convId = string.Empty;
            Args.NotEmpty(convId, nameof(convId));

            var staff = StaffExistsResult.Check(m_StaffManager, staffId).Staff;
            if (staff == null) return new HttpNotFoundObjectResult(staffId);

            var staffInOrgs = m_StaffManager.FetchStaffsByOrg(staff.Org.Id, null);

            if (pageSize.HasValue && !time.HasValue)
                time = DateTime.Now;

            var member = ConvrMemberExistsResult.Check(this.m_MemberManager, staffId, convId.Substring(0, 24)).Member;

            if (member == null)
                return new HttpNotFoundObjectResult(convId);

            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(m_Config["AzureSettings:StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("convlogs");


            var base64ParKey = Convert.ToBase64String(Encoding.Default.GetBytes(convId.PadRight(32, '0')));

            var keyFilter = TableQuery.GenerateFilterCondition("PartitionKey",
                QueryComparisons.Equal, base64ParKey);
            var bgFilter = TableQuery.GenerateFilterCondition("From",
                QueryComparisons.NotEqual, "Background");
            var combine = TableQuery.CombineFilters(keyFilter, TableOperators.And, bgFilter);
            if (member.ClearLogAt != null)
            {
                long ts = GetTimestamp(member.ClearLogAt.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.GreaterThan, ts));
            }

            if (time.HasValue)
            {
                long ts = GetTimestamp(time.Value);
                combine = TableQuery.CombineFilters(combine, TableOperators.And,
                    TableQuery.GenerateFilterConditionForLong("TimeStamp", QueryComparisons.LessThan, ts));
            }
            var query = new TableQuery<ConvMessageModel>().Where(combine);

            var message = table.ExecuteQuery(query).ToList();

            message = message.Where(p => p.Data.Contains("\"Alarm\":null") && p.Data.Contains("\"_lctype\":-1"))
                .Where(p => JsonConvert.DeserializeObject<MsgData>(p.Data).LcText.Contains(content))
                .OrderByDescending(p => p.Time)
                .ToList();


            if (message.Any())
                return
                    new ObjectResult(message.Select(p => p.ToViewModel(staffInOrgs))
                        .AsQueryable()
                        .ToPagedList(time, pageSize));


            return new HttpNotFoundObjectResult(convId);
        }

        [HttpPost("api/im/RemoveConvLog")]
        public IActionResult RemoveConvLog(Guid staffId, string convId, bool unique = false)
        {
            m_MemberManager.ClearLog(convId, staffId);
            return new HttpStatusCodeResult(204);
        } 

        private long GetTimestamp(DateTime time)
        {
            return (long) (time.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
        }
    }
}
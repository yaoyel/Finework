using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppBoot.Common;
using AppBoot.Security.Crypto;
using AVOSCloud;
using AVOSCloud.RealtimeMessageV2;
using FineWork.Colla;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FineWork.Net.IM
{
    public class LCIMService : IIMService
    {
        public LCIMService(IConfiguration config)
        {
            if (config == null)
                return;
            m_Config = config;
            m_AppId = config["LeanCloud:AppId"];
            m_AppMaster = config["LeanCloud:MasterKey"];
            m_AppKey = config["LeanCloud:AppKey"];
            AVClient.Initialize(m_AppId, m_AppMaster);
        }

        public LCIMService(string lcId, string lcKey, string lcMaster)
        {
            m_AppId = lcId;
            m_AppMaster = lcMaster;
            m_AppKey = lcKey;
        }

        private readonly string m_AppKey;
        private readonly string m_AppId;
        private readonly string m_AppMaster;
        private readonly IConfiguration m_Config;
        private const string m_FetchLogUrl = "https://leancloud.cn/1.1/rtm/messages/logs";


        public async Task<IEnumerable<AVIMConversation>> FecthConversationsByStaffIdAsync(Guid staffId)
        {
            var creatorClient = new AVIMClient(staffId.ToString()); 
                  
            var conQuery = creatorClient.GetQuery();

            var queryCondition = conQuery.WhereContains("m", staffId.ToString()).Limit(int.MaxValue);

            return await queryCondition.FindAsync();
        }

        public async Task<string> CreateConversationAsync(string creatorId, string clientId, string name,
            IDictionary<string, object> attrs)
        {
            Args.NotEmpty(creatorId, nameof(creatorId));
            Args.NotEmpty(clientId, nameof(clientId));

            var creatorClient = new AVIMClient(creatorId);
            if (!string.IsNullOrEmpty(name))
            {
                if (attrs != null)
                {
                    var conversationWithAttrs = await creatorClient.CreateConversationAsync(clientId, name, attrs);
                    return conversationWithAttrs.ConversationId;
                }
                var conversationWithName = await creatorClient.CreateConversationAsync(clientId, name);
                return conversationWithName.ConversationId; 
            }

            var conversation = await creatorClient.CreateConversationAsync(clientId);
            return conversation.ConversationId;
        }

        public async Task<string> CreateConversationAsync(string creatorId, IList<string> clientIds, string name,
            IDictionary<string, object> attrs, bool transient = false)
        {
            Args.NotEmpty(creatorId, nameof(creatorId));

            var creatorClient = new AVIMClient(creatorId);
            var conversation = creatorClient.CreateConversationAsync(clientIds, name, attrs, transient).Result;

            //新建的shine会议室默认预警数量加1
            if (attrs != null)
            {
                object chatRoomKind;
                var hasChatRoomKindAttr = attrs.TryGetValue("ChatRoomKind", out chatRoomKind);
                if (hasChatRoomKindAttr && chatRoomKind.ToString() == ((int)ChatRoomKinds.Shine).ToString())
                    await SetAttribute(conversation.ConversationId, "AlarmsCount", "Add");
            }


            return conversation.ConversationId;
        }


        public async Task<bool> SendTextMessageAsync(string clientId, string conversationId, string textContent,
            IDictionary<string, object> attrs, bool transient = false, bool receipt = false)
        {
            Args.NotEmpty(clientId, nameof(clientId));
            Args.NotEmpty(conversationId, nameof(conversationId));
            Args.NotEmpty(textContent, nameof(textContent));

            var currentClient = new AVIMClient(clientId);
            var conversation = currentClient.GetConversationById(conversationId);

            var message = new AVIMTextMessage();
            message.Attributes = attrs;
            message.TextContent = textContent;
            message.Receipt = receipt;
            message.Transient = transient;

            var result = await conversation.SendTextMessageAsync(message);
            return result.Item1;
        }

        public async Task<bool> SendImageMessageAsync(string clientId, string conversationId, string name,
            Stream imageStream, IDictionary<string, object> attrs)
        {
            Args.NotEmpty(clientId, nameof(clientId));
            Args.NotEmpty(conversationId, nameof(conversationId));
            Args.NotNull(imageStream, nameof(imageStream));
            Args.NotEmpty(name, nameof(name));


            var currentClient = new AVIMClient(clientId);
            var conversation = currentClient.GetConversationById(conversationId);
            var imageMessage = new AVIMImageMessage(name, imageStream);
            imageMessage.Attributes = attrs;
            var result = await conversation.SendImageMessageAsync(imageMessage);
            return result.Item1;
        }

        public async Task<bool> SendAudioMessageAsync(string clientId, string conversationId, string name,
            Stream audioStream,
            IDictionary<string, object> attrs)
        {
            Args.NotEmpty(clientId, nameof(clientId));
            Args.NotEmpty(conversationId, nameof(conversationId));
            Args.NotNull(audioStream, nameof(audioStream));


            var currentClient = new AVIMClient(clientId);

            var conversation = currentClient.GetConversationById(conversationId);
            var audioMessage = new AVIMAudioMessage(name, audioStream);
            audioMessage.Attributes = attrs;

            var result = await conversation.SendAudioMessageAsync(audioMessage);
            return result.Item1;
        }

        public  async Task<bool> AddMemberAsync(string creatorId, string conversationId, params string[] clientId)
        {
            Args.NotNull(creatorId, nameof(creatorId));
            Args.NotNull(conversationId, nameof(conversationId));
            Args.NotNull(clientId, nameof(clientId));
            var creatorClient = new AVIMClient(creatorId);
            var conversation = creatorClient.GetConversationById(conversationId);

            //在shine会议室中，聊天人员
            return await conversation.AddMembersAsync(clientId);
        }

        public async Task<bool> RemoveMemberAsync(string creatorId, string conversationId, params string[] clientId)
        {
            Args.NotNull(creatorId, nameof(creatorId));
            Args.NotNull(conversationId, nameof(conversationId));
            Args.NotNull(clientId, nameof(clientId));
            var creatorClient = new AVIMClient(creatorId);
            var conversation = creatorClient.GetConversationById(conversationId);
            return await conversation.RemoveMembersAsync(clientId);
        }

        public async Task<bool> ChangeConversationMemberAsync(string clientId, string conversationId, string[] members)
        {
            var creatorClient = new AVIMClient(clientId);
            var conversation = creatorClient.GetConversationById(conversationId);

            
            if (conversation != null)
            {
                var memberIds = conversation.MemberIds;
                if (memberIds.Except(members).Count() != 0 || members.Except(memberIds).Count() != 0)
                {
                    await conversation.RemoveMembersAsync(memberIds);
                   return await conversation.AddMembersAsync(members);
                }
            }
            return await Task.FromResult(false);
        }

        public async Task<bool> ChangeConversationNameAsync(string clientId, string conversationId, string name)
        {
            var creatorClient = new AVIMClient(clientId);
            var conversation = creatorClient.GetConversationById(conversationId);
            if (conversation != null)
            {
                if (conversation.Name != name)
                {
                    conversation.Name = name;
                    await conversation.SaveAsync();
                    return await Task.FromResult(true);
                }

            }
            return await Task.FromResult(false);
        }


        public async Task ChangeConAttrAsync(string creatorId, string conversationId, string key, object value)
        {
            Args.NotNull(creatorId, nameof(creatorId));
            Args.NotNull(conversationId, nameof(conversationId));
            Args.NotNull(key, nameof(key));
            await SetAttribute(conversationId, key, value);
        } 

        /// <summary>
        /// 在不能确认发送方的情况下使用，其他情况下尽量不使用此方法
        /// </summary>
        /// <param name="taskId"></param> 
        /// <param name="title"></param>
        /// <param name="conversationId"></param>
        /// <param name="accountId"></param>
        /// <param name="textMessage"></param>
        /// <param name="transient"></param>
        /// <returns></returns>
        public async Task<bool> SendTextMessageByConversationAsync(Guid taskId,Guid accountId, string conversationId, string title,
            string textMessage,
            bool transient = false)
        {
            var url = m_Config["LeanCloud:Urls:SendMessage"];
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", m_AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", $"{m_AppMaster},master");

            var message = new
            {
                _lctype = -1,
                _lctext = textMessage,
                _lcattrs = new Dictionary<string, object>()
                { 
                    ["AccountId"]=accountId.ToString(),
                    ["TaskId"] = taskId.ToString(),
                    ["Title"] = title
                }
            };
            var request = await client.PostAsJsonAsync(url, new
            {
                from_peer = "Background",
                message = JsonConvert.SerializeObject(message),
                conv_id = conversationId,
                transient = transient
            });
            return (int) request.StatusCode == 200;
        }

        public async Task<string> FetchMessageByConversationAsync(string convId, int limit, long maxts = 0L, string msgId = "")
        {
            var urlBuilder =new UriBuilder( m_Config["LeanCloud:Urls:FetchMessages"]);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-LC-Id", m_AppId);
            client.DefaultRequestHeaders.Add("X-LC-Key", $"{m_AppMaster},master");

            urlBuilder.Query= $"convid={convId}&limit={limit}";


            if (maxts != 0L)
                urlBuilder.Query+=$"&max_ts= {maxts}";
            if(!string.IsNullOrEmpty(msgId))
                urlBuilder.Query += $"&msgid= {msgId}";


            var result = await client.GetAsync(urlBuilder.Uri);
            return await result.Content.ReadAsStringAsync();
        }
        public static string StrToMD5(string str)
        {
            byte[] data = Encoding.GetEncoding("GB2312").GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] outBytes = md5.ComputeHash(data);

            string outString = "";
            for (int i = 0; i < outBytes.Length; i++)
            {
                outString += outBytes[i].ToString("x2");
            }

            return outString.ToLower();
        }
        public async Task<string> FetchMessageAsync(int limit, long maxts)
        {
            var urlBuilder = new UriBuilder(m_Config==null?m_FetchLogUrl: m_Config["LeanCloud:Urls:FetchMessages"]);
         
            var time = DateTime.Now.ToUniversalTime();
            long unixTimestamp = (long)(time.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
            var md5 = StrToMD5(string.Concat(unixTimestamp.ToString(), m_AppKey));

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("Application/x-www-form-urlencoded"));
            client.DefaultRequestHeaders.Add("X-LC-Id", m_AppId);
            client.DefaultRequestHeaders.Add("X-LC-Sign", $"{md5},{unixTimestamp}");
            client.DefaultRequestHeaders.Add("X-LC-Key", $"{m_AppMaster},master");

            urlBuilder.Query = $"max_ts={maxts}&limit={limit}"; 

            var result = await client.GetAsync(urlBuilder.Uri);
            if(result.IsSuccessStatusCode)
            return await result.Content.ReadAsStringAsync();
            return await Task.FromResult(string.Empty);
        }

        public async Task RemoveConversationById(string convId)
        {
            Args.NotEmpty(convId, nameof(convId));

            var urlBuilder = new UriBuilder(string.Format( m_Config["LeanCloud:Urls:DeleteConv"]),convId);
            var client = new HttpClient(); 
            client.DefaultRequestHeaders.Add("X-LC-Id", m_AppId); 
            client.DefaultRequestHeaders.Add("X-LC-Key", m_AppKey);

            await client.DeleteAsync(urlBuilder.Uri); 
        }

        public async Task RemoveConversationByStaffIdAsync(string staffId, string taskId, bool isShineOnly = true)
        {
            Args.NotEmpty(staffId, nameof(staffId));
            Args.NotEmpty(taskId, nameof(taskId));

            var client = new AVIMClient(staffId);
            var conQuery = client.GetQuery();

            var queryCondition = conQuery.WhereEqualTo("attr.TaskId", taskId);
            if (isShineOnly)
                queryCondition = queryCondition.WhereEqualTo("attr.ChatRoomKind", (int) ChatRoomKinds.Shine);

            //自己创建的会议室
            var createdConQuery = queryCondition.WhereEqualTo("attr.Creator", staffId);

            //参与的会议室
            var joinedConQuery = queryCondition.WhereNotEqualTo("attr.Creator", staffId);

            var createdcons = (await createdConQuery.FindAsync()).ToList();
            var joinedcons = (await joinedConQuery.FindAsync()).ToList();

            //从参与的会议室退出
            if (joinedcons.Any())
            {
                joinedcons.ForEach(p => p.RemoveMembersAsync(staffId));
            }

            //删掉自己创建的会议室的所有成员
            if (createdcons.Any())
            {
                //createdcons.ForEach(p =>
                //{
                //    p.RemoveMembersAsync(p.MemberIds);

                //});

                //不删除聊天室，只退出
                createdcons.ForEach(p => p.RemoveMembersAsync(staffId));
            }

        }

        public async Task ChangeConversationNameByStaffAsync(string staffId, string oldStaffName,string newStaffName)
        {
            Args.NotEmpty(staffId, nameof(staffId)); 
            Args.NotEmpty(oldStaffName, nameof(oldStaffName));
            Args.NotEmpty(newStaffName, nameof(newStaffName));

            var conversations = await FetchConversationsByMemberIdAsync(staffId);
            if (conversations == null)   await Task.FromResult(0);

            conversations.ForEach(async (p) =>
            {
                var newName = ReplaceByPattern(p["name"].ToString(), oldStaffName, newStaffName);
                if (p["name"].ToString() != newName)
                {
                    p["name"] = newName;
                    await p.SaveAsync();
                }
            });
        }

        private string ReplaceByPattern(string replaceStr,string oldStr, string newStr)
        { 
            var pattern = $"(\\s{oldStr},)|({oldStr},)|(,{oldStr},)|(,{oldStr}\\()";
            var reg = new Regex(pattern);
             
            var match = reg.Match(replaceStr);

            if (!match.Success)
            { 
                return replaceStr;
            }

            var subReplace= replaceStr.Substring(match.Index, match.Length).Replace(oldStr, newStr);
         
            return replaceStr.Replace(match.Value, subReplace);
        }

        private void AddOrSubtractValue(string key, object value, IDictionary<string, object> attrs)
        {
            object count;
            var hasValue = attrs.TryGetValue(key, out count);
            if (hasValue)
            {
                var alarmCount = Convert.ToInt32(count);
                if (value.ToString() == "Add")
                    attrs[key] = Convert.ToInt32(count) + 1;
                else
                    attrs[key] = alarmCount == 0 ? 0 : alarmCount - 1;
            }
            else
            {
                if (value.ToString() == "Add")
                    attrs[key] = 1;
                else
                    attrs[key] = 0;
            }
        }

        private async Task SetAttribute(string conversationId, string key, object value)
        {
            Args.NotEmpty(conversationId, nameof(conversationId));
            Args.NotEmpty(key, nameof(key));

            var obj = AVObject.CreateWithoutData("_Conversation", conversationId);
            var conversation = await obj.FetchAsync();
            if (conversation == null)
                return;

            Dictionary<string, object> attrs;
            if (!conversation.TryGetValue("attr", out attrs))
            {
                return;
            }
            if (attrs != null)
            {
                if (key == "AlarmsCount" || key == "ResolvedCount")
                {
                    AddOrSubtractValue(key, value, attrs);
                }
                else
                {
                    attrs[key] = value;
                }

                await  obj.SaveAsync();
            }

        }

        private async Task<List<AVObject>> FetchConversationsByMemberIdAsync(string staffId)
        {
            Args.NotEmpty(staffId, nameof(staffId));

            var query = new AVQuery<AVObject>("_Conversation");
            query = query.WhereContains("m", staffId)
                .WhereEqualTo("attr.ChatRoomKind", (int) ChatRoomKinds.Shine);
            var objs = (await query.FindAsync()).ToList();

            if (objs.Any()) return await Task.FromResult<List<AVObject>>(objs);

            return await Task.FromResult<List<AVObject>>(null);
        }
    }

}

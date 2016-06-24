using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Net.IM
{
    /// <summary>
    /// Im服务，基于leancloud.RealTimemessageV2
    /// </summary>
    public  interface IIMService
    {

        /// <summary>
        /// 创建一个单人对话，针对客户端id发送消息
        /// 返回一个对话的id
        /// </summary>
        /// <param name="cteatorId">创建人客户端id</param>
        /// <param name="clientId">参与人客户端id</param>
        /// <param name="name"></param>
        /// <param name="attrs"></param>
        /// <returns>对话的id</returns>
        Task<string> CreateConversationAsync(string  cteatorId, string clientId, string name, IDictionary<string, object> attrs);


        /// <summary>
        /// 创建一个多人对话
        /// </summary>
        /// <param name="creatorId">创建人客户端id</param>
        /// <param name="clientId"></param>
        /// <param name="name"></param>
        /// <param name="attrs"></param>
        /// <param name="transient">是否是聊天室</param>
        /// <returns>对话的id</returns>
        Task<string> CreateConversationAsync(string creatorId,IList<string> clientId, string name, IDictionary<string, object> attrs,bool transient=false);

        /// <summary>
        /// 在已经存在一个对话的情况下，发送一个文本信息
        /// </summary>
        /// <param name="clientId">发送人客户端id</param>
        /// <param name="conversationId">对话的id</param>
        /// <param name="textContent">发送的文本内容</param>
        /// <param name="attrs">附加属性</param>
        /// <param name="transient">是否是暂态消息，默认为false</param>
        /// <param name="receipt"></param>
        /// <returns>是否发送成功</returns>
        Task<bool> SendTextMessageAsync(string clientId,string conversationId, string textContent, IDictionary<string,object> attrs, bool transient = false, bool receipt = false);


        /// <summary>
        /// 在已经存在一个对话的情况下, 发送一个图片信息
        /// </summary>
        /// <param name="clientId">发送人客户端id</param>
        /// <param name="conversationId">对话id</param>
        /// <param name="name">图片名称</param>
        /// <param name="imageStream">图片流</param>
        /// <param name="attrs">附加属性</param>
        /// <returns>是否发送成功</returns>
        Task<bool> SendImageMessageAsync(string clientId,string conversationId, string name, Stream imageStream,
            IDictionary<string, object> attrs);

        Task<bool> SendAudioMessageAsync(string clientId, string conversationId,string name, Stream audioStream,
            IDictionary<string, object> attrs);

        Task<bool> AddMemberAsync(string creatorId, string conversationId,params string[] clientId);

        Task<bool> RemoveMemberAsync(string creatorId, string conversationId, string clientId);
        
        Task<bool> ChangeConversationMemberAsync(string clientId,string conversationId, string[] members);

        Task<bool> ChangeConversationNameAsync(string clientId, string conversationId, string name);
 
        Task ChangeConAttr(string creatorId, string conversationId, string key, object value);

        Task<bool> SendTextMessageByConversationAsync(Guid taskId, Guid accountId,string conversationId,string title,string textMessage,
            bool transient = false);

        Task RemoveConversationAsync(string staffId,string taskId,bool isShineOnly=true);

        Task ChangeConversationNameByStaffAsync(string staffId,string oldStaffName, string newStaffName);

    }
}

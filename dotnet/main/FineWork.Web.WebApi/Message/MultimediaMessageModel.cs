using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using Microsoft.AspNet.Http;

namespace FineWork.Web.WebApi.Message
{
    public class MultimediaMessageModel
    {
        public Guid StaffId { get; set; }

        public string ConversationId { get; set; }

        public ChatRoomKinds Kind { get; set; } 
        /// <summary>
        /// 用户获取发送者的头像
        /// </summary>
        public Guid AccountId { get; set; }

        public string MsgType { get; set; }

        public string CustomId { get; set; }
        /// <summary>
        /// 区分个人/聊天室
        /// </summary>
        public Guid FromId { get; set; }

        public string Name { get; set; }

        public IFormFile File { get; set; } 
    }
}

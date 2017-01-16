using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Common;
using AVOSCloud.RealtimeMessageV2;
using FineWork.Common;
using FineWork.Net.IM;

namespace FineWork.Colla.Impls
{
    public static class IMServiceExtensions
    {
        internal static string CreateChatRoom(this IIMService imService, StaffEntity creator, IList<StaffEntity> members,
            TaskEntity task,
            TaskAlarmKinds alarmKind)
        {
            var attrs = new Dictionary<string, object>()
            {
                ["Creator"] = creator.Id.ToString(),
                ["ChatRoomKind"] = (short) ChatRoomKinds.Shine,
                ["TaskId"] = task.Id.ToString(),
                ["TaskName"] = task.Name,
                ["Progress"] = task.Progress,
                ["TaskAlarmKind"] = (int) alarmKind,
                ["LeaderStaffId"] = task.Partakers.First(p => p.Kind == PartakerKinds.Leader).Staff.Id.ToString(),
                ["ResolvedCount"] = 0,
                ["OrgId"]=creator.Org.Id.ToString()
            };

            var creatorId = creator.Id.ToString();
            var memberIds = members.Select(p => p.Id.ToString()).ToList();
            var chatRoomName = GetShineRoomName(creator,members, task);

            return imService.CreateConversationAsync(creatorId, memberIds, chatRoomName, attrs).Result;
        }

 
        /// <summary>
        /// 任务负责人，目标不清/方法不够/协同不足-->Tong
        /// 不开学/累趴了/能力差/制度不好/流程不畅/资源不足-->与指导者形成Shine
        /// </summary>
        /// <param name="imService"></param>
        /// <param name="creator"></param>
        /// <param name="task"></param>
        /// <param name="alarmKind"></param>
        /// <returns></returns>
        internal static string CreateChatRoomForLeader(this IIMService imService, StaffEntity creator, TaskEntity task, TaskAlarmKinds alarmKind)
        {
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(creator, nameof(creator));
            Args.NotNull(task, nameof(task));
            //目标不清，方法不够，协同不足返回进去tong会议室
            if (alarmKind.GetGroupName() == TaskAlarmFactor.Project)
            {
                ChangeChatRoomToTong(imService, creator.Id.ToString(), task.Conversation.Id).Wait(); 

                //增加一个预警
                imService.AddTaskAlarms(creator.Id.ToString(), task.Conversation.Id).Wait();
                return task.Conversation.Id;
            }

            //其他的预警与指导者进入shine会议室
            var members = new List<StaffEntity>() { creator };
            var mentor = task.Partakers.Where(p => p.Kind == PartakerKinds.Mentor).ToList();
            if (!mentor.Any())
                throw new FineWorkException("请为任务添加指导者。");
            members.AddRange(mentor.Select(p=>p.Staff));

            return CreateChatRoom(imService, creator, members, task, alarmKind); 
         
        }

        /// <summary>
        /// 所有预警与负责人形成Shine
        /// </summary>
        /// <param name="imService"></param>
        /// <param name="creator"></param>
        /// <param name="task"></param>
        /// <param name="alarmKind"></param>
        /// <returns></returns>
        internal static string CreateChatRoomForMentor(this IIMService imService, StaffEntity creator, TaskEntity task,
            TaskAlarmKinds alarmKind)
        {
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(creator, nameof(creator));
            Args.NotNull(task, nameof(task));

            var members = new List<StaffEntity>() { creator };
            var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
            members.Add(leader.Staff);
              
            return CreateChatRoom(imService, creator, members, task, alarmKind);
        }


        /// <summary>
        /// 任务负责人，目标不清/方法不够/协同不足-->Tong
        /// 不开学/累趴了/能力差/制度不好/流程不畅/资源不足-->与负责人形成Shine
        /// </summary>
        /// <param name="imService"></param>
        /// <param name="creator"></param>
        /// <param name="task"></param>
        /// <param name="alarmKind"></param>
        /// <returns></returns>
        internal static string CreateChatRoomForCollaborator(this IIMService imService, StaffEntity creator,
            TaskEntity task, TaskAlarmKinds alarmKind)
        {
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(creator, nameof(creator));
            Args.NotNull(task, nameof(task));

            if (alarmKind.GetGroupName() == TaskAlarmFactor.Project)
            {
                imService.ChangeChatRoomToTong( creator.Id.ToString(), task.Conversation.Id).Wait();
                //增加一个预警
                imService.AddTaskAlarms(creator.Id.ToString(), task.Conversation.Id).Wait();
                return task.Conversation.Id;
            }

            var members = new List<StaffEntity>() {creator};
            var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
            members.Add(leader.Staff);

            return CreateChatRoom(imService, creator, members, task,alarmKind);
        }


        /// <summary>
        /// 接受者所有预警与负责人形成Shine
        /// </summary>
        /// <param name="imService"></param>
        /// <param name="creator"></param>
        /// <param name="task"></param>
        /// <param name="alarmKind"></param>
        /// <returns></returns>
        internal static string CreateChatRoomForRecipient(this IIMService imService, StaffEntity creator,
            TaskEntity task, TaskAlarmKinds alarmKind)
        {
            //任务接受者所有预警与任务管理者进行沟通
            Args.NotNull(imService, nameof(imService));
            Args.NotNull(creator, nameof(creator));
            Args.NotNull(task, nameof(task));
             

            var members = new List<StaffEntity>() { creator };
            var leader = task.Partakers.First(p => p.Kind == PartakerKinds.Leader);
            members.Add(leader.Staff);

            return CreateChatRoom(imService, creator, members, task, alarmKind); 
        }

        #region 修改聊天室属性
        public static async Task ChangeTaskNameAsync(this IIMService imService, string creator,TaskEntity task,string taskName)
        {
            //修改所有与之相关聊天室的名称
            var creatorClient = new AVIMClient(creator);
            var query = creatorClient.GetQuery();
            var findConversations = (query.WhereEqualTo("attr.TaskId", task.Id.ToString())).FindAsync();

            await imService.ChangeConAttrAsync(creator, task.Conversation.Id, "TaskName", taskName);
            var conversations= (await findConversations).AsParallel().ToList(); 
          
            if (conversations.Any())
            {
                conversations.ForEach(p =>
                {
                    if (Convert.ToInt32(p.Attributes["ChatRoomKind"]) == (int) ChatRoomKinds.Tong)
                        p.Name = taskName;
                    else
                        p.Name = p.Name.Replace(task.Name,taskName);
                    p.SaveAsync();
                });
            }
        }

        private static async Task ChangeChatRoomToTong(this IIMService imService, string creator, string conversationId)
        {
            await imService.ChangeConAttrAsync(creator, conversationId, "ChatRoomKind", (short)ChatRoomKinds.Tong);
        }

        public static   Task ChangeTaskProgressAsync(this IIMService imService, string creator, string conversationId,int progress)
        {
             return Task.Factory.StartNew(()=> imService.ChangeConAttrAsync(creator, conversationId, "Progress", progress));
        }

        public static async Task ChangeTaskLeader(this IIMService imService, string creator, string conversationId,string leaderId)
        {
            await imService.ChangeConAttrAsync(creator, conversationId, "LeaderStaffId", leaderId);
        }

        public static async Task CloseTaskAlarms(this IIMService imService, string creator, string conversationId)
        {
            await imService.ChangeConAttrAsync(creator, conversationId, "AlarmsCount", "subtract");
            await imService.ChangeConAttrAsync(creator, conversationId, "ResolvedCount", "subtract");
        }

        public static async Task ResolveTaskAlarms(this IIMService imService, string creator, string conversationId)
        {
            await imService.ChangeConAttrAsync(creator, conversationId, "ResolvedCount", "Add");
        }

        public static async Task AddTaskAlarms(this IIMService imService, string creator, string conversationId)
        { 
            await imService.ChangeConAttrAsync(creator, conversationId, "AlarmsCount", "Add"); 
        }

        public static async Task ChangeConversationNameAsync(this IIMService imService,string convId,StaffEntity creator, IList<StaffEntity> members, TaskEntity task)
        {
            var convName = GetShineRoomName(creator, members, task);
            await imService.ChangeConversationNameAsync(creator.Id.ToString(),convId,convName);
        }

        #endregion 
        private static string GetShineRoomName(StaffEntity creator,IList<StaffEntity> members, TaskEntity task)
        {
            Args.NotNull(members, nameof(members));
            Args.NotNull(task, nameof(task));
            if (members.Count() > 2)
            {
                members.Remove(creator);
               
                var firstMember = members.OrderBy(p=>p.Id).First(p => p.Id != creator.Id); 
                return string.Concat( creator.Name, ",",firstMember.Name, "...", $"({task.Name})");
            }

            return string.Concat(string.Join(",",members.Select(p => p.Name).ToArray()), $"({task.Name})");
        } 
    }
}

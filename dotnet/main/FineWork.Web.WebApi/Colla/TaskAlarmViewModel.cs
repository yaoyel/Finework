using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;
using FineWork.Colla.Checkers;
using FineWork.Web.WebApi.Common;
using Newtonsoft.Json;

namespace FineWork.Web.WebApi.Colla
{
    public class TaskAlarmViewModel
    {
        [Necessity]
        public string AlarmKind { get; set; }

        [Necessity]
        public ResolveStatus ResolveStatus { get; set; }

        public Guid Id { get; set; }
         
        public string Content { get; set; }
          
        //自定义预警描述
        public string AlarmDesc { get; set; }

        [Necessity]
        public int AlarmKindKey { get; set; } 

        public bool IsCommunicating { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        [Necessity(NecessityLevel.Low)]
        public TaskViewModel Task { get; set; }

        public StaffViewModel StaffFrom { get; set; }

        public string ConversationId { get; set; }

        public List<StaffViewModel> StaffTo { get; set; }
 
        public virtual void AssignFrom(TaskAlarmEntity entity, bool isShowhighOnly = false, bool isShowLow = true)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            #region 赋值
            var propertiesDic = new Dictionary<string, Func<TaskAlarmEntity, dynamic>>
            {
                ["AlarmKind"] = (t) => t.TaskAlarmKind.GetLabel(),
                ["ResolveStatus"] = (t)=>t.ResolveStatus,
                ["Id"] = (t) => t.Id,
                ["AlarmKindKey"] =(t) => (int)t.TaskAlarmKind,
                ["ResolvedAt"] = (t) =>t.ResolvedAt,
                ["Comment"] = (t) =>t.Comment,
                ["Content"] = (t) => t.Content,
                ["AlarmDesc"]=(t)=>t.AlarmDesc,
                ["CreatedAt"] = (t) => t.CreatedAt,
                ["Task"] = (t) => t.Task.ToViewModel(),
                ["StaffFrom"] = (t) => t.Staff.ToViewModel(isShowhighOnly,isShowLow),
                ["ClosedAt"] = (t) => t.ClosedAt,
                ["ConversationId"]=(t)=>t.Conversation.Id,
                ["StaffTo"] = (t) =>
                {
                    if (!t.Conversation.Members.Any())
                        return null;
                    return t.Conversation.Members.GroupBy(p=>p.Staff.Id).Select(p=>p.First()).Where(p=>p.Staff.Id!=t.Staff.Id).Select(p => new StaffViewModel()
                    {
                        Id=p.Staff.Id,
                        Name = p.Staff.Name
                    }).ToList();
                } 
            }; 
            #endregion

            NecessityAttributeUitl<TaskAlarmViewModel, TaskAlarmEntity>.SetVuleByNecssityAttribute(this, entity,
                propertiesDic, isShowhighOnly, isShowLow);

        }
    }

    public static class TaskAlarmViewModelExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isShowhighOnly">是否只显示标记为high的字段</param>
        /// <param name="isShowLow">标记为low的字段是否需要显示</param>
        /// <returns></returns>
        public static TaskAlarmViewModel ToViewModel(this TaskAlarmEntity entity,bool isShowhighOnly=false,bool isShowLow=true)
        {
            var result = new TaskAlarmViewModel();
            result.AssignFrom(entity,isShowhighOnly,isShowLow);
            return result;
        }
 
    }
}
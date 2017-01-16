using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{  
    public class AlarmViewModel
    {
        public Guid Id { get; set; }

        public int Weekdays { get; set; }

        public string ShortTime { get; set; }

        public bool IsEnabled { get; set; }

        public string Bell { get; set; }

        public TaskViewModel Task { get; set; }

        public int[] ReceiverKinds { get; set; }

        public Guid[] ReceiverStaffIds { get; set; } 

        public string Content { get; set; }

        public bool IsRepeat { get; set; }

        public DateTime? NoRepeatTime { get; set; }

        public AlarmTempKinds TempletKind { get; set; }

        public string[] DaysInMonth { get; set; } 

        public int? AttSize { get; set; }
        public virtual void AssignFrom(AlarmEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id; 
            this.IsEnabled = entity.IsEnabled;
            this.Task = entity.Task.ToViewModel();
            this.Bell = entity.Bell;
            this.IsRepeat = entity.IsRepeat;
            this.ShortTime = entity.ShortTime;
            if (entity.IsRepeat)
            { 
                this.DaysInMonth = string.IsNullOrEmpty(entity.DaysInMonth) ? null : entity.DaysInMonth.Split(',');
                this.Weekdays = entity.Weekdays; 
            }
            else
            { 
                this.NoRepeatTime = entity.NoRepeatTime;
            }
            this.Content = entity.Content; 
            this.TempletKind = entity.TempletKind; 
            this.AttSize = entity.AttSize;
       
            if (!string.IsNullOrEmpty(entity.ReceiverStaffIds))
                this.ReceiverStaffIds =Array.ConvertAll(entity.ReceiverStaffIds.Split(','),Guid.Parse);
            else
            {
                if (!string.IsNullOrEmpty(entity.ReceiverKinds))
                    this.ReceiverKinds =Array.ConvertAll(entity.ReceiverKinds.Split(','),int.Parse);
            }

        }
    }

    public class ConversationViewModel
    {
        public string ConversationId { get; set; }

        public bool IsEnd { get; set; } 

        public int Alarms { get; set; }

        public int Resolveds { get; set; }

        public int Closeds { get; set; }

        public virtual void AssignFromTask(TaskEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity)); 

            var alarms = entity.Conversation.TaskAlarms.ToList();
            ConversationId = entity.ConversationId;
            IsEnd = entity.Report != null; 
            Alarms = alarms.Count();
            Resolveds = alarms.Count(p => p.ResolveStatus == ResolveStatus.Resolved);
            Closeds = alarms.Count(p => p.ResolveStatus == ResolveStatus.Closed);
        }
        public virtual void AssignFromAlarm(TaskAlarmEntity entity)
        { 
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var alarms = entity.Conversation.TaskAlarms.ToList();
            ConversationId = entity.Conversation.Id; 
            Alarms = alarms.Count();
            Resolveds = alarms.Count(p => p.ResolveStatus == ResolveStatus.Resolved);
            Closeds = alarms.Count(p => p.ResolveStatus == ResolveStatus.Closed);
        }
    }

    public static class ConversationViewModelExtensions
    {
        public static ConversationViewModel ToConversationViewModel(this TaskEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new ConversationViewModel();
            result.AssignFromTask(entity);
            return result;
        }

        public static ConversationViewModel ToConversationViewModel(this TaskAlarmEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new ConversationViewModel();
            result.AssignFromAlarm(entity);
            return result;
        }
    }

    public static class AlarmPeriodViewModelExtensions
    {
        public static AlarmViewModel ToViewModel(this AlarmEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new AlarmViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

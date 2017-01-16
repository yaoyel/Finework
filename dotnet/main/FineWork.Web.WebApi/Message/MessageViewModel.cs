using System;
using System.Collections.Generic;
using System.Linq;
using FineWork.Colla;
using FineWork.Colla.Models;
using FineWork.Net.IM;
using Newtonsoft.Json;

namespace FineWork.Web.WebApi.Message
{
    public class MessageViewModel:ModelBase
    {
        public string ConvId { get; set; }

        public string MsgId { get; set; }

        public string Data { get; set; }

        public string From { get; set; } 

        public virtual void AssignFrom(ConvMessageModel entity,IList<StaffEntity> staffs )
        {
            var staff = staffs.FirstOrDefault(p => p.Id == new Guid(entity.From));
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.ConvId = entity.ConvId;
            this.MsgId = entity.MsgId;
            this.Data = entity.Data;
            this.From = staff?.Name;
            if (entity.Time != null) Time = new DateTimeOffset( entity.Time.Value).DateTime;
        }
    }

    public static class MessageViewModelExtensions
    {
        public static MessageViewModel ToViewModel(this ConvMessageModel entity,IList<StaffEntity> staffs )
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new MessageViewModel();
            result.AssignFrom(entity,staffs);
            return result;
        }
    }
}
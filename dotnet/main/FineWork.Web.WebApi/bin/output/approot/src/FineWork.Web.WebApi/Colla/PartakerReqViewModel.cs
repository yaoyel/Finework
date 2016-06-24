using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FineWork.Colla;

namespace FineWork.Web.WebApi.Colla
{
    public class PartakerReqViewModel
    {
        public Guid Id { get; set; }

        /// <summary> 任务. </summary>
        public TaskViewModel Task { get; set; }

        /// <summary> 申请人. </summary>
        public StaffViewModel Staff { get; set; }

        /// <summary> 成员资格类型 </summary>
        public PartakerKinds PartakerKind { get; set; }

        /// <summary> 申请内容. </summary>
        public String Message { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual void AssignFrom(PartakerReqEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            this.Id = entity.Id;
            this.Task = entity.Task.ToViewModel();
            this.Staff = entity.Staff.ToViewModel();
            this.PartakerKind = entity.PartakerKind;
            this.Message = entity.Message;
            this.CreatedAt = entity.CreatedAt;
        }
    }

    public static class PartakerReqViewModelExtensions
    {
        public static PartakerReqViewModel ToViewModel(this PartakerReqEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            var result = new PartakerReqViewModel();
            result.AssignFrom(entity);
            return result;
        }
    }
}

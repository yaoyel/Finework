using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class CreateTaskAlarmModel
    {
        public Guid TaskId { get; set; }

        public Guid CreatorId { get; set; }

        //0.9.0
        public Guid StaffId { get; set; }

        public TaskAlarmKinds AlarmKind { get; set; }

        public string Content { get; set; }

        /// <summary>
        /// 自定义的预警需要对预警进行描述
        /// </summary>
        public string AlarmDesc { get; set; }
 
        //老版本
        public int[] Receivers { get; set; }

        /// <summary>
        /// 选择与那些角色进行沟通
        /// </summary>
        public int[] PartakerKinds{ get; set; }   

        /// <summary>
        /// 选择的成员
        /// </summary>
        public Guid[]  StaffIds { get; set; } 

        public Guid? AlarmId { get; set; }
    }
}

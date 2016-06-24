using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
     public class StaffAlarmsModel
     {
         public IEnumerable<TaskAlarmEntity> SendOuts { get; set; } = new List<TaskAlarmEntity>(); 

        public IEnumerable<TaskAlarmEntity> Receiveds { get; set; } = new List<TaskAlarmEntity>();

        public IEnumerable<TaskAlarmEntity> Others { get; set; } = new List<TaskAlarmEntity>();
    }
}

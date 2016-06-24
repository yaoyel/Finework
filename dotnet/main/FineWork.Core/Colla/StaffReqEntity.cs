using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using FineWork.Security.Repos.Aef;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace FineWork.Colla
{
    //Add some comment to test GIT


    public class StaffReqEntity : EntityBase<Guid>
    {
        public virtual OrgEntity Org { get; set; }

        public virtual AccountEntity Account { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ReviewStatuses ReviewStatus { get; set; }

        public DateTime ReviewAt { get; set; }

        //[Timestamp]
        //public byte[] RowVer { get; set; }

    }
}

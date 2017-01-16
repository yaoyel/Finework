using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class VoteModel
    {
        [Required(ErrorMessage = "请输入共识主题")]
        [MaxLength(128, ErrorMessage = "共识主题不能大于128字符")]
        public string Subject { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        public bool IsMultiEnabled { get; set; } = false;

        public bool IsAnonEnabled { get; set; } = false;

        public Guid CreatorStaffId { get; set; }
    }

    public class CreateVoteModel:VoteModel
    { 
        public IList<CreateVoteOptionModel> VoteOptions { get; set; } = new List<CreateVoteOptionModel>();
    }

    public class UpdateVoteModel:VoteModel
    {
        public  Guid VoteId { get; set; }

        public IList<UpdateVoteOptionModel> VoteOptions { get; set; }=new List<UpdateVoteOptionModel>();
    }

}

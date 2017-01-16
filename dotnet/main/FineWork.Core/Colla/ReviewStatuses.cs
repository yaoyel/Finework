using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    public enum ReviewStatuses
    {
        [Display(Name ="未处理")]
        Unspecified,

        [Display(Name ="同意")]
        Approved,

        [Display(Name ="拒绝")]
        Rejected
    }

    public enum AnncStatus
    {
        [Display(Name = "未处理")] 
        Unspecified,

        [Display(Name = "已达成")]
        Approved,

        [Display(Name = "延期")]
        Delay,

        [Display(Name = "放弃")]
        Abandon, 
    }

    public static class ReviewStatusesExtensions
    {
        public static string GetLabel(this ReviewStatuses kind)
        {
            return EnumUtil.GetLabel(kind);
        }
    }
}
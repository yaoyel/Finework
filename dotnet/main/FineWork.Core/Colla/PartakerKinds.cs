using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    /// <summary> 代表任务相关者与任务间的关系. </summary>
    public enum PartakerKinds
    {
        [Display(Name = "未知")]
        Unspecified,
        [Display(Name = "协同者")]
        Collaborator,
        [Display(Name = "负责人")]
        Leader,
        [Display(Name = "指导者")]
        Mentor,
        [Display(Name = "接收者")]
        Recipient
    }

    public static class PartakerKindsExtensions
    {
        public static String GetLabel(this PartakerKinds kind)
        {
            return EnumUtil.GetLabel(kind);
        }
    }
}
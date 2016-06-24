using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{

    public class TaskAlarmFactor
    {
        public const string Project = "Project";
        public const string Human = "Human";
        public const string Company = "Company";
    }

    public enum TaskAlarmKinds
    {
        /// <summary>
        ///用户点击绿灯的时候，进入tong会议室分享，系统指导用户进入对应的会议室，
        /// 还要记录用户点击绿灯的动作，判断用户是否处理了定时预警，
        /// 对应此类预警不应该出现在用户未处理的预警列表中
        /// </summary>
        [Display(Description ="绿灯表示不存在预警")]
        GreenLight= 0,

        [Display(Name ="目标不清", GroupName = TaskAlarmFactor.Project)]
        Target= 10,

        [Display(Name ="方法不够", GroupName = TaskAlarmFactor.Project)]
        Method,

        [Display(Name = "协同不足", GroupName = TaskAlarmFactor.Project)]
        Cooperation,

        [Display(Name = "不开心", GroupName = TaskAlarmFactor.Human)]
        Mood = 20,

        [Display(Name = "累趴了", GroupName = TaskAlarmFactor.Human)]
        PhysicalStrength,

        [Display(Name = "能力差", GroupName = TaskAlarmFactor.Human)]
        Ability,

        [Display(Name = "制度不好", GroupName = TaskAlarmFactor.Company)]
        System = 30,

        [Display(Name = "流程不畅", GroupName = TaskAlarmFactor.Company)]
        Procedure,

        [Display(Name = "资源不足", GroupName = TaskAlarmFactor.Company)]
        Resources,

        [Display(Name ="自定义预警")]
        Custom=9
    }

    public static class TaskAlarmKindsExtensions
    {
        public static String GetGroupName(this TaskAlarmKinds kind)
        {
            return EnumUtil.GetGroupName(kind);
        }

        public static String GetLabel(this TaskAlarmKinds kind)
        {
            return EnumUtil.GetLabel(kind);
        }
    }
}

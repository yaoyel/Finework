using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public enum AnnouncementKinds
    {
        [Display(Name = "通告")]
        Announcement = 1,

        [Display(Name = "承诺")]
        Promise,
         
        All
    }

    public static class AnnouncementKindsExtensions
    {
        public static String GetLabel(this AnnouncementKinds kind)
        {
            return EnumUtil.GetLabel(kind);
        }
    }
}
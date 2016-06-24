using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using FineWork.Common;

namespace FineWork.Colla
{
    [Flags]
    public enum WeekDays
    {
        [Display(Order =0)]
        Sunday = 1,

        [Display(Order = 1)]
        Monday = 2,

        [Display(Order = 2)]
        Tuesday = 4,

        [Display(Order = 3)]
        Wednesday = 8,

        [Display(Order = 4)]
        Thursday = 16,

        [Display(Order = 5)]
        Friday = 32,

        [Display(Order = 6)]
        Saturday = 64
    }

    public static class WeekDaysExtensions
    { 

        public static String GetOrder(this WeekDays kind)
        {
            return EnumUtil.GetOrder(kind);
        }
    }

}

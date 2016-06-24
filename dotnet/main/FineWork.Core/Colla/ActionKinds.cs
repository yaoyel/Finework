using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla
{
    public enum ActionKinds
    {
        //显示在任务中心页的日志类型
        [Display(Name ="inserttable")]
        InsertTable,
        [Display(Name = "updatetable")]
        UpdateTable,
        [Display(Name = "deletetable")]
        DeleteTable,

        //不在任务中心页显示
        [Display(Name = "insertcolumn")]
        InsertColumn,
        [Display(Name = "updatecolumn")]
        UpdateColumn,
        [Display(Name = "deletecolumn")]
        DeleteColumn
    }


    public static class ActionKindsExtensions
    {
        public static String GetLabel(this ActionKinds kind)
        {
            return EnumUtil.GetLabel(kind);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.KeyGenerators;
using FineWork.Common;

namespace FineWork.Settings.Repos.Aef
{
    public class SettingEntity : EntityBase<String>, ISetting
    {
        public virtual String Value { get; set; }

        /// <summary> Database row version for concurrency checking. </summary>
        [Timestamp]
        public virtual byte[] RowVer { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class MomentExistsResult:FineWorkCheckResult
    {
        public MomentExistsResult(bool isSucceed, String message, MomentEntity  moment)
            : base(isSucceed, message)
        {
            this.Moment = moment;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public MomentEntity Moment { get; private set; }

        public static MomentExistsResult Check(IMomentManager momentManager, Guid momentId)
        {
            if (momentManager == null) throw new ArgumentNullException(nameof(momentManager));
            var mement = momentManager.FindMementById(momentId);
            return Check(mement, "不存在对应的共享信息.");
        }

        private static MomentExistsResult Check(MomentEntity moment, string message)
        {
            if (moment == null)
            {
                return new MomentExistsResult(false, message, null);
            }
            return new MomentExistsResult(true, null, moment);
        }


    }
}
 

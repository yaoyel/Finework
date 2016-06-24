using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    class MomentLikeExistsResult : FineWorkCheckResult
    {
        public MomentLikeExistsResult(bool isSucceed, String message, MomentLikeEntity momentLike)
            : base(isSucceed, message)
        {
            this.MomentLike = momentLike;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public MomentLikeEntity MomentLike { get; private set; }

        public static MomentLikeExistsResult Check(IMomentLikeManager momentLikeManager, Guid momentLikeId)
        {
            if (momentLikeManager == null) throw new ArgumentNullException(nameof(momentLikeManager));
            var mementLike = momentLikeManager.FindMomentLikeById(momentLikeId);
            return Check(mementLike, "不存在对应的赞信息.");
        }

        public static MomentLikeExistsResult CheckByStaff(IMomentLikeManager momentLikeManager, Guid momentId,Guid staffId)
        {
            if (momentLikeManager == null) throw new ArgumentNullException(nameof(momentLikeManager));
            var mementLike = momentLikeManager.FindMomentLikeByStaffId(momentId,staffId);
            return Check(mementLike, "不存在对应的赞信息.");
        }

        private static MomentLikeExistsResult Check(MomentLikeEntity momentLike, string message)
        {
            if (momentLike == null)
            {
                return new MomentLikeExistsResult(false, message, null);
            }
            return new MomentLikeExistsResult(true, null, momentLike);
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class MomentCommentExistsResult: FineWorkCheckResult
    {
        public MomentCommentExistsResult(bool isSucceed, String message, MomentCommentEntity momentComment)
            : base(isSucceed, message)
        {
            this.MomentComment = momentComment;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public MomentCommentEntity MomentComment { get; private set; }

        public static MomentCommentExistsResult Check(IMomentCommentManager momentCommentManager, Guid momentCommentId)
        {
            if (momentCommentManager == null) throw new ArgumentNullException(nameof(momentCommentManager));
            var mement = momentCommentManager.FineMomentCommentById(momentCommentId);
            return Check(mement, "不存在对应的留言信息.");
        }

        private static MomentCommentExistsResult Check(MomentCommentEntity momentComment, string message)
        {
            if (momentComment == null)
            {
                return new MomentCommentExistsResult(false, message, null);
            }
            return new MomentCommentExistsResult(true, null, momentComment);
        }
    }
}

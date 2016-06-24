using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;

namespace FineWork.Colla.Checkers
{
    public class MomentFileExistsResult : FineWorkCheckResult
    {
        public MomentFileExistsResult(bool isSucceed, String message, MomentFileEntity momentFile)
            : base(isSucceed, message)
        {
            this.MomentFile = momentFile;
        }

        /// <summary> 若检查<b>不</b>通过，则包含相应的 <see cref="StaffEntity"/>, 否则为 <c>null</c>. </summary>
        public MomentFileEntity MomentFile { get; private set; }

        public static MomentFileExistsResult Check(IMomentFileManager momentFileManager, Guid momentFileId)
        {
            if (momentFileManager == null) throw new ArgumentNullException(nameof(momentFileManager));
            var mementFile = momentFileManager.FindMomentFileById(momentFileId);
            return Check(mementFile, "文件不存在.");
        }

        private static MomentFileExistsResult Check(MomentFileEntity momentFile, string message)
        {
            if (momentFile == null)
            {
                return new MomentFileExistsResult(false, message, null);
            }
            return new MomentFileExistsResult(true, null, momentFile);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    class AnncAttExistsResult:FineWorkCheckResult
    {
        public AnncAttExistsResult(bool isSucceed, String message, AnncAttEntity anncAttEntity)
            : base(isSucceed, message)
        {
            this.AnncAtt = anncAttEntity;
        }

        public AnncAttEntity AnncAtt { get; set; }
        public static AnncAttExistsResult Check(IAnncAttManager anncAttManager, Guid anncId,Guid taskSharingId,bool isAchv)
        {
            var anncAtt = anncAttManager.FindAnncAttByAnncAndSharingId(anncId,taskSharingId,isAchv);

            var attType = isAchv ? "成果" : "资源";
            return Check(anncAtt, $"{attType}不存在.");
        }


        private static AnncAttExistsResult Check([CanBeNull]  AnncAttEntity anncAttEntity, String message)
        {
            if (anncAttEntity == null)
            {
                return new AnncAttExistsResult(false, message, null);
            }

            return new AnncAttExistsResult(true, null, anncAttEntity);
        }
    }
}

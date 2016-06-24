using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class AnncIncentiveExistsResult : FineWorkCheckResult
    {
        public AnncIncentiveExistsResult(bool isSucceed, String message, AnncIncentiveEntity anncIncentiveEntity)
            : base(isSucceed, message)
        {
            this.AnncIncentiveEntity = anncIncentiveEntity;
        }

        public AnncIncentiveEntity AnncIncentiveEntity { get; set; }

        public static AnncIncentiveExistsResult CheckByAnncIdAndKind(IAnncIncentiveManager anncIncentiveManager,
            Guid anncId, int kind)
        {
            var anncIncentive = anncIncentiveManager.FindAnncIncentiveByAnncIdAndKind(anncId, kind);
            return Check(anncIncentive, "激励不存在.");
        }


        private static AnncIncentiveExistsResult Check([CanBeNull] AnncIncentiveEntity anncIncentiveEntity,
            String message)
        {
            if (anncIncentiveEntity == null)
            {
                return new AnncIncentiveExistsResult(false, message, null);
            }

            return new AnncIncentiveExistsResult(true, null, anncIncentiveEntity);
        }
    }
}
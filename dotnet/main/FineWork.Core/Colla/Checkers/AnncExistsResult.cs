using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class AnncExistsResult: FineWorkCheckResult
    {
        public AnncExistsResult(bool isSucceed, String message, AnnouncementEntity anncEntity)
            : base(isSucceed, message)
        {
            this.Annc= anncEntity;
        }

        public AnnouncementEntity Annc { get; set; }
        public static AnncExistsResult Check(IAnnouncementManager anncManager, Guid anncId)
        {
            var annc = anncManager.FindAnncById(anncId);
            return Check(annc, "计划不存在.");
        }
         

        private static AnncExistsResult Check([CanBeNull]  AnnouncementEntity anncEntity, String message)
        {
            if (anncEntity == null)
            {
                return new AnncExistsResult(false, message, null);
            }

            return new AnncExistsResult(true, null, anncEntity);
        }
    }
}

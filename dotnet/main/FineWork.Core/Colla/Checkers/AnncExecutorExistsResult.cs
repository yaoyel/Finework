using System;
using FineWork.Common;
using JetBrains.Annotations;

namespace FineWork.Colla.Checkers
{
    public class AnncExecutorExistsResult : FineWorkCheckResult
    {
        public AnncExecutorExistsResult(bool isSucceed, String message, AnncExecutorEntity anncExecutor)
            : base(isSucceed, message)
        {
            this.AnncExecutor = anncExecutor;
        }

        public AnncExecutorEntity AnncExecutor { get; set; }
        public static AnncExecutorExistsResult Check(IAnncExecutorManager anncExecutorManager,Guid anncId,Guid staffId)
        {
            var anncExecutor = anncExecutorManager.FindByAnncIdWithStaffId(anncId,staffId);
            return Check(anncExecutor, null);
        }

        
        private static AnncExecutorExistsResult Check([CanBeNull]  AnncExecutorEntity anncExecutor, String message)
        {
            if (anncExecutor == null)
            {
                return new AnncExecutorExistsResult(false, message, null);
            }

            return new AnncExecutorExistsResult(true, null, anncExecutor);
        }
    }
}
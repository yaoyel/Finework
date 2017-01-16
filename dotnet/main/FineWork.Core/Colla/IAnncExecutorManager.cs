using System;
using FineWork.Colla.Checkers;

namespace FineWork.Colla
{
    public interface IAnncExecutorManager
    {
        AnncExecutorEntity CreateAnncExecutor(Guid anncId, Guid staffId,bool checkPartaker=true);

        void DeleteAnncExecutor(Guid anncId, Guid staffId);

        AnncExecutorEntity FindByAnncIdWithStaffId(Guid anncId, Guid staffId);

        void DeleteAnncExecutorByAnncId(Guid anncId);
    }
}
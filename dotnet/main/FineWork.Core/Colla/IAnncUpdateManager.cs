using System;

namespace FineWork.Colla
{
    public interface IAnncUpdateManager
    {
        void CreateAnncUpdate(Guid anncId,Guid staffId);

        void DeleteAnncUpdatesByAnncId(Guid anncId);
    }
}
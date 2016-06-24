using System;
using AppBoot.Repos;

namespace FineWork.Security.Repos
{
    public interface IAccountRepository : IRepository<IAccount, Guid>
    {
        IAccount CreateNew();

        IAccount Create(Guid id);
    }
}

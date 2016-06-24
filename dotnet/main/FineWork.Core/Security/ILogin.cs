using System;
using AppBoot.Repos;

namespace FineWork.Security
{
    public interface ILogin
    {
        Guid Id { get; }

        String Provider { get; set; }

        String ProviderKey { get; set; }

        IAccount Account { get; set; }
    }
}
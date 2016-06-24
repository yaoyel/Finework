using System;
using AppBoot.Repos;
using JetBrains.Annotations;

namespace FineWork.Security
{
    public interface IClaim
    {
        Guid Id { get; }

        String ClaimType { get; set; }

        String ClaimValue { get; set; }

        #region Navigation Properties

        IAccount Account { get; set; }

        #endregion
    }
}
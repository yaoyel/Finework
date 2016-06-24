using System;
using AppBoot.Repos;

namespace FineWork.Settings.Repos
{
    public interface ISettingRepository : IRepository<ISetting, String>
    {
        ISetting Create(String id);
    }
}
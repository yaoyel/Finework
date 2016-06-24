using System;
using AppBoot.KeyGenerators;
using AppBoot.Repos;
using AppBoot.Repos.Inmem;
using AppBoot.Repos.KeyGenerators;
using FineWork.Settings.Repos.Aef;

namespace FineWork.Settings.Repos.Fakes
{
    public class FakeSettingRepository
        : InmemUpcastRepository<ISettingItem, String, SettingItemEntity>, ISettingRepository
    {
        public FakeSettingRepository()
            : base(RepoUtil.KeyAccessorFor<SettingItemEntity, String>(), GuidFormatKeyGenerator.Default)
        {
        }
    }
}

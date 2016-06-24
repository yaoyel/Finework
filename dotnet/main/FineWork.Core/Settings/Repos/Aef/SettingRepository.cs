using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.KeyGenerators;
using AppBoot.Repos;
using AppBoot.Repos.Adapters;
using AppBoot.Repos.Aef;

namespace FineWork.Settings.Repos.Aef
{
    public class SettingRepository
        : AefUpCastRepository<ISetting, String, SettingEntity>
        , ISettingRepository
    {
        public SettingRepository(ISessionProvider<AefSession> session)
            : base(session)
        {
        }

        public ISetting Create(String id)
        {
            return new SettingEntity {Id = id};
        }
    }
}

using System;
using AppBoot.Common;
using AppBoot.Data;
using FineWork.Core;
using FineWork.Repos.Aef;
using NUnit.Framework;

namespace FineWork.Settings
{
    [TestFixture]
    public class SettingManagerTests : FineWorkTestBase
    {
        [Test]
        public void SetValue_should_InsertNewValue_ForNewOne()
        {
            var id = Guid.NewGuid().ToString();

            using (var tx = TxManager.Acquire())
            {
                using (var ctx = Engine.CreateContext())
                {
                    var settingManager = ctx.Resolve<ISettingManager>();

                    var oldValue = settingManager.GetValue(id);
                    Assert.IsNull(oldValue);

                    settingManager.SetValue(id, id.ToUpper());

                    var afterSet = settingManager.GetValue(id);
                    Assert.AreEqual(id.ToUpper(), afterSet);

                    settingManager.DeleteValue(id);

                    var afterDelete = settingManager.GetValue(id);
                    Assert.IsNull(afterDelete);

                    ctx.SaveChanges();
                }

                tx.Complete();
            }
        }

        [Test]
        public void SetValue_should_UpdateValue_ForExistingOne()
        {
            var id = Guid.NewGuid().ToString();

            using (var tx = TxManager.Acquire())
            {
                using (var ctx = Engine.CreateContext())
                {
                    var settingManager = ctx.Resolve<ISettingManager>();

                    settingManager.SetValue(id, "Old");
                    settingManager.SetValue(id, "NEW");

                    var afterSet = settingManager.GetValue(id);
                    Assert.AreEqual("NEW", afterSet);

                    settingManager.DeleteValue(id);

                    ctx.SaveChanges();
                }

                tx.Complete();
            }
        }
    }
}

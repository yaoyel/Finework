using System;
using System.Configuration;
using System.IO;
using FineWork.Azure;
using FineWork.Files;
using Microsoft.WindowsAzure.Storage;
using NUnit.Framework;

namespace FineWork.Avatar
{
    [TestFixture]
    public class AvatarManagerTests
    {
        private String GetAzureConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["FineWorkAzureStorage"].ConnectionString;
        }

        [Test]
        public void AzureStorage_should_be_connectable()
        {
            var cs = this.GetAzureConnectionString();
            Console.WriteLine($"Windows Azure Connection String: {cs}");
            var account = CloudStorageAccount.Parse(cs);
            Assert.NotNull(account);
            Console.WriteLine(account.BlobStorageUri);
        }

        [Test]
        public void CreateAvatars_creates_account_avatars()
        {
            CreateAvatarsFor(KnownAvatarOwnerTypes.Accounts);
        }

        [Test]
        public void CreateAvatars_creates_org_avatars()
        {
            CreateAvatarsFor(KnownAvatarOwnerTypes.Orgs);
        }

        private void CreateAvatarsFor(String ownerType)
        {
            IFileManager fileManager = AzureTestUtil.CreateTestFileManager();
            IAvatarManager avatarManager = new AvatarManager(fileManager);

            Guid id = Guid.Empty;

            //创建头像文件
            String directory;
            using (var stream = File.Open("Avatar\\WangFei.1024x768.jpg", FileMode.Open))
            {
                directory = avatarManager.CreateAvatars(ownerType, id, stream);
            }

            //验证头像文件已经生成
            var files = fileManager.GetFiles(directory);
            Assert.AreEqual(Enum.GetValues(typeof (AvatarSizes)).Length, files.Length);
            foreach (var f in files)
            {
                Console.WriteLine(f);
            }

            //删除生成的头像并验证结果
            avatarManager.DeleteAvatars(ownerType, id);
            var filesAfterDelete = fileManager.GetFiles(directory);
            Assert.AreEqual(0, filesAfterDelete.Length);
        }
    }
}

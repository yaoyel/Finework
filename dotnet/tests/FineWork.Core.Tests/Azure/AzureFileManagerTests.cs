using System;
using System.IO;
using FineWork.Files;
using NUnit.Framework;

namespace FineWork.Azure
{
    [TestFixture]
    public class AzureFileManagerTests
    {
        /// <summary> 当路径不存在时， <see cref="IFileManager.GetFiles"/> 返回空数组. </summary>
        [Test]
        public void GetFiles_returns_empty_array_when_no_such_directory()
        {
            IFileManager fileManager = AzureTestUtil.CreateTestFileManager();
            var result = fileManager.GetFiles(Guid.NewGuid().ToString());
            Assert.NotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void GetFiles_returns_empty_array_when_path_is_a_file()
        {
            IFileManager fileManager = AzureTestUtil.CreateTestFileManager();

            var pathName = "test/" + Guid.NewGuid() + ".jpg";

            fileManager.CreateFile(pathName, "image/jpg", new MemoryStream());
            try
            {
                var result = fileManager.GetFiles(pathName);
                Assert.NotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                fileManager.DeleteFile(pathName);
            }
        }
    }
}

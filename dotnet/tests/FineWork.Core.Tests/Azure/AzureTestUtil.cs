using System.Configuration;

namespace FineWork.Azure
{
    public static class AzureTestUtil
    {
        public static AzureFileManager CreateTestFileManager()
        {
            var cs = ConfigurationManager.ConnectionStrings["FineWorkAzureStorage"].ConnectionString;
            return new AzureFileManager("test", cs, "public");
        }
    }
}
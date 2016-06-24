using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FineWork.Common;
using FineWork.Files;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace FineWork.Azure
{
    public class AzureFileManager : IFileManager
    {
        /// <summary> 创建一个实例. </summary>
        /// <param name="name"><see cref="INamedService.Name"/> 的值.</param>
        /// <param name="storageConnectionString">Azure Storage 的连接字符串.</param>
        /// <param name="containerName">Azure 的 Blob 容器的名称</param>
        public AzureFileManager(String name, String storageConnectionString, String containerName)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentException("Value is null or empty.", nameof(name));
            if (String.IsNullOrEmpty(storageConnectionString))
                throw new ArgumentException("Value is null or empty.", nameof(storageConnectionString));
            if (String.IsNullOrEmpty(containerName))
                throw new ArgumentException("containerName is null or empty.", nameof(containerName));
            this.Name = name;
            this.m_StorageConnectionString = storageConnectionString;
            this.ContainerName = containerName;
        }

        public String Name { get; private set; }

        private readonly String m_StorageConnectionString;

        public String ContainerName { get; private set; }

        private CloudBlobContainer GetContainer()
        {
            var account = CloudStorageAccount.Parse(m_StorageConnectionString);
            var client = account.CreateCloudBlobClient();

            var serviceProperties = client.GetServiceProperties();

            ConfigureCors(serviceProperties);

            client.SetServiceProperties(serviceProperties);

            var container = client.GetContainerReference(this.ContainerName);
            if (!container.Exists())
            {
                container.CreateIfNotExists();
                container.SetPermissions(new BlobContainerPermissions()
                {
                    PublicAccess = BlobContainerPublicAccessType.Container
                });
                //throw new InvalidOperationException($"The container {this.ContainerName} does NOT exist.");
            }

            return container;
        }


        private void ConfigureCors(ServiceProperties serviceProperties)
        {
            serviceProperties.Cors.CorsRules.Clear();
            serviceProperties.Cors = new CorsProperties();
            serviceProperties.Cors.CorsRules.Add(new CorsRule()
            {
                AllowedHeaders = new List<string>() { "*" },
                AllowedMethods = CorsHttpMethods.Get | CorsHttpMethods.Head | CorsHttpMethods.Post,
                AllowedOrigins = new List<string>() { "*" },
                MaxAgeInSeconds = 1800,
                ExposedHeaders = new List<string> { "*" }
            });
        }

        public void CreateFile(String pathName, String contentType, Stream stream)
        {
            var container = this.GetContainer();

            var blockBlob = container.GetBlockBlobReference(pathName);
            blockBlob.Properties.ContentType = contentType;
            stream.Position = 0;
            blockBlob.UploadFromStream(stream);
        }

        public String[] GetFiles(string path)
        {
            var container = this.GetContainer();

            var directory = container.GetDirectoryReference(path);
            var list = directory.ListBlobs(true);

            //ListBlobs 返回的 IListBlobItem.AbsolutePath 以 containerName 为前缀，
            //故需要将此前缀从 Uri 中去除以得到相对于容器的路径名
            var prefix = $"/{this.ContainerName}/";
            return list.Select(item => item.Uri.AbsolutePath.Substring(prefix.Length)).ToArray();
        }

        public bool DeleteFile(string pathName)
        {
            var container = GetContainer();

            var file = container.GetBlockBlobReference(pathName);
            var result = file.Exists();

            if (result)
            {
                file.Delete();
            }
            return result;
        }

        public void DeleteBlobDirectory(string directory)
        {

            var container = GetContainer();

            var blobs = container.ListBlobs(directory, true);
            foreach (var blob in blobs)
            {
                container.GetBlockBlobReference(((CloudBlockBlob)blob).Name).DeleteIfExists();
            }
        }


        public bool FileIsExists(string pathName)
        {
            var container = GetContainer();

            var file = container.GetBlockBlobReference(pathName);
            return file.Exists();
        }

        public void DownloadToStream(string pathName, Stream stream)
        {
            var container = GetContainer();

            var blob = container.GetBlockBlobReference(pathName);
            blob.DownloadToStream(stream);
        }

    }
}
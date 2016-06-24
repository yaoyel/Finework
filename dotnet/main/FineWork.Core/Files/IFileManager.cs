using System;
using System.IO;
using FineWork.Common;

namespace FineWork.Files
{
    public interface IFileManager : INamedService
    {
        /// <summary> 根据流的内容创建文件. </summary>
        /// <param name="pathName">文件在容器中的路径名（包括路径、文件名与扩展名）</param>
        /// <param name="contentType">内容的类型，如 "image/jpeg".</param>
        /// <param name="stream">文件的内容. 注意：由调用方负责对流进行 <see cref="IDisposable.Dispose"/>.</param>
        void CreateFile(String pathName, String contentType, Stream stream);

        /// <summary> 列出路径下的文件 </summary>
        /// <param name="path">路径</param>
        /// <returns>路径下的文件名</returns>
        String[] GetFiles(String path);

        /// <summary> 删除一个文件. </summary>
        /// <param name="pathName">文件在容器中的路径名（包括路径、文件名与扩展名）</param>
        /// <returns> 当文件被成功删除则返回 <c>true</c>， 若文件不存在则返回 <c>false</c>.</returns>
        bool DeleteFile(String pathName);

        bool FileIsExists(string pathName);

        void DownloadToStream(string pathName, Stream stream);
    }
}
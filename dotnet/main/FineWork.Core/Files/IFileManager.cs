using System;
using System.IO;
using FineWork.Common;

namespace FineWork.Files
{
    public interface IFileManager : INamedService
    {
        /// <summary> �����������ݴ����ļ�. </summary>
        /// <param name="pathName">�ļ��������е�·����������·�����ļ�������չ����</param>
        /// <param name="contentType">���ݵ����ͣ��� "image/jpeg".</param>
        /// <param name="stream">�ļ�������. ע�⣺�ɵ��÷������������ <see cref="IDisposable.Dispose"/>.</param>
        void CreateFile(String pathName, String contentType, Stream stream);

        /// <summary> �г�·���µ��ļ� </summary>
        /// <param name="path">·��</param>
        /// <returns>·���µ��ļ���</returns>
        String[] GetFiles(String path);

        /// <summary> ɾ��һ���ļ�. </summary>
        /// <param name="pathName">�ļ��������е�·����������·�����ļ�������չ����</param>
        /// <returns> ���ļ����ɹ�ɾ���򷵻� <c>true</c>�� ���ļ��������򷵻� <c>false</c>.</returns>
        bool DeleteFile(String pathName);

        bool FileIsExists(string pathName);

        void DownloadToStream(string pathName, Stream stream);
    }
}
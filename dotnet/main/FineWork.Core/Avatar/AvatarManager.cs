using System;
using System.IO;
using FineWork.Common;
using FineWork.Files;

namespace FineWork.Avatar
{
    /// <summary>
    /// 默认的 <see cref="IAvatarManager"/> 实现.
    /// </summary>
    public class AvatarManager : IAvatarManager
    {
        public AvatarManager(IFileManager fileManager)
        {
            if (fileManager == null) throw new ArgumentNullException(nameof(fileManager));
            this.m_FileManager = fileManager;
        }

        private readonly IFileManager m_FileManager;

        private static string GetAvatarDirectory(string ownerType, Guid ownerId)
        {
            return $"{ownerType}/{ownerId}/avatars";
        }

        public String CreateAvatars(String ownerType, Guid ownerId, Stream stream)
        {
            if (String.IsNullOrEmpty(ownerType)) throw new ArgumentException("Value is null or empty.", nameof(ownerType));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var directory = GetAvatarDirectory(ownerType, ownerId);

            foreach (var avatarSize in Enum.GetValues(typeof(AvatarSizes)))
            {
                var size = (int) avatarSize;
                
                //例如： accounts/00000000-0000-0000-0000-000000000000/avatars/132x132.jpg
                //或: orgs/00000000-0000-0000-0000-000000000000/avatars/132x132.jpg
                var pathName = $"{directory}/{size}x{size}.jpg";

                using (var avatarStream = ImageUtil.CompressImage(stream, (int) avatarSize))
                {
                    m_FileManager.CreateFile(pathName, "image/jpeg", avatarStream);
                }
            }

            return directory;
        }

        public void DeleteAvatars(string ownerType, Guid ownerId)
        {
            var directory = GetAvatarDirectory(ownerType, ownerId);
            var files = m_FileManager.GetFiles(directory);
            foreach (var f in files)
            {
                m_FileManager.DeleteFile(f);
            }
        }
    }
}
using System;
using System.IO;
using FineWork.Azure;

namespace FineWork.Avatar
{
    /// <summary> 头像管理. </summary>
    public interface IAvatarManager
    {
        /// <summary> 根据图片内容创建一组头像. </summary>
        /// <param name="ownerType">头像拥有者的类型，如 <c>accounts</c> 或 <c>orgs</c>. </param>
        /// <param name="ownerId">头像拥有者的 Id.</param>
        /// <param name="stream">原始的头像内容，必须是jpeg格式.</param>
        /// <returns> 头像所在的目录. </returns>
        /// <remarks> 本方法会以原始内容为基础，创建一组不同大小的头像. </remarks>
        String CreateAvatars(String ownerType, Guid ownerId, Stream stream);

        void DeleteAvatars(String ownerType, Guid ownerId);
    }

    /// <summary> 已知的头像拥有者的类型. </summary>
    public static class KnownAvatarOwnerTypes
    {
        public static readonly String Accounts = "accounts";
        public static readonly String Orgs = "orgs";
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AppBoot.Repos;
using JetBrains.Annotations;

namespace FineWork.Security
{
    public interface IAccount
    {
        Guid Id { get; }

        /// <summary> 用户名 </summary>
        /// <remarks> 其值 <see cref="Name"/> 应当在系统中是唯一的.</remarks>
        [Required]
        [DisplayName("用户名")]
        String Name { get; set; }

        /// <summary> 密码格式 </summary>
        String PasswordFormat { get; set; }

        /// <summary> 密码 </summary>
        /// <remarks> The password may be <c>null</c> if an account is registered with an external login.</remarks>
        [DataType(DataType.Password)]
        String Password { get; set; }

        /// <summary> 密码附加串. </summary>
        String PasswordSalt { get; set; }

        /// <summary> 电子邮件 </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        String Email { get; set; }

        /// <summary> 电子邮件是否已经确认. </summary>
        /// <remarks> 通过向 <see cref="Email"/> 发送“确认邮件”, 用户点击邮件中的链接以确认其邮件. </remarks>
        bool IsEmailConfirmed { get; set; }

        /// <summary> 电话号码 </summary>
        [DataType(DataType.PhoneNumber)]
        String PhoneNumber { get; set; }

        /// <summary> 电话是否已经确认. </summary>
        /// <remarks> 通过向 <see cref="PhoneNumber"/> 发送“验证码”, 用户通过在系统中输入验证码以确认其电话. </remarks>
        bool IsPhoneNumberConfirmed { get; set; }

        bool IsTwoFactorEnabled { get; set; }

        /// <summary> 是否被锁定 </summary>
        /// <remarks> 当值从 <c>true</c> 变为 <c>false</c>，即锁定解除时，<see cref="PasswordFailedCount"/> 会被重置为 0.</remarks>
        bool IsLocked { get; set; }

        /// <summary> 锁定截止时间. </summary>
        DateTime? LockEndAt { get; set; }

        //TODO: Rename to AccessFailedCount, as the count includes the TwoFactor failed count.
        /// <summary> 密码错误的次数. </summary>
        /// <remarks> 当 <see cref="IsLocked"/> 值从 <c>true</c> 变为 <c>false</c>，即锁定解除时，<see cref="PasswordFailedCount"/> 会被重置为 0.</remarks>
        int PasswordFailedCount { get; set; }

        /// <summary> 安全戳. </summary>
        /// <remarks> 当密码或 <see cref="Logins"/> 发生变化时会更新此属性的值. </remarks>
        String SecurityStamp { get; set; }

        /// <summary> 创建时间 </summary>
        /// <remarks> 在对象创建之后即不再更改. </remarks>
        DateTime CreatedAt { get; set; }
         
        #region Navigation Properties

        /// <summary> 拥有的登录账号. </summary>
        /// <remarks> 一个用户可以有不同的登录账号 </remarks>
        ICollection<ILogin> Logins { get; }

        /// <summary> 拥有的角色. </summary>
        ICollection<IRole> Roles { get; }

        ICollection<IClaim> Claims { get; } 

        #endregion
    }
}

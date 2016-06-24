using System;

namespace AppBoot.Security.Passwords
{
    /// <summary> 代表密码的存储信息. </summary>
    public interface ITransformedPassword
    {
        /// <summary> 经转换后的密码. </summary>
        String Transformed { get; }

        /// <summary> 用于增加保密性的字符串. </summary>
        String Salt { get; }
    }
}
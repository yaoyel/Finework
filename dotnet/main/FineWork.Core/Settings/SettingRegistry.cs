using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FineWork.Settings
{
    /// <summary> 登记通过 <see cref="ISettingManager" /> 管理的配置项. </summary>
    /// <remarks>
    ///     它用于:
    ///     <list type="bullet">
    ///         <item>避免程序中意外声明了重复的配置项标识</item>
    ///         <item>便于在系统初始化过程中处理配置项及其初始值.</item>
    ///     </list>
    /// </remarks>
    public class SettingRegistry
    {
        public static readonly SettingRegistry Instance = new SettingRegistry();
        private readonly ConcurrentDictionary<String, String> m_Registry = new ConcurrentDictionary<String, String>();

        private SettingRegistry()
        {
        }

        /// <summary> 注册新的配置项. </summary>
        /// <returns> 参数 <paramref name="id" /> 的值. </returns>
        public String Register(String id, [CanBeNull] String defaultValue)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentException("id is null or empty.", "id");

            bool isAdded = m_Registry.TryAdd(id, defaultValue);
            if (!isAdded)
                throw new ArgumentException(
                    String.Format("The id [{0}] has been registered before.", id));
            return id;
        }

        /// <summary> 返回所有的项. </summary>
        /// <returns>
        ///     数组中每个元素的 <see cref="KeyValuePair{T1,T2}.Key" /> 为配置项的标识,
        ///     <see cref="KeyValuePair{T1,T2}.Value" /> 为配置项的初始值.
        /// </returns>
        public KeyValuePair<String, String>[] GetAll()
        {
            KeyValuePair<string, string>[] items = m_Registry.ToArray();
            return items;
        }
    }
}
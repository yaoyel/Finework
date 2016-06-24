using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace FineWork.Common
{
	/// <summary> 一组枚举类型的辅助方法. </summary>
	public static class EnumUtil
	{
		private static readonly Object m_Lock = new Object();

		/// <summary> Returns a typed array for enum members. </summary>
		[NotNull]
		public static T[] GetValues<T>() where T : struct
		{
			CheckIsEnum<T>();
			var values = Enum.GetValues(typeof(T));
			return values.Cast<T>().ToArray();
		}

		#region Enum display label

		/// <summary> 获取枚举值 <paramref name="value"/> 用于显示的标签。 </summary>
		/// <typeparam name="T">枚举类型</typeparam>
		/// <param name="value">枚举值</param>
		/// <returns>若枚举值有 <see cref="DisplayAttribute"/>，则返回其 <see cref="DisplayAttribute.Name"/>， 
		/// 否则返回该枚举值的变量名。</returns>
		public static String GetLabel<T>(T value) where T : struct
		{
			CheckIsEnum<T>();

			IDictionary<T, String> names = InternalGetLabelMap<T>();
			return names[value];
		}

	    public static string GetGroupName<T>(T value) where T : struct
	    {
	        CheckIsEnum<T>();

            IDictionary<T, String> groupNames = InternalGetGroupNameMap<T>();
            return groupNames[value]; 
        }

        public static string GetOrder<T>(T value) where T : struct
        {
            CheckIsEnum<T>();

            IDictionary<T, String> orders = InternalGetOrderMap<T>();
            return orders[value];
        }

        /// <summary> 为枚举类型缓存其枚举值的标签，其键值为枚举的类型，其值为包含该枚举类型所有枚举值的显示标签的字典。 </summary>
        private static readonly Dictionary<string, Object> m_LabelsCache = new Dictionary<string, Object>();

		private static IDictionary<T, String> InternalGetLabelMap<T>() where T : struct
		{
			var t = typeof(T).ToString()+"/Lable";
			Object result;
			if (m_LabelsCache.TryGetValue(t, out result) == false)
			{
				lock (m_Lock)
				{
					if (m_LabelsCache.TryGetValue(t, out result) == false)
					{
						IDictionary<T, String> labels = InternalCreateLabelMap<T>();
						result = new ReadOnlyDictionary<T, String>(labels);
						m_LabelsCache.Add(t, result);
					}
				}
			}
			return (IDictionary<T, String>)result;
		}

        private static IDictionary<T, String> InternalGetGroupNameMap<T>() where T : struct
        {
            var t = typeof(T)+ "/GroupName";
            Object result;
            if (m_LabelsCache.TryGetValue(t, out result) == false)
            {
                lock (m_Lock)
                {
                    if (m_LabelsCache.TryGetValue(t, out result) == false)
                    {
                        IDictionary<T, String> labels = InternalCreateGroupNameMap<T>();
                        result = new ReadOnlyDictionary<T, String>(labels);
                        m_LabelsCache.Add(t, result);
                    }
                }
            }
            return (IDictionary<T, String>)result;
        }

        private static IDictionary<T, String> InternalGetOrderMap<T>() where T : struct
        {
            var t = typeof(T) + "/Order";
            Object result;
            if (m_LabelsCache.TryGetValue(t, out result) == false)
            {
                lock (m_Lock)
                {
                    if (m_LabelsCache.TryGetValue(t, out result) == false)
                    {
                        IDictionary<T, String> labels = InternalCreateOrderMap<T>();
                        result = new ReadOnlyDictionary<T, String>(labels);
                        m_LabelsCache.Add(t, result);
                    }
                }
            }
            return (IDictionary<T, String>)result;
        }

        /// <summary> 为枚举类型 <typeparamref name="T"/> 创建名称字典。 </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <remarks>一个<see cref="IDictionary{TKey, TValue}"/>, 其 <see cref="IDictionary{TKey, TValue}.Keys"/>为枚举值, <see cref="IDictionary{TKey, TValue}.Values"/> 为相对应的名称.</remarks>
        private static IDictionary<T, String> InternalCreateLabelMap<T>() where T : struct
		{
			CheckIsEnum<T>();

			Dictionary<T, String> result = new Dictionary<T, String>();

			FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo field in fields)
			{
				T key = (T)field.GetValue(typeof(T));

				Object[] arr = field.GetCustomAttributes(typeof(DisplayAttribute), false);
				DisplayAttribute displayValue = arr.Length > 0 ? (DisplayAttribute)arr[0] : null;
				String value = (displayValue != null) ? displayValue.GetName() : field.Name;

				result.Add(key, value);
			}
			return result;
		}

        private static IDictionary<T, String> InternalCreateGroupNameMap<T>() where T : struct
        {
            CheckIsEnum<T>();

            Dictionary<T, String> result = new Dictionary<T, String>();

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                T key = (T)field.GetValue(typeof(T));

                Object[] arr = field.GetCustomAttributes(typeof(DisplayAttribute), false);
                DisplayAttribute displayValue = arr.Length > 0 ? (DisplayAttribute)arr[0] : null;
                String value = (displayValue != null) ? displayValue.GetGroupName() : field.Name;

                result.Add(key, value);
            }
            return result;
        }
        private static IDictionary<T, String> InternalCreateOrderMap<T>() where T : struct
        {
            CheckIsEnum<T>();

            Dictionary<T, string> result = new Dictionary<T, string>();

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                T key = (T)field.GetValue(typeof(T));

                Object[] arr = field.GetCustomAttributes(typeof(DisplayAttribute), false);
                DisplayAttribute displayValue = arr.Length > 0 ? (DisplayAttribute)arr[0] : null;
                var  value = displayValue?.GetOrder().ToString() ?? field.Name;

                result.Add(key, value);
            }
            return result;
        }

        /// <summary> 获取枚举值的名称字典. </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <remarks>一个<see cref="IDictionary{TKey, TValue}"/>, 其 <see cref="IDictionary{TKey, TValue}.Keys"/>为枚举值, <see cref="IDictionary{TKey, TValue}.Values"/> 为相对应的名称.</remarks>
        public static IDictionary<T, String> GetLabelMap<T>() where T : struct
		{
			IDictionary<T, String> internalMap = InternalGetLabelMap<T>();
			Dictionary<T, String> result = new Dictionary<T, string>(internalMap);
			return result;
		}

		#endregion

		public static void CheckIsEnum<T>() where T : struct 
		{
			Type t = typeof(T);
			if (t.IsEnum == false) throw new ArgumentException(String.Format("The type {0} is not an enum type.", t));
		}

		/// <summary> Creates an exception that indicates the enum value is not handled. </summary>
		/// <typeparam name="T"> The enum type. </typeparam>
		/// <param name="value"> The enum value. </param>
		/// <returns> An instance of <see cref="NotSupportedException"/>. </returns>
		/// <exception cref="ArgumentException"> if <typeparamref name="T"/> is not an enum type. </exception>
		public static NotSupportedException NotHandled<T>(T value) where T : struct
		{
			CheckIsEnum<T>();
			String s = String.Format("The enum value {0} of type {1} is not handled.", value, typeof(T).FullName);
			return new NotSupportedException(s);
		}
	}
}
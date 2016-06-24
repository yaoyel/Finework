using System;
using System.Text;
using JetBrains.Annotations;

namespace AppBoot.Common
{
    public static class Args
    {
        public static void NotNull(Object value, [InvokerParameterName] String name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void MaxLength(string value, int max, [InvokerParameterName] String name,string displayName)
        {
            if (Encoding.Default.GetByteCount(value)> max)
            {
                var paraName = string.IsNullOrEmpty(displayName) ? "输入内容" : displayName;
                throw new ArgumentException($"{paraName}不能超过{max}个字符",name);
            }
        }

        public static T NotNull<T>(T value, [InvokerParameterName] String name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
            return value;
        }

        public static void NotEmpty(String value, [InvokerParameterName] String name)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException($"Argument [{name}] is null or empty.", name);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBoot.Common
{
    /// <summary> Represents the format for converting a <see cref="Guid"/> to a <see cref="String"/>. </summary>
    /// <seealso cref="Guid.ToString(string)"/>
    public enum GuidFormats
    {
        Digits,
        HyphenDigits,
        BraceHyphenDigits,
        ParenthesesHyphenDigits,
        Hexadecimal
    }

    [ExcludeFromCodeCoverage]
    public static class GuidFormatsExtensions
    {
        /// <summary> Gets the format string from a <seealso cref="GuidFormats"/>. </summary>
        /// <returns> The string accepted by <seealso cref="Guid.ToString(String)"/></returns>
        public static String GetFormatString(this GuidFormats formats)
        {
            switch (formats)
            {
                case GuidFormats.Digits:
                    return "N";
                case GuidFormats.HyphenDigits:
                    return "D";
                case GuidFormats.BraceHyphenDigits:
                    return "B";
                case GuidFormats.ParenthesesHyphenDigits:
                    return "P";
                case GuidFormats.Hexadecimal:
                    return "X";
                default:
                    throw new NotSupportedException(String.Format("The Guid format [{0}] is not supported.", formats));
            }
        }
    }
}

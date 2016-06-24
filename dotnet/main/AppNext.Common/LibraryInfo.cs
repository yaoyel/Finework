using System;
using System.Diagnostics.CodeAnalysis;

namespace AppBoot
{
    namespace Common
    {
        /// <summary> A dummy class for checking this assembly has been successfully loaded. </summary>
        [ExcludeFromCodeCoverage()]
        public static class LibraryInfo
        {
            public static readonly String AssemblyName = "AppNext.Common";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FineWork.Web.WebApp.Models
{
    public class HomeModels
    {
    }

    /// <summary>
    /// A dummy class for checking class library references targeting framework DNX451.
    /// </summary>
    /// <remarks>
    /// Visual Studio 2015 RC will show errors when the target framework of the referenced class libraries are 4.5.
    /// </remarks>
    public static class LibraryChecker
    {
        public static String[] AssemblyNames
        {
            get
            {
                return new String[]
                {
                    AppBoot.Common.LibraryInfo.AssemblyName,
                    AppBoot.Data.LibraryInfo.AssemblyName,
                    AppBoot.Data.Aef.LibraryInfo.AssemblyName
                };
            }
        }
    }
}

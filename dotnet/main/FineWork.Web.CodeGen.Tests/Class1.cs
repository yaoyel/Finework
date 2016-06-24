using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FineWork.Web.CodeGen.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class HelloXunit
    {
        [Fact]
        public void Test()
        {
            Assert.Equal(1, 1);
        }
    }
}

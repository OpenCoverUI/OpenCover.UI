using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenCover.UI.TestDiscoverer.TestResources.xUnit
{
    public class RegularxUnitTestClass
    {

        [Fact]
        public void RegularTestMethod()
        {

        }

        public class SubTestClass
        {
            [Fact]
            public void RegularSubTestClassMethod()
            {
            }

            public class Sub2NdTestClass
            {
                [Fact]
                public void RegularSub2NdTestClassMethod()
                {
                }
            }


        }
    }
}



using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenCover.UI.TestDiscoverer.TestResources.MSTest
{
    [TestClass]
    public class RegularTestClass
    {
        [TestMethod]
        public void RegularTestMethod()
        {
        }

        [TestClass]
        public class SubTestClass
        {
            [TestMethod]
            public void RegularSubTestClassMethod()
            {
            }

            [TestClass]
            public class Sub2NdTestClass
            {
                [TestMethod]
                public void RegularSub2NdTestClassMethod()
                {
                }

            }
        }
    }
}
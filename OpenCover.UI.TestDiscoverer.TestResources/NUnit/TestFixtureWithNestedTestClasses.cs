using NUnit.Framework;

namespace OpenCover.UI.TestDiscoverer.TestResources.NUnit
{
    [TestFixture]
    public class TestFixtureWithNestedTestClasses
    {
        [Test]
        public void RegularTestMethod()
        {
        }

        [TestFixture]
        public class SubTestClass
        {
            [Test]
            public void RegularSubTestClassMethod()
            {
            }

            [TestFixture]
            public class Sub2NdTestClass
            {
                [Test]
                public void RegularSub2NdTestClassMethod()
                {
                }

            }
        }
    }
}

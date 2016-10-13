using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenCover.UI.TestDiscoverer.TestResources.MSTest
{
    [TestClass]
    public class TestFixtureWithTraits
    {
        [TestMethod]
        [TestCategory("Category_TestMethod_TestFixtureWithTraits")]
        public void TestMethod_TestFixtureWithTraits()
        {

        }
    }

    [TestClass]
    public class TestFixtureInheritingTraitMethods : TestFixtureWithTraits
    {
        [TestMethod]
        [TestCategory("Category_TestMethod_TestFixtureInheritingTraitMethods_1")]
        [TestCategory("Category_TestMethod_TestFixtureInheritingTraitMethods_2")]
        public void TestMethod_TestFixtureInheritingTraitMethods()
        {

        }
    }
}

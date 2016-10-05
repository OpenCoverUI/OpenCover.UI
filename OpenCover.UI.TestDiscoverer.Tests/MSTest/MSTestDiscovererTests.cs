using System;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.MSTest;

namespace OpenCover.UI.TestDiscoverer.Tests.MSTest
{
    [TestFixture]
    public class MSTestDiscovererTests : DiscovererTestsBase
    {
        [TestCase(typeof(RegularTestClass), "RegularTestMethod")]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod);
        }

        [TestCase(typeof(RegularTestClass.SubTestClass), "RegularSubTestClassMethod")]
        public void Discover_Finds_Sub_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod);
        }
    }
}
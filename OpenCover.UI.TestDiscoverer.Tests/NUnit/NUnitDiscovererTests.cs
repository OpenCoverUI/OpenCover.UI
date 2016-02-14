using System;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.NUnit;

namespace OpenCover.UI.TestDiscoverer.Tests.NUnit
{
    public class NUnitDiscovererTests : DiscovererTestsBase
    {
        [TestCase(typeof(RegularTestFixture), "RegularTestMethod")]
        [TestCase(typeof(TestFixtureWithoutExplicitTestFixtureAttribute), "TestMethodInTestFixtureWithoutExplicitTestFixtureAttribute")]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod);
        }
    }
}
using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.MSTest;

namespace OpenCover.UI.TestDiscoverer.Tests.MSTest
{
    [TestFixture]
    public class MSTestDiscovererTests : DiscovererTestsBase
    {
        [TestCase(typeof(RegularTestClass), "RegularTestMethod", null)]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod, IEnumerable<string> expectedTraits)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, expectedTraits);
        }
    }
}
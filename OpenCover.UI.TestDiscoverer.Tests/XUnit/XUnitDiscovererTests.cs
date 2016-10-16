using System;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.Xunit;

//using OpenCover.UI.TestDiscoverer.TestResources.Xunit;

namespace OpenCover.UI.TestDiscoverer.Tests.XUnit
{
    
    public class XUnitDiscovererTests: DiscovererTestsBase
    {
        [TestCase(typeof(RegularFacts), "RegularTestMethod")]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod);
        }
    }
}

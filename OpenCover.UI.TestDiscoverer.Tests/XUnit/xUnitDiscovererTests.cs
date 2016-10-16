using System;
using NUnit.Framework;
using OpenCover.UI.Model.Test;
using OpenCover.UI.TestDiscoverer.TestResources.Xunit;


namespace OpenCover.UI.TestDiscoverer.Tests.XUnit
{
    
    class XUnitDiscovererTests: DiscovererTestsBase
    {
        [TestCase(typeof(RegularFacts), "RegularTestMethod")]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.XUnit);
        }
      
        [TestCase(typeof(RegularFacts.SubTestClass), "RegularSubTestClassMethod")]
        [TestCase(typeof(RegularFacts.SubTestClass.Sub2NdTestClass), "RegularSub2NdTestClassMethod")]
        public void Discover_Finds_Sub_Test_Fixtures_And_Methods(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.XUnit);
        }


    }
}

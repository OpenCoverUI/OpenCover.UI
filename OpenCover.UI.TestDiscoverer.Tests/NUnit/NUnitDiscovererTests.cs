using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.NUnit;

namespace OpenCover.UI.TestDiscoverer.Tests.NUnit
{
    public class NUnitDiscovererTests : DiscovererTestsBase
    {
        [TestCase(typeof(RegularTestFixture), "RegularTestMethod", null)]
        [TestCase(typeof(TestFixtureWithoutExplicitTestFixtureAttribute), "TestMethodInTestFixtureWithoutExplicitTestFixtureAttribute", null)]
        [TestCase(typeof(TestFixtureWithTestCase), "SomeTestCase", null)]
        [TestCase(typeof(TestFixtureWithNamedTestCase), "SomeNamedTestCase", null)]
        [TestCase(typeof(TestFixtureWithInheritedTraitTestCase), "SomeTraitTestMethod", new string[] { "InheritedTrait", "ClassTrait" })]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod, IEnumerable<string> expectedTraits)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, expectedTraits);
        }
    }
}
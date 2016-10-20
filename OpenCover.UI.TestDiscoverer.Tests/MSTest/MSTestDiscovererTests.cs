using System;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.MSTest;
using OpenCover.UI.Model.Test;

namespace OpenCover.UI.TestDiscoverer.Tests.MSTest
{
    [TestFixture]
    class MSTestDiscovererTests : DiscovererTestsBase
    {
        [TestCase(typeof(RegularTestClass), "RegularTestMethod")]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.MSTest);
        }

        [TestCase(typeof(RegularTestClass.SubTestClass), "RegularSubTestClassMethod")]
        [TestCase(typeof(RegularTestClass.SubTestClass.Sub2NdTestClass), "RegularSub2NdTestClassMethod")]
        public void Discover_Finds_Sub_Test_Fixtures_And_Methods(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.MSTest);
        }

        [Ignore("MsTest Discoverer doesn't find base class methods yet"), 
            TestCase(typeof(TestFixtureInheritingTraitMethods), 
            "TestMethod_TestFixtureWithTraits", 
            new[] { "Category_TestMethod_TestFixtureWithTraits" })]
        public void Discover_Finds_Inherited_Fixtures_And_Methods_With_Traits(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod, string[] expectedTraits)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.MSTest, expectedTraits);
        }

        [TestCase(typeof(TestFixtureWithTraits), 
            "TestMethod_TestFixtureWithTraits", 
            new[] { "Category_TestMethod_TestFixtureWithTraits" })]
        [TestCase(typeof(TestFixtureInheritingTraitMethods), 
            "TestMethod_TestFixtureInheritingTraitMethods", 
            new[] { "Category_TestMethod_TestFixtureInheritingTraitMethods_1",
                    "Category_TestMethod_TestFixtureInheritingTraitMethods_2" })]
        public void Discover_Finds_Fixtures_And_Methods_With_Traits(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod, string[] expectedTraits)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.MSTest, expectedTraits);
        }
    }
}

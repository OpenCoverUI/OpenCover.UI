using System;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.NUnit;
using OpenCover.UI.Model.Test;

namespace OpenCover.UI.TestDiscoverer.Tests.NUnit
{
    class NUnitDiscovererTests : DiscovererTestsBase
    {
        [TestCase(typeof(RegularTestFixture), "RegularTestMethod")]
        [TestCase(typeof(TestFixtureWithoutExplicitTestFixtureAttribute), "TestMethodInTestFixtureWithoutExplicitTestFixtureAttribute")]
        [TestCase(typeof(TestFixtureWithTestCase), "SomeTestCase")]
        [TestCase(typeof(TestFixtureWithNamedTestCase), "SomeNamedTestCase")]
        public void Discover_Finds_Regular_Test_Fixture_And_Method(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.NUnit);
        }

        [TestCase(typeof(TestFixtureWithNestedTestClasses.SubTestClass), "RegularSubTestClassMethod")]
        [TestCase(typeof(TestFixtureWithNestedTestClasses.SubTestClass.Sub2NdTestClass), "RegularSub2NdTestClassMethod")]
        public void Discover_Finds_Sub_Test_Fixtures_And_Methods(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            AssertDiscoveredMethod(testFixtureInAssemblyToDiscoverTestsIn, expectedNameOfFirstTestMethod, TestType.NUnit);
        }
    }
}
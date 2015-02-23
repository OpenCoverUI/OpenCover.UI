using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.NUnit;

namespace OpenCover.UI.TestDiscoverer.Tests.NUnit
{
    [TestFixture]
    public class NUnitDiscovererTests
    {
        private string TestAssemblyLocation  {get { return typeof (RegularTestFixture).Assembly.Location; }}

        [Test]
        public void Discover_Finds_Regular_Test_Fixture_And_Method()
        {
            // Arrange
            var discoverer = new Discoverer(new List<string> {TestAssemblyLocation});
            
            // Act
            var discoveredTests = discoverer.Discover();

            // Assert
            discoveredTests.Should().NotBeNullOrEmpty();
            
            var discoveredTest = discoveredTests.FirstOrDefault(x => x.Name == typeof (RegularTestFixture).Name);
            discoveredTest.Should().NotBeNull();

            var discoveredMethod = discoveredTest.TestMethods.FirstOrDefault(x => x.Name == "RegularTestMethod");
            discoveredMethod.Should().NotBeNull();
        }
    }
}


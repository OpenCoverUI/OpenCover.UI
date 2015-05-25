using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using OpenCover.UI.TestDiscoverer.TestResources.Xunit;
using Xunit;

namespace OpenCover.UI.TestDiscoverer.Tests.NUnit
{
    
    public class XunitDiscovererTests
    {
        private string TestAssemblyLocation { get { return typeof (RegularFacts).Assembly.Location; } }

        [Fact]
        public void Discover_Finds_Regular_Test_Fixture_And_Method()
        {
            // Arrange
            var discoverer = new Discoverer(new List<string> {TestAssemblyLocation});
            
            // Act
            var discoveredTests = discoverer.Discover();

            // Assert
            discoveredTests.Should().NotBeNullOrEmpty();

            var discoveredTest = discoveredTests.FirstOrDefault(x => x.Name == typeof(RegularFacts).Name);
            discoveredTest.Should().NotBeNull();

            var discoveredMethod = discoveredTest.TestMethods.FirstOrDefault(x => x.Name == "RegularTestMethod");
            discoveredMethod.Should().NotBeNull();
        }
    }
}


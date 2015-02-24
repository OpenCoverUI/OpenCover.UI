using NUnit.Framework;
using OpenCover.UI.TestDiscoverer.TestResources.MSTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace OpenCover.UI.TestDiscoverer.Tests.MSTest
{
    [TestFixture]
    public class MSTestDiscovererTests
    {
        private string TestAssemblyLocation { get { return typeof(RegularTestClass).Assembly.Location; } }

        [Test]
        public void Discover_Finds_Regular_Test_Fixture_And_Method()
        {
            // Arrange
            var discoverer = new Discoverer(new List<string> { TestAssemblyLocation });

            // Act
            var discoveredTests = discoverer.Discover();

            // Assert
            discoveredTests.Should().NotBeNullOrEmpty();

            var discoveredTest = discoveredTests.FirstOrDefault(x => x.Name == typeof(RegularTestClass).Name);
            discoveredTest.Should().NotBeNull();

            var discoveredMethod = discoveredTest.TestMethods.FirstOrDefault(x => x.Name == "RegularTestMethod");
            discoveredMethod.Should().NotBeNull();
        }
    }
}

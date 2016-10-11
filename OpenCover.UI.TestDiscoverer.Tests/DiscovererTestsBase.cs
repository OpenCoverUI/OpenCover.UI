using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace OpenCover.UI.TestDiscoverer.Tests
{
    [TestFixture]
    public abstract class DiscovererTestsBase
    {
        protected void AssertDiscoveredMethod(Type testFixtureInAssemblyToDiscoverTestsIn, string expectedNameOfFirstTestMethod)
        {
            // Arrange
            var discoverer = new Discoverer(new List<string> { testFixtureInAssemblyToDiscoverTestsIn.Assembly.Location });

            // Act
            var discoveredTests = discoverer.Discover();

            // Assert
            discoveredTests.Should().NotBeNullOrEmpty();

            var discoveredTest = discoveredTests.FirstOrDefault(x => 
                                                    x.Name == testFixtureInAssemblyToDiscoverTestsIn.Name &&
                                                    x.Namespace == testFixtureInAssemblyToDiscoverTestsIn.Namespace);
            discoveredTest.Should().NotBeNull();

            var discoveredMethod = discoveredTest.TestMethods.FirstOrDefault(x => 
                                                    x.Name == expectedNameOfFirstTestMethod);
            discoveredMethod.Should().NotBeNull();
        }
    }
}
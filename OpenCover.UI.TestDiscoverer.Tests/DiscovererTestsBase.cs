using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenCover.UI.Model.Test;

namespace OpenCover.UI.TestDiscoverer.Tests
{
    [TestFixture]
    abstract class DiscovererTestsBase
    {
        protected void AssertDiscoveredMethod(Type testFixtureInAssemblyToDiscoverTestsIn,
            string expectedNameOfFirstTestMethod, TestType frameworkType, string[] expectedTraits = null)
        {
            // Arrange
            var discoverer = new Discoverer(new List<string> { testFixtureInAssemblyToDiscoverTestsIn.Assembly.Location });

            // Act
            var discoveredTestClasses = discoverer.Discover();

            // Assert
            discoveredTestClasses.Should().NotBeNullOrEmpty();

            var discoveredTestClass = discoveredTestClasses
                .FirstOrDefault(x => x.TestType == frameworkType
                    && x.Name == testFixtureInAssemblyToDiscoverTestsIn.Name
                    && x.Namespace == testFixtureInAssemblyToDiscoverTestsIn.Namespace);
            discoveredTestClass.Should().NotBeNull();

            var discoveredMethod = discoveredTestClass.TestMethods
                .FirstOrDefault(x => x.Name == expectedNameOfFirstTestMethod);
            discoveredMethod.Should().NotBeNull();

            if (expectedTraits != null)
            {
                discoveredMethod.Traits.ShouldAllBeEquivalentTo(expectedTraits);
            }
        }

    }
}

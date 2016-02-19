using NUnit.Framework;

namespace OpenCover.UI.TestDiscoverer.TestResources.NUnit
{
    [TestFixture]
    public class TestFixtureWithNamedTestCase
    {
        [TestCase(true, TestName = "TestSomethingTrue")]
        [TestCase(true, TestName = "TestSomethingTrue2")]
        public void SomeNamedTestCase(bool input) { }

    }
}
using NUnit.Framework;

namespace OpenCover.UI.TestDiscoverer.TestResources.NUnit
{
    [TestFixture]
    public class TestFixtureWithTestCase
    {
        [TestCase(false)]
        [TestCase(false)]
        public void SomeTestCase(bool input)
        {
        }
    }
}
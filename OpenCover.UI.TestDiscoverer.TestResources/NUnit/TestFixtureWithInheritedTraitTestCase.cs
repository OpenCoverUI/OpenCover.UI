using NUnit.Framework;

namespace OpenCover.UI.TestDiscoverer.TestResources.NUnit
{
    [Category("InheritedTrait")]
    public class InheritedTraitBase
    {
    }

    [TestFixture]
    [Category("ClassTrait")]
    public class TestFixtureWithInheritedTraitTestCase : InheritedTraitBase
    {
        [Test]
        public void SomeTraitTestMethod() { }
    }
}
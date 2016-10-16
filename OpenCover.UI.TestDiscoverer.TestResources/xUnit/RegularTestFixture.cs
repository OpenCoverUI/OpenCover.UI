using Xunit;

namespace OpenCover.UI.TestDiscoverer.TestResources.Xunit
{    
	public class RegularFacts
	{
		[Fact]
		public void RegularTestMethod()
		{
		}

        public class SubTestClass
        {
            [Fact]
            public void RegularSubTestClassMethod()
            {
            }

            public class Sub2NdTestClass
            {
                [Fact]
                public void RegularSub2NdTestClassMethod()
                {
                }
            }


        }

    }
}

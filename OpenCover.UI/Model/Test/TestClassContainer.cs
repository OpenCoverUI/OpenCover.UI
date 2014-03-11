using ICSharpCode.TreeView;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.Model.Test
{
	internal class TestClassContainer : SharpTreeNode
	{
		internal IEnumerable<TestClass> Classes
		{
			get;
			private set;
		}

		internal TestClassContainer(IEnumerable<TestClass> classes)
		{
			Classes = classes;
			LazyLoading = true;
		}

		protected override void LoadChildren()
		{
			if (Classes != null)
			{
				Children.AddRange(Classes); 
			}

			// Children.AddRange(Classes.SelectMany(c => c.TestMethods).GroupBy(tm => tm.Trait).Select(tr => new { Trait = tr.Key, Methods = tr}));
		}
	}
}

using ICSharpCode.TreeView;
using System.Collections.Generic;

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
			Children.AddRange(Classes);
		}
	}
}

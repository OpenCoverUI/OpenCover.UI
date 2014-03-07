using ICSharpCode.TreeView;
using System.Collections.Generic;

namespace OpenCover.UI.Model.Test
{
	internal class TestClassContainer : SharpTreeNode
	{
		internal List<TestClass> Classes
		{
			get;
			private set;
		}

		internal TestClassContainer(List<TestClass> classes)
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

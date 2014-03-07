using ICSharpCode.TreeView;

namespace OpenCover.UI.Model.Test
{
	internal partial class TestMethod : SharpTreeNode
	{
		public override object Text
		{
			get
			{
				return Name;
			}
		}
	}
}

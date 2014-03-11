using ICSharpCode.TreeView;

namespace OpenCover.UI.Model.Test
{
	internal partial class TestMethod : SharpTreeNode
	{
		/// <summary>
		/// Gets or sets the class that this method belongs to.
		/// </summary>
		/// <value>
		/// The class.
		/// </value>
		public TestClass Class { get; set; }

		/// <summary>
		/// Gets or sets the test execution status.
		/// </summary>
		/// <value>
		/// The execution status.
		/// </value>
		public TestExecutionStatus ExecutionStatus { get; set; }

		public override object Text
		{
			get
			{
				return Name;
			}
		}
	}
}

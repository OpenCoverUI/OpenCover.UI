using ICSharpCode.TreeView;
using System.Windows.Controls;

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
		public TestMethodWrapper Class { get; set; }

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

		/// <summary>
		/// Gets the name of the fully qualified of .
		/// </summary>
		/// <value>
		/// The name of the fully qualified.
		/// </value>
		public string FullyQualifiedName
		{
			get
			{
				return string.Format("{0}.{1}.{2}", Class.Namespace, Class.Name, Name);
			}
		}
	}
}

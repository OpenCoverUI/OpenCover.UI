//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using OpenCover.UI.Helpers;
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
		public TestClass Class { get; set; }

		/// <summary>
		/// Gets or sets the test execution status.
		/// </summary>
		/// <value>
		/// The execution status.
		/// </value>
		public TestExecutionStatus ExecutionStatus { get; set; }

		/// <summary>
		/// Gets the icon.
		/// </summary>
		public override object Icon
		{
			get
			{
				return IDEHelper.GetImageURL(GetIcon());
			}
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		public override object Text
		{
			get
			{
				return Name;
			}
		}

		/// <summary>
		/// Gets the name of the fully qualified name.
		/// </summary>
		public string FullyQualifiedName
		{
			get
			{
				return string.Format("{0}.{1}.{2}", Class.Namespace, Class.Name, Name);
			}
		}

		/// <summary>
		/// Clones the test method
		/// </summary>
		public TestMethod Clone()
		{
			var method = new TestMethod()
			{
				Name = this.Name,
				Class = this.Class,
				Traits = this.Traits,
				ExecutionStatus = this.ExecutionStatus
			};

			return method;
		}

		private string GetIcon()
		{
			string icon = "Resources/{0}";

			switch (ExecutionStatus)
			{
				case TestExecutionStatus.NotRun:
					return string.Format(icon, "NotRun.png");
				case TestExecutionStatus.Successful:
					return string.Format(icon, "Successful.png");
				case TestExecutionStatus.Error:
					return string.Format(icon, "Failed.png");
				case TestExecutionStatus.Inconclusive:
					return string.Format(icon, "Inconclusive.png");
				default:
					return string.Format(icon, "NotRun.png");
			}
		}
	}
}

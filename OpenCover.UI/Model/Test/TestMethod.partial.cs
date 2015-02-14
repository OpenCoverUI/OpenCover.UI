//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using OpenCover.UI.Helpers;
using System.Windows.Controls;

namespace OpenCover.UI.Model.Test
{
	internal partial class TestMethod : SharpTreeNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethod"/> class.
		/// </summary>
		public TestMethod()
		{
			ExecutionResult = new TestResult(null, TestExecutionStatus.NotRun, null, null, null, null);
		}

		/// <summary>
		/// Gets or sets the class that this testResult belongs to.
		/// </summary>
		/// <value>
		/// The class.
		/// </value>
		public TestClass Class { get; set; }

		/// <summary>
		/// Gets or sets the execution result.
		/// </summary>
		public TestResult ExecutionResult { get; set; }

		/// <summary>
		/// Gets the icon.
		/// </summary>
		public override object Icon
		{
			get
			{
				return IDEHelper.GetImageURL(IDEHelper.GetIcon(ExecutionResult.Status));
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
		/// Clones the test testResult
		/// </summary>
		public TestMethod Clone()
		{
			var method = new TestMethod()
			{
				Name = this.Name,
				Class = this.Class,
				Traits = this.Traits,
				ExecutionResult = this.ExecutionResult != null ? this.ExecutionResult.Clone() : null
			};

			return method;
		}
	}
}

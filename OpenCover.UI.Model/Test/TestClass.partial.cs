using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model.Test
{
	public partial class TestClass : SharpTreeNode
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		public override object Text
		{
			get
			{
				int lastIndex = FullName.LastIndexOf(".");
				string name = lastIndex > 0 ? FullName.Substring(lastIndex + 1) : FullName;

				return String.Format("{0} ({1})", name, TestMethods.Count);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestClass"/> class.
		/// </summary>
		public TestClass()
		{
			LazyLoading = true;
		}

		/// <summary>
		/// Loads the children.
		/// </summary>
		protected override void LoadChildren()
		{
			Children.AddRange(TestMethods);
		}
	}
}

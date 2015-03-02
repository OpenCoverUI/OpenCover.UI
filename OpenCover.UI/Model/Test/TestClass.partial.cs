//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model.Test
{
	internal partial class TestClass : SharpTreeNode
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
				return String.Format("{0} ({1})", Name, TestMethods.Length);
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
		/// Updates the TestMethods by setting their Class property to itself.
		/// </summary>
		internal void UpdateChildren()
		{
			if (TestMethods != null)
			{
				foreach (var method in TestMethods)
				{
					method.Class = this;
				}
			}
		}
	}
}

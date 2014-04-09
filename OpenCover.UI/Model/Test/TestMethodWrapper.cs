//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenCover.UI.Model.Test
{
	/// <summary>
	/// Test Method wrapper class
	/// </summary>
	internal class TestMethodWrapper : SharpTreeNode
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodWrapper"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="testMethods">The test methods.</param>
		public TestMethodWrapper(string name, IEnumerable<TestMethod> testMethods, string fullyQualifiedName)
		{
			this.Name = name;
			this.FullyQualifiedName = fullyQualifiedName;
			this.TestMethods = testMethods;

			this.LazyLoading = true;
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		internal string Name { get; private set; }

		/// <summary>
		/// Gets the fully qualified name of the wrapper.
		/// </summary>
		/// <value>
		/// The fully qualified name.
		/// </value>
		internal string FullyQualifiedName { get; private set; }

		/// <summary>
		/// Gets the test methods.
		/// </summary>
		/// <value>
		/// The test methods.
		/// </value>
		internal IEnumerable<TestMethod> TestMethods { get; private set; }

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
				var treeHeader = new TreeHeader();
				treeHeader.MainText.Text = Name;
				treeHeader.SubText.Text = TestMethods.Count().ToString();

				return treeHeader;
			}
		}

		/// <summary>
		/// Loads the children.
		/// </summary>
		protected override void LoadChildren()
		{
			if (TestMethods != null && TestMethods.Any())
			{
				foreach (var method in TestMethods)
				{
					TestMethod methodToAdd = method;

					if (method.Parent != null)
					{
						methodToAdd = method.Clone();
					}

					Children.Add(methodToAdd);
				}
			}
		}

	}
}

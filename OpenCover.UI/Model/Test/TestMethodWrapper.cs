using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		public TestMethodWrapper(string name, IEnumerable<TestMethod> testMethods)
		{
			this.Name = name;
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
				return String.Format("{0} ({1})", Name, TestMethods.Count());
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
					var methodToAdd = method;

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

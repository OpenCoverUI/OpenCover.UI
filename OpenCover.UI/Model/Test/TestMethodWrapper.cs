using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model.Test
{
	internal class TestMethodWrapper:SharpTreeNode
	{
		public TestMethodWrapper(string name, IEnumerable<TestMethod> testMethods)
		{
			this.Name = name;
			this.TestMethods = testMethods;

			this.LazyLoading = true;
		}

		internal string Name { get; private set; }

		internal IEnumerable<TestMethod> TestMethods { get; private set; }

		public override object Text { get { return Name; } }

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

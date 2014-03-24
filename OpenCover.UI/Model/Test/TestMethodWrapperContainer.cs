using ICSharpCode.TreeView;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.Model.Test
{
	internal class TestMethodWrapperContainer : SharpTreeNode
	{
		public TestMethodGroupingField groupingField { get; set; }

		internal IEnumerable<TestMethodWrapper> testMethodsWrapper
		{
			get;
			private set;
		}

		internal TestMethodWrapperContainer(IEnumerable<TestMethodWrapper> testMethodsWrapper, TestMethodGroupingField groupingField)
		{
			this.testMethodsWrapper = testMethodsWrapper;
			this.LazyLoading = true;
			this.groupingField = groupingField;
		}

		protected override void LoadChildren()
		{
			switch (groupingField)
			{
				case TestMethodGroupingField.Class:
					Children.AddRange(testMethodsWrapper.SelectMany(c => c.TestMethods).GroupBy(tm => tm.Class.Name).Select(tr => new TestMethodWrapper(tr.Key, tr)));
					break;

				case TestMethodGroupingField.Trait:

					var traits = testMethodsWrapper.SelectMany(c => c.TestMethods).SelectMany(m => m.Traits).Distinct();

					foreach (var trait in traits)
					{
						var selectedTraits = testMethodsWrapper
												.SelectMany(c => c.TestMethods)
												.Where(m => m.Traits != null && m.Traits.Contains(trait));

						Children.Add(new TestMethodWrapper(trait, selectedTraits));
					}

					//Children.AddRange(testMethodsWrapper.SelectMany(c => c.TestMethods).GroupBy(tm => tm.Trait)
					//.Select(tr => new TestMethodWrapper(tr.Key, tr)));
					break;
			}
		}
	}
}

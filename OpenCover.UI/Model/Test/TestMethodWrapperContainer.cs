using ICSharpCode.TreeView;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.Model.Test
{
	internal class TestMethodWrapperContainer : SharpTreeNode
	{
		private TestMethodGroupingField _groupingField;

		internal IEnumerable<TestMethodWrapper> TestMethodsWrapper
		{
			get;
			private set;
		}

		internal TestMethodWrapperContainer(IEnumerable<TestMethodWrapper> testMethodsWrapper, TestMethodGroupingField groupingField)
		{
			this.TestMethodsWrapper = testMethodsWrapper;
			this.LazyLoading = true;
			this._groupingField = groupingField;
		}

		protected override void LoadChildren()
		{
			switch (_groupingField)
			{
				case TestMethodGroupingField.Class:
					GroupByClass();
					break;

				case TestMethodGroupingField.Trait:
					GroupByTraits();
					break;
			}
		}

		/// <summary>
		/// Groups the tests by Class.
		/// </summary>
		private void GroupByClass()
		{
			Children.AddRange(TestMethodsWrapper
								.SelectMany(c => c.TestMethods)
								.GroupBy(tm => tm.Class.Name)
								.Select(tr => new TestMethodWrapper(tr.Key, tr.OrderBy(t => t.FullyQualifiedName)))
								.OrderBy(tmr => tmr.Name));
		}

		/// <summary>
		/// Groups the test by Traits.
		/// </summary>
		private void GroupByTraits()
		{
			var traits = TestMethodsWrapper.SelectMany(c => c.TestMethods).SelectMany(m => m.Traits).Distinct();
			var testMethodWrapper = new List<TestMethodWrapper>();

			foreach (var trait in traits)
			{
				var selectedTraits = TestMethodsWrapper
										.SelectMany(c => c.TestMethods)
										.Where(m => m.Traits != null && m.Traits.Contains(trait))
										.OrderBy(m => m.FullyQualifiedName);

				testMethodWrapper.Add(new TestMethodWrapper(trait, selectedTraits));
			}

			Children.AddRange(testMethodWrapper.OrderBy(tmw => tmw.Name));
		}
	}
}

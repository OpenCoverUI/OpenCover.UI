//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using System.Collections.Generic;
using System.IO;
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
					GroupByTrait();
					break;

				case TestMethodGroupingField.Project:
					GroupByProject();
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
		private void GroupByTrait()
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

		/// <summary>
		/// Groups the tests by project.
		/// </summary>
		private void GroupByProject()
		{
			var methodsGroupedByProject = TestMethodsWrapper
											.SelectMany(c => c.TestMethods)
											.GroupBy(tm => tm.Class.DLLPath)
											.Select(tm => new TestMethodWrapper(Path.GetFileNameWithoutExtension(tm.Key), tm));

			Children.AddRange(methodsGroupedByProject);
			
		}
	}
}

//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenCover.UI.Model.Test
{
	internal class TestMethodWrapperContainer : SharpTreeNode
	{
		private TestMethodGroupingField _groupingField;

		/// <summary>
		/// Gets the test methods wrapper.
		/// </summary>
		/// <value>
		/// The test methods wrapper.
		/// </value>
		internal IEnumerable<TestMethodWrapper> TestMethodsWrapper
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodWrapperContainer"/> class.
		/// </summary>
		/// <param name="testMethodsWrapper">The test methods wrapper.</param>
		/// <param name="groupingField">The grouping field.</param>
		internal TestMethodWrapperContainer(IEnumerable<TestMethodWrapper> testMethodsWrapper, TestMethodGroupingField groupingField)
		{
			this.TestMethodsWrapper = testMethodsWrapper;
			this.LazyLoading = true;
			this._groupingField = groupingField;
		}

		/// <summary>
		/// Gets the selected tests.
		/// </summary>
		/// <returns>Selected Tests</returns>
		internal Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> GetSelectedTestGroupsAndTests()
		{
			var builder = new StringBuilder();

			var testsItemSource = (this.Children.Cast<TestMethodWrapper>());

			var selectedTestGroups = testsItemSource
										.Where(t => t.IsSelected)
										.Select(t => t.FullyQualifiedName);

			var selectedDLLsInGroups = testsItemSource
										.Where(t => t.IsSelected)
										.SelectMany(t => t.TestMethods)
										.Select(t => t.Class.DLLPath)
										.Distinct();


			var testsInNotSelectedGroupQuery = this.Children
												.Where(tg => !tg.IsSelected)
												.SelectMany(tg => tg.Children.Where(test => test.IsSelected))
												.Cast<TestMethod>();

			var testsInNotSelectedGroup = testsInNotSelectedGroupQuery.Select(tm => tm.FullyQualifiedName);

			var dllsInSelectedTests = testsInNotSelectedGroupQuery.Select(tm => tm.Class.DLLPath).Distinct();


			return new Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>>(selectedTestGroups, testsInNotSelectedGroup, selectedDLLsInGroups.Union(dllsInSelectedTests));
		}

		/// <summary>
		/// Loads the children.
		/// </summary>
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
								.Select(tr =>
									{
										var @class = tr.ElementAt(0).Class;
										var fqn = String.Format("{0}.{1}", @class.Namespace, @class.Name);

										return new TestMethodWrapper(tr.Key, tr.OrderBy(t => t.FullyQualifiedName), fqn);
									})
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

				testMethodWrapper.Add(new TestMethodWrapper(trait, selectedTraits, trait));
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
											.Select(tm =>
											{
												var dll = Path.GetFileNameWithoutExtension(tm.Key);
												return new TestMethodWrapper(dll, tm, dll);
											});

			Children.AddRange(methodsGroupedByProject);

		}
	}
}

//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using ICSharpCode.TreeView;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OpenCover.UI.Model.Test
{
	/// <summary>
	/// Container for Test Methods Wrapper
	/// </summary>
	internal class TestMethodWrapperContainer : SharpTreeNode
	{
		private TestMethodGroupingField _groupingField;
		private string _caption;

		internal TestType TestType;

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
		internal TestMethodWrapperContainer(string caption, IEnumerable<TestMethodWrapper> testMethodsWrapper, TestMethodGroupingField groupingField, TestType testType)
		{
			_caption = caption;
			_groupingField = groupingField;

			TestType = testType;

			TestMethodsWrapper = testMethodsWrapper;
			LazyLoading = true;
		}

		/// <summary>
		/// Gets the selected tests.
		/// </summary>
		/// <returns>Selected Tests</returns>
		internal Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> GetSelectedTestGroupsAndTests()
		{
			var testsItemSource = (Children.Cast<TestMethodWrapper>());

			TestMethodWrapper noTraitsWrapper = null;

			var selectedTestGroups = testsItemSource
										.Where(t =>
										{
											if (_groupingField == TestMethodGroupingField.Trait && t.FullyQualifiedName == "No Traits")
											{
												if (t.IsSelected)
												{
													noTraitsWrapper = t;
												}

												return false;
											}

											return t.IsSelected;
										})
										.Select(t => t.FullyQualifiedName)
										.ToArray();

			var selectedDLLsInGroups = GetSelectedDLLsInGroups(testsItemSource);

			var testsInNotSelectedGroupQuery = GetTestsInNotSelectedGroupQuery();

			var testsInNotSelectedGroup = testsInNotSelectedGroupQuery.Select(tm => tm.FullyQualifiedName);

			if (noTraitsWrapper != null)
			{
				testsInNotSelectedGroup = testsInNotSelectedGroup.Union(noTraitsWrapper.TestMethods.Select(t => t.FullyQualifiedName));
			}

			var dllsInSelectedTests = testsInNotSelectedGroupQuery.Select(tm => tm.Class.DLLPath).Distinct();


			return new Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>>(selectedTestGroups, testsInNotSelectedGroup, selectedDLLsInGroups.Union(dllsInSelectedTests));
		}

		/// <summary>
		/// Gets the selected tests.
		/// </summary>
		internal Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> GetSelectedTests()
		{
			var testsItemSource = (Children.Cast<TestMethodWrapper>());

			var selectedDLLsInGroups = GetSelectedDLLsInGroups(testsItemSource);

			var testsInNotSelectedGroupQuery = GetTestsInNotSelectedGroupQuery().Select(tm => tm.FullyQualifiedName);

			var testsInSelectedGroups = testsItemSource.Where(t => t.IsSelected).SelectMany(t => t.TestMethods).Select(tm => tm.FullyQualifiedName);

			return new Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>(null, testsInNotSelectedGroupQuery.Union(testsInSelectedGroups), selectedDLLsInGroups);
		}

		/// <summary>
		/// Gets all test methods.
		/// </summary>
		internal IEnumerable<TestMethod> GetAllTestMethods()
		{
			var testsItemSource = (Children.Cast<TestMethodWrapper>());

			if (testsItemSource != null)
			{
				return testsItemSource.SelectMany(t => t.TestMethods);
			}

			return null;
		}

		/// <summary>
		/// Gets the selected dlls in groups.
		/// </summary>
		/// <param name="testsItemSource">The tests item source.</param>
		private IEnumerable<string> GetSelectedDLLsInGroups(IEnumerable<TestMethodWrapper> testsItemSource)
		{
			return testsItemSource
					.Where(t => t.IsSelected)
					.SelectMany(t => t.TestMethods)
					.Select(t => t.Class.DLLPath)
					.Distinct();
		}

		/// <summary>
		/// Gets the tests in not selected group query.
		/// </summary>
		private IEnumerable<TestMethod> GetTestsInNotSelectedGroupQuery()
		{
			return Children
						.Where(tg => !tg.IsSelected)
						.SelectMany(tg => tg.Children.Where(test => test.IsSelected))
						.Cast<TestMethod>();
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		public override object Text
		{
			get
			{
				if (_caption != null)
				{
					var treeHeader = new TreeHeader();
					treeHeader.MainText.Text = _caption;
					treeHeader.SubText.Text = TestMethodsWrapper.SelectMany(c => c.TestMethods).Count().ToString();

					return treeHeader;
				}

				return null;
			}
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

				case TestMethodGroupingField.Outcome:
					GroupByOutcome();
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

		/// <summary>
		/// Groups the tests by outcome.
		/// </summary>
		private void GroupByOutcome()
		{
			var methodsGroupedByProject = TestMethodsWrapper
										.SelectMany(c => c.TestMethods)
										.GroupBy(tm => tm.ExecutionStatus)
										.Select(tm =>
										{
											var text = GetOutcomeText(tm.Key);
											return new TestMethodWrapper(text, tm, text);
										});

			Children.AddRange(methodsGroupedByProject);
		}

		/// <summary>
		/// Converts Enum to its string equivalent.
		/// </summary>
		/// <param name="status">The status.</param>
		private string GetOutcomeText(TestExecutionStatus status)
		{
			// TODO: Find a better approach. It works for now!

			switch (status)
			{
				case TestExecutionStatus.NotRun:
					return "Not Run";
				case TestExecutionStatus.Successful:
					return "Successful";
				case TestExecutionStatus.Error:
					return "Error";
				case TestExecutionStatus.Inconclusive:
					return "Inconclusive";
				default:
					return "Not Run";
			}
		}
	}
}

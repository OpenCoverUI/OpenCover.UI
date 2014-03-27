//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Helpers;
using OpenCover.UI.Model.Test;
using OpenCover.UI.Processors;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCover.UI.Commands
{
	/// <summary>
	/// Executes the selected tests
	/// </summary>
	public class ExecuteSelectedTestsCommand : Command
	{
		private const string CODE_COVERAGE_RESULTS_WINDOW_TITLE = "Code Coverage";
		private const string CODE_COVERAGE_SELECT_TESTS_MESSAGE = "Please select a test to run";

		private OpenCoverUIPackage _package;
		private IEnumerable<TestMethod> _selectedTests;
		private TestExplorerControl _testExplorerControl;
		private CodeCoverageResultsControl _codeCoverageResultsControl;
		private bool _isRunningCodeCoverage;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExecuteSelectedTestsCommand" /> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public ExecuteSelectedTestsCommand(OpenCoverUIPackage package)
			: base(package, new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet, (int)PkgCmdIDList.CommandIDOpenCoverTestExplorerRunTestWithOpenCover))
		{
			_package = package;

			base.Enabled = false;

			_testExplorerControl = _package.ToolWindows.OfType<TestExplorerToolWindow>().First().TestExplorerControl;
			_testExplorerControl.TestDiscoveryFinished += OnTestDiscoveryFinished;

			_codeCoverageResultsControl = _package.ToolWindows.OfType<CodeCoverageResultsToolWindow>().First().CodeCoverageResultsControl;
		}

		/// <summary>
		/// Event handler for TestExplorerControl.TestDiscoveryFinished.
		/// </summary>
		void OnTestDiscoveryFinished()
		{
			var hasTests = _testExplorerControl.TestsTreeView.Root != null && _testExplorerControl.TestsTreeView.Root.Children.Any();
			if (hasTests & !_isRunningCodeCoverage)
			{
				Enabled = true;
			}
			else
			{
				Enabled = false;
			}
		}

		/// <summary>
		/// Called when the command is executed.
		/// </summary>
		protected override void OnExecute()
		 {
			 _codeCoverageResultsControl.ClearTreeView();

			var testGroupCollection = _testExplorerControl.TestsTreeView.Root;
			var testsItemSource = (testGroupCollection.Children.Cast<TestMethodWrapper>());

			// Need to select all tests which are under the selected group.
			var testsInSelectedGroup = testGroupCollection.Children
											.Where(tg => tg.IsSelected)
											.SelectMany(tg =>
											{
												var testClass = tg as TestMethodWrapper;
												return testsItemSource
													.Where(tc => tc == testClass)
													.SelectMany(tc => tc.TestMethods);
											});

			// Need to select only those tests which are selected under not selected groups.
			var testsInNotSelectedGroup = testGroupCollection.Children
															.Where(tg => !tg.IsSelected)
															.SelectMany(tg => tg.Children.Where(test => test.IsSelected))
															.Cast<TestMethod>();

			// Union of both tests is our selected tests
			_selectedTests = testsInNotSelectedGroup.Union(testsInSelectedGroup, new SelectedTestsComparer());

			if (_selectedTests.Any())
			{
				// show tool window which shows the progress.
				ShowCodeCoverageResultsToolWindow();

				Enabled = false;
				_isRunningCodeCoverage = true;
				_package.VSEventsHandler.BuildDone += RunOpenCover;
				_package.VSEventsHandler.BuildSolution();
			}
			else
			{
				MessageBox.Show(ExecuteSelectedTestsCommand.CODE_COVERAGE_SELECT_TESTS_MESSAGE, 
								ExecuteSelectedTestsCommand.CODE_COVERAGE_RESULTS_WINDOW_TITLE, 
								MessageBoxButton.OK, 
								MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Runs OpenCover for gathering code coverage details. This method gets called after the build is completed
		/// </summary>
		private void RunOpenCover()
		{
			// TODO: Check validity of tests
			Task.Factory.StartNew(
				() =>
				{
					var control = _package.ToolWindows.OfType<CodeCoverageResultsToolWindow>().First().CodeCoverageResultsControl;

					control.IsLoading = true;
					var testExecutor = new TestExecutor(_package, _selectedTests);
					Tuple<string, string> files = testExecutor.Execute();
					var finalResults = testExecutor.GetExecutionResults();

					control.UpdateCoverageResults(finalResults);

					// if the tool window is hidden, show it again.
					ShowCodeCoverageResultsToolWindow();

					Enabled = true;
					_isRunningCodeCoverage = false;
				});

			_package.VSEventsHandler.BuildDone -= RunOpenCover;
		}

		private void ShowCodeCoverageResultsToolWindow()
		{
			_package.Commands.OfType<CodeCoverageToolWindowCommand>().First().Invoke();
		}
	}

	class SelectedTestsComparer : IEqualityComparer<TestMethod>
	{
		public bool Equals(TestMethod x, TestMethod y)
		{
			return x.FullyQualifiedName == y.FullyQualifiedName;
		}

		public int GetHashCode(TestMethod obj)
		{
			return 1;
		}
	}
}


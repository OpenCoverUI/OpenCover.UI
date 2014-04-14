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
		private const string CODE_COVERAGE_SELECT_TESTS_MESSAGE = "Please select a test to run";
		private const string CODE_COVERAGE_SELECT_LESS_TESTS_MESSAGE = "We could not run all the tests that you selected. Please see output window for more details.";

		private OpenCoverUIPackage _package;
		private TestExplorerControl _testExplorerControl;
		private CodeCoverageResultsControl _codeCoverageResultsControl;
		private bool _isRunningCodeCoverage;
		private TestExecutor _testExecutor;

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
			TestMethodWrapperContainer msTests = null, nUnitTests = null;
			TestMethodWrapperContainer container = _testExplorerControl.TestsTreeView.Root as TestMethodWrapperContainer;

			if (container != null)
			{
				if (container.TestType == TestType.MSTest)
				{
					msTests = container;
				}
				else
				{
					nUnitTests = container;
				}
			}
			else
			{
				msTests = _testExplorerControl.TestsTreeView.Root.Children[0] as TestMethodWrapperContainer;
				nUnitTests = _testExplorerControl.TestsTreeView.Root.Children[1] as TestMethodWrapperContainer;
			}

			var selectedMSTests = msTests != null ? msTests.GetSelectedTestGroupsAndTests() : null;
			var selectedNUnitTests = nUnitTests != null ? nUnitTests.GetSelectedTests() : null;

			_testExecutor = null;

			SetTestExecutor(selectedMSTests, selectedNUnitTests);

			if (_testExecutor == null)
			{
				MessageBox.Show(ExecuteSelectedTestsCommand.CODE_COVERAGE_SELECT_TESTS_MESSAGE,
								Resources.MessageBoxTitle,
								MessageBoxButton.OK,
								MessageBoxImage.Error);

				return;
			}

			if (!_testExecutor.ValidateCommandLineArgumentsLength())
			{
				MessageBox.Show(ExecuteSelectedTestsCommand.CODE_COVERAGE_SELECT_LESS_TESTS_MESSAGE,
								Resources.MessageBoxTitle,
								MessageBoxButton.OK,
								MessageBoxImage.Error);

				return;
			}
			
			// show tool window which shows the progress.
			ShowCodeCoverageResultsToolWindow();

			Enabled = false;
			_isRunningCodeCoverage = true;
			_package.VSEventsHandler.BuildDone += RunOpenCover;
			_package.VSEventsHandler.BuildSolution();
			
		}

		private void SetTestExecutor(Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>> selectedMSTests, Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>> selectedNUnitTests)
		{
			if (selectedMSTests != null && (selectedMSTests.Item1.Any() || selectedMSTests.Item2.Any() || selectedMSTests.Item3.Any()))
			{
				_testExecutor = new MSTestExecutor(_package, selectedMSTests);
			}
			else if (selectedNUnitTests != null && (selectedNUnitTests.Item2.Any() || selectedNUnitTests.Item3.Any()))
			{
				_testExecutor = new NUnitTestExecutor(_package, selectedNUnitTests);
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
					var testsExplorer = _package.ToolWindows.OfType<TestExplorerToolWindow>().First().TestExplorerControl;
					
					try
					{

						control.IsLoading = true;
						Tuple<string, string> files = _testExecutor.Execute();
						var finalResults = _testExecutor.GetExecutionResults();
						_testExecutor.UpdateTestMethodsExecution(_testExplorerControl.Tests);

						testsExplorer.Update();

						if (finalResults != null)
						{
							control.UpdateCoverageResults(finalResults);

							// if the tool window is hidden, show it again.
							ShowCodeCoverageResultsToolWindow();
						}
						else
						{
							control.IsLoading = false;
						}
					}
					catch (Exception ex)
					{
						IDEHelper.WriteToOutputWindow(ex.Message);
						IDEHelper.WriteToOutputWindow(ex.StackTrace);

						MessageBox.Show(String.Format("An exception occured: {0}\nPlease refer to output window for more details", ex.Message), Resources.MessageBoxTitle, MessageBoxButton.OK);
						
						control.IsLoading = false;
					}
					finally
					{
						Enabled = true;
						_isRunningCodeCoverage = false;
					}
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


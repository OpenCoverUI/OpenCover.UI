//
// This source code is released under the MIT License; Please read license.md file for more details.
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
		private TestExecutor _testExecutor;

		private CodeCoverageResultsControl CodeCoverageResults { get { return _package.GetToolWindow<CodeCoverageResultsToolWindow>().CodeCoverageResultsControl; } }
		private TestExplorerControl TestExplorer { get { return _package.GetToolWindow<TestExplorerToolWindow>().TestExplorerControl; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExecuteSelectedTestsCommand" /> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public ExecuteSelectedTestsCommand(OpenCoverUIPackage package)
			: base(package, new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet, (int)PkgCmdIDList.CommandIDOpenCoverTestExplorerRunTestWithOpenCover))
		{
			_package = package;

            DisableControl();

			TestExplorerControl.TestDiscoveryFinished += OnTestDiscoveryFinished;
		}

		/// <summary>
		/// Event handler for TestExplorerControl.TestDiscoveryFinished.
		/// </summary>
		void OnTestDiscoveryFinished()
		{
            var hasTests = TestExplorer.TestsTreeView.Root != null && TestExplorer.TestsTreeView.Root.Children.Any();

            if (hasTests && (_testExecutor == null || !_testExecutor.IsExecuting))
            {
                EnableControl();
            }
            else 
            {
                DisableControl();
            }
		}

		/// <summary>
		/// Called when the command is executed.
		/// </summary>
		protected override void OnExecute()
		{
			CodeCoverageResults.ClearTreeView();
			TestMethodWrapperContainer msTests = null, nUnitTests = null, xUnitTests = null;
			TestMethodWrapperContainer container = TestExplorer.TestsTreeView.Root as TestMethodWrapperContainer;

			if (container != null)
			{
				if (container.TestType == TestType.MSTest)
				{
					msTests = container;
				}                
				else if (container.TestType == TestType.XUnit)
				{
                    xUnitTests = container;
                }
                else
                {
					nUnitTests = container;
				}
			}
			else
			{
                
				msTests = TestExplorer.TestsTreeView.Root.Children.GetContainer(TestType.MSTest);
                nUnitTests = TestExplorer.TestsTreeView.Root.Children.GetContainer(TestType.NUnit);
                xUnitTests = TestExplorer.TestsTreeView.Root.Children.GetContainer(TestType.XUnit);
			}

			var selectedMSTests = msTests != null ? msTests.GetSelectedTestGroupsAndTests() : null;
			var selectedNUnitTests = nUnitTests != null ? nUnitTests.GetSelectedTests() : null;
            var selectedXUnitTests = xUnitTests != null ? xUnitTests.GetSelectedTests() : null;

			_testExecutor = null;

			SetTestExecutor(selectedMSTests, selectedNUnitTests, selectedXUnitTests);

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

            DisableControl();

			_package.VSEventsHandler.BuildSucceeded += RunOpenCover;
            _package.VSEventsHandler.BuildFailed += () => DisableControl();
			_package.VSEventsHandler.BuildSolution();
		}

		/// <summary>
		/// Sets the command availability status to true.
		/// </summary>		
		private void EnableControl()
		{
			Enabled = true;
		}

        /// <summary>
        /// Sets the command availability status to false.
        /// </summary>
        private void DisableControl()
        {
            Enabled = false;
        }

		/// <summary>
		/// Sets the test executor based on the type of Unit Tests selected.
		/// </summary>
		/// <param name="selectedMSTests">The selected ms tests.</param>
		/// <param name="selectedNUnitTests">The selected n unit tests.</param>
		private void SetTestExecutor(Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>> selectedMSTests
            , Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>> selectedNUnitTests
            , Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>> selectedXUnitTests)
		{
			if (selectedMSTests != null && (selectedMSTests.Item1.Any() || selectedMSTests.Item2.Any() || selectedMSTests.Item3.Any()))
			{
				_testExecutor = new MSTestExecutor(_package, selectedMSTests);
			}
			else if (selectedNUnitTests != null && (selectedNUnitTests.Item2.Any() || selectedNUnitTests.Item3.Any()))
			{
				_testExecutor = new NUnitTestExecutor(_package, selectedNUnitTests);
			}
            else if (selectedXUnitTests != null && (selectedXUnitTests.Item2.Any() || selectedXUnitTests.Item3.Any()))
            {
                _testExecutor = new XUnitTestExecutor(_package, selectedXUnitTests);
            }
		}

		/// <summary>
		/// Runs OpenCover for gathering code coverage details. This testResult gets called after the build is completed
		/// </summary>
		private void RunOpenCover()
		{
			// TODO: Check validity of tests
			Task.Factory.StartNew(
				() =>
				{
					var control = _package.GetToolWindow<CodeCoverageResultsToolWindow>().CodeCoverageResultsControl;
					var testsExplorer = _package.GetToolWindow<TestExplorerToolWindow>().TestExplorerControl;
					
					try
					{
						control.IsLoading = true;
						Tuple<string, string> files = _testExecutor.Execute();
						var finalResults = _testExecutor.GetExecutionResults();
						_testExecutor.UpdateTestMethodsExecution(TestExplorer.Tests);

						testsExplorer.ChangeGroupBy(TestMethodGroupingField.Outcome);

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
                        EnableControl();
					}
				});

			_package.VSEventsHandler.BuildSucceeded -= RunOpenCover;
		}

		/// <summary>
		/// Shows the code coverage results tool window.
		/// </summary>
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


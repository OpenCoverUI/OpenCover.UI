//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Model;
using Microsoft.VisualStudio.TestWindow.UI;
using OpenCover.UI.Helpers;
using OpenCover.UI.Processors;
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
		private OpenCoverUIPackage _package;
		private IVsUIShell _uiShell;
		private TestTreeControl _testTreeControl;
		private IEnumerable<Test> _selectedTests;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExecuteSelectedTestsCommand"/> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public ExecuteSelectedTestsCommand(OpenCoverUIPackage package, IVsUIShell uiShell)
			: base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.CmdidCoverWithOpenCover))
		{
			_package = package;
			_uiShell = uiShell;

			FetchTestsTreeView();

			base.Enabled = false;
		}

		/// <summary>
		/// Fetches the TestsTreeView control.
		/// </summary>
		public void FetchTestsTreeView()
		{
			if (_testTreeControl == null)
			{
				_testTreeControl = IDEHelper.GetTestTreeControl(_uiShell);
				if (_testTreeControl != null)
				{
					_testTreeControl.LayoutUpdated += EnableDisableCommand;
				} 
			}
		}

		/// <summary>
		/// Enables the disable command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void EnableDisableCommand(object sender, EventArgs e)
		{
			// TODO: This event handler gets called multiple times. Need to find a better way to enable/disable the command
			var items = _testTreeControl.ItemsSource as TestGroupCollection;

			if (items.Count > 0)
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
			if (_testTreeControl == null)
			{
				_testTreeControl = IDEHelper.GetTestTreeControl(_uiShell);
				if (_testTreeControl != null)
				{
					_testTreeControl.LayoutUpdated += EnableDisableCommand;
				}
			}

			var testGroupCollection = _testTreeControl.ItemsSource as TestGroupCollection;

			// Need to select all tests which are under the selected group.
			var testsInSelectedGroup = testGroupCollection.Where(tg => tg.IsSelected).SelectMany(tg => tg.Tests);

			// Need to select only those tests which are selected under not selected groups.
			var testsInNotSelectedGroup = testGroupCollection.Where(tg => !tg.IsSelected).SelectMany(tg => tg.Tests.Where(test => test.IsSelected));

			// Union of both tests is our selected tests
			_selectedTests = testsInNotSelectedGroup.Union(testsInSelectedGroup);

			if (_selectedTests.Any())
			{
				// show tool window which shows the progress.
				_package.ShowResultsCodeCoverageResultsToolWindow();

				Enabled = false;

				MessageBox.Show("Please wait while we collect code coverage results. The results will be shown in 'Code Coverage Results' window!", "Code Coverage", MessageBoxButton.OK, MessageBoxImage.Information);

				_package.VSEventsHandler.BuildSolution(RunOpenCover);
			}
			else
			{
				MessageBox.Show("Please select a test to run", "Code Coverage", MessageBoxButton.OK, MessageBoxImage.Error);
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
					var testExecutor = new TestExecutor(_package, _selectedTests);
					Tuple<string, string> files = testExecutor.Execute();
					var finalResults = testExecutor.GetExecutionResults();

					IDEHelper.WriteToOutputWindow(String.Format("Updating Code Coverage Results window. Data = {0}", finalResults.CoveredModules.Count()));

					_package.CodeCoverageResultsControl.UpdateCoverageResults(finalResults);

					// if the tool window is hidden, show it again.
					_package.ShowResultsCodeCoverageResultsToolWindow();

					Enabled = true; 
				});
		}
	}
}

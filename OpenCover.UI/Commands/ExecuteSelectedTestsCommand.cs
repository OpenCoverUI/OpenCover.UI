//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Model;
using Microsoft.VisualStudio.TestWindow.UI;
using OpenCover.UI.Helpers;
using OpenCover.UI.Processors;
using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
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

		/// <summary>
		/// Initializes a new instance of the <see cref="ExecuteSelectedTestsCommand"/> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public ExecuteSelectedTestsCommand(OpenCoverUIPackage package, IVsUIShell uiShell)
			: base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.CmdidCoverWithOpenCover))
		{
			this._package = package;
			this._uiShell = uiShell;
			this._testTreeControl = IDEHelper.GetTestTreeControl(_uiShell);
			this._testTreeControl.LayoutUpdated += EnableDisableCommand;
			base.Enabled = false;
		}

		/// <summary>
		/// Enables the disable command.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void EnableDisableCommand(object sender, EventArgs e)
		{
			// TODO: This event handler gets called multiple times. Need to find a better way to enable/disable the command
			var items = this._testTreeControl.ItemsSource as TestGroupCollection;
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
			var testGroupCollection = _testTreeControl.ItemsSource as TestGroupCollection;
			var selectedTests = testGroupCollection.SelectMany(testGroup => testGroup.Tests.Where(test => test.IsSelected));

			if (selectedTests.Any())
			{
				// show tool window which shows the progress.
				_package.ShowResultsCodeCoverageResultsToolWindow();

				this.Enabled = false;

				MessageBox.Show("Please wait while we collect code coverage results. The results will be shown in 'Code Coverage Results' window!", "Code Coverage Results", MessageBoxButton.OK, MessageBoxImage.Information);

				var executorThread = new Thread(
					() =>
					{
						var testExecutor = new TestExecutor(_package, selectedTests);
						Tuple<string, string> files = testExecutor.Execute();
						var finalResults = testExecutor.GetExecutionResults();

						_package.CodeCoverageResultsControl.UpdateCoverageResults(finalResults);

						// if the tool window is hidden, show it again.
						_package.ShowResultsCodeCoverageResultsToolWindow();

						this.Enabled = true;
					});
				
				executorThread.Start();
			}
		}
	}
}

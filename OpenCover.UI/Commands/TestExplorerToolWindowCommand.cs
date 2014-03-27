//
// This source code is released under the GPL License; Please read license.md file for more details
//
using Microsoft.VisualStudio.Shell.Interop;
using OpenCover.UI.Views;
using System.ComponentModel.Design;
using System.Linq;

namespace OpenCover.UI.Commands
{
	/// <summary>
	/// Opens the OpenCover UI Test Explorer Tool Window
	/// </summary>
	public class TestExplorerToolWindowCommand : Command
	{
		private OpenCoverUIPackage _package;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestExplorerToolWindowCommand"/> class.
		/// </summary>
		/// <param name="package">The package.</param>
		public TestExplorerToolWindowCommand(OpenCoverUIPackage package)
			: base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.CmdidCodeCoverageTestWindow))
		{
			this._package = package;
		}

		/// <summary>
		/// Called when the command is executed.
		/// </summary>
		protected override void OnExecute()
		{
			var frame = _package.ToolWindows.OfType<TestExplorerToolWindow>().First().Frame as IVsWindowFrame;
			if (frame != null)
			{
				frame.Show();
			}
		}
	}
}

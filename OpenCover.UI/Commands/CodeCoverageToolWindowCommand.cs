//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.Shell.Interop;
using OpenCover.UI.Views;
using System.ComponentModel.Design;
using System.Linq;

namespace OpenCover.UI.Commands
{
	/// <summary>
	/// Shows the Code Coverage Tool Window
	/// </summary>
	public class CodeCoverageToolWindowCommand : Command
	{
		private OpenCoverUIPackage _package;

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeCoverageToolWindowCommand"/> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public CodeCoverageToolWindowCommand(OpenCoverUIPackage package)
			: base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.CmdidCodeCoverageResults))
		{
			this._package = package;
		}

		/// <summary>
		/// Opens the Code Coverage Results ToolWindow
		/// </summary>
		protected override void OnExecute()
		{
			var frame = _package.ToolWindows.OfType<CodeCoverageResultsToolWindow>().First().Frame as IVsWindowFrame;
			if (frame != null)
			{
				frame.Show();
			}
		}
	}
}

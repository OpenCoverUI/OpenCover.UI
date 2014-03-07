using Microsoft.VisualStudio.Shell.Interop;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCover.UI.Commands
{
	public class TestExplorerToolWindowCommand : Command
	{
		private OpenCoverUIPackage _package;

		public TestExplorerToolWindowCommand(OpenCoverUIPackage package)
			: base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.CmdidCodeCoverageTestWindow))
		{
			this._package = package;
		}

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

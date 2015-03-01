using Microsoft.Win32;
//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Commands
{
	public class MSTestSettingsFileSelectorCommand : Command
	{
		private OpenCoverUIPackage _package;

		internal string SelectedFile {get; private set;}

		public MSTestSettingsFileSelectorCommand(OpenCoverUIPackage package)
			: base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.OpenCoverSelectMSTestSettings))
		{
			_package = package;
			BeforeQueryStatus += OnBeforeQueryStatus;
		}

		private void OnBeforeQueryStatus(object sender, EventArgs e)
		{
			if (SelectedFile != null && File.Exists(SelectedFile))
			{
				Text = String.Format("MSTest Settings: {0}", Path.GetFileName(SelectedFile));
			}
		}

		protected override void OnExecute()
		{
			var dlg = new OpenFileDialog();
			dlg.Filter = "TestSettings (*.testsettings,*.runsettings,*.vsmdi,*.testrunconfig)|*.testsettings;*.runsettings;*.vsmdi;*.testrunconfig";

			if (dlg.ShowDialog() == true)
			{
				SelectedFile = dlg.FileName;
			}
		}
	}
}

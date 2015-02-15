using System.ComponentModel.Design;
using OpenCover.UI.Views;

namespace OpenCover.UI.Commands
{
    public class OpenSettingsWindowCommand : Command
    {
        public OpenSettingsWindowCommand(OpenCoverUIPackage package)
            : base(package, new CommandID(GuidList.GuidOpenCoverUICmdSet, (int)PkgCmdIDList.OpenCoverOpenSettings))
        {  
        }

        protected override void OnExecute()
        {
            var settings = new SettingsDialog();
            settings.ShowDialog();
        }
    }
}
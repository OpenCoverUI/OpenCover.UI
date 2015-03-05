using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;

namespace OpenCover.UI.Views.OptionPages
{
    [Guid(GuidList.GuidOpenCoverUIOptionPage)]
    public class OpenCoverUIOptionPage : UIElementDialogPage
    {
        protected override System.Windows.UIElement Child
        {
            get { return new SettingsControl(); }
        }

        protected override void OnClosed(EventArgs e)
        {
            OpenCoverUISettings.Default.Save();
            base.OnClosed(e);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            OpenCoverUISettings.Default.Save();
            base.OnApply(e);
        }
    }
}

using GalaSoft.MvvmLight.CommandWpf;

namespace OpenCover.UI.Framework.ViewModel
{
    public interface ISettingsViewModel
    {
        string NUnitExePath { get; set; }

        string OpenCoverExePath { get; set; }

        string XUnitExePath { get; set; }
    }
}
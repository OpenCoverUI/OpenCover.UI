using GalaSoft.MvvmLight.CommandWpf;

namespace OpenCover.UI.Framework.ViewModel
{
    public interface ISettingsDialogViewModel
    {
        string NUnitExePath { get; set; }

        string OpenCoverExePath { get; set; }

        RelayCommand ProcessSelectNUnitExe { get; set; }

        RelayCommand ProcessSelectOpenCoverExe { get; set; }
    }
}
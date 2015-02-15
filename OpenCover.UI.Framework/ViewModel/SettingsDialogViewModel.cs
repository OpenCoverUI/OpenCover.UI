using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace OpenCover.UI.Framework.ViewModel
{
    public class SettingsDialogViewModel : ViewModelBase, ISettingsDialogViewModel
    {
        public SettingsDialogViewModel()
        {
            ProcessSelectOpenCoverExe = new RelayCommand(SelectOpenCoverExe);
            ProcessSelectNUnitExe = new RelayCommand(SelectSelectNUnitExe);
            if (IsInDesignMode)
            {
                NUnitExePath = @"C:\nunit\nunit-console.exe";
                OpenCoverExePath = @"C:\opencover\opencover.console.exe";
            }
        }

        private string _nUnitExePath;
        private string _openCoverExePath;

        public string NUnitExePath
        {
            get { return _nUnitExePath; }
            set
            {
                Set(ref _nUnitExePath, value);
            }
        }

        public string OpenCoverExePath
        {
            get { return _openCoverExePath; }
            set
            {
                Set(ref _openCoverExePath, value);
            }
        }


        public void SelectSelectNUnitExe()
        {

        }

        public RelayCommand ProcessSelectNUnitExe { get; set; }

        public void SelectOpenCoverExe()
        {
            
        }

        public RelayCommand ProcessSelectOpenCoverExe { get; set; }
    }
}

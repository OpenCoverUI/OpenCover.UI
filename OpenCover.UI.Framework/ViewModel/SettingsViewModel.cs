using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace OpenCover.UI.Framework.ViewModel
{
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly IOpenCoverUiSettings _settings;

        public SettingsViewModel() : this(null)
        {
        }

        public SettingsViewModel(IOpenCoverUiSettings settings)
        {
            _settings = settings;
            ProcessSelectOpenCoverExe = new RelayCommand(FireSelectOpenCoverExeEvent);
            ProcessSelectNUnitExe = new RelayCommand(FireSelectNUnitExeEvent);
            if (IsInDesignMode)
            {
                NUnitExePath = @"C:\nunit\nunit-console.exe";
                OpenCoverExePath = @"C:\opencover\opencover.console.exe";
            }
            else
            {
                _nUnitExePath = _settings.NUnitPath;
                _openCoverExePath = _settings.OpenCoverPath;
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
                if (_settings != null) _settings.NUnitPath = value;
            }
        }

        public string OpenCoverExePath
        {
            get { return _openCoverExePath; }
            set
            {
                Set(ref _openCoverExePath, value);
                if (_settings != null) _settings.OpenCoverPath = value;
            }
        }

        public event EventHandler<EventArgs> SelectNunitExeEvent;
  
        private void FireSelectNUnitExeEvent()
        {
            if (SelectNunitExeEvent!=null) 
                SelectNunitExeEvent(this, new EventArgs());
        }

        public RelayCommand ProcessSelectNUnitExe { get; private set; }

        public event EventHandler<EventArgs> SelectOpenCoverExeEvent;

        private void FireSelectOpenCoverExeEvent()
        {
            if (SelectOpenCoverExeEvent != null) 
                SelectOpenCoverExeEvent(this, new EventArgs());
        }

        public RelayCommand ProcessSelectOpenCoverExe { get; private set; }
    }
}

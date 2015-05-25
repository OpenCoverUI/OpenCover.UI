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
            ProcessSelectXUnitExe = new RelayCommand(FireSelectXUnitExeEvent);
            if (IsInDesignMode)
            {
                NUnitExePath = @"C:\nunit\nunit-console.exe";
                XUnitExePath = @"C:\xunit\xunit.console.exe";
                OpenCoverExePath = @"C:\opencover\opencover.console.exe";
            }
            else
            {
                _nUnitExePath = _settings.NUnitPath;
                _openCoverExePath = _settings.OpenCoverPath;
                _xUnitExePath = _settings.XUnitPath;
            }
        }

        private string _nUnitExePath;
        private string _openCoverExePath;
        private string _xUnitExePath;

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
        public event EventHandler<EventArgs> SelectXUnitExeEvent;
        
  
        private void FireSelectNUnitExeEvent()
        {
            if (SelectNunitExeEvent!=null) 
                SelectNunitExeEvent(this, new EventArgs());
        }

        private void FireSelectXUnitExeEvent()
        {
            if (SelectXUnitExeEvent != null)
                SelectXUnitExeEvent(this, new EventArgs());
        }

        public RelayCommand ProcessSelectNUnitExe { get; private set; }
        public RelayCommand ProcessSelectXUnitExe { get; private set; }

        public event EventHandler<EventArgs> SelectOpenCoverExeEvent;

        private void FireSelectOpenCoverExeEvent()
        {
            if (SelectOpenCoverExeEvent != null) 
                SelectOpenCoverExeEvent(this, new EventArgs());
        }

        public RelayCommand ProcessSelectOpenCoverExe { get; private set; }


        public string XUnitExePath
        {
            get { return _xUnitExePath; }
            set
            {
                Set(ref _xUnitExePath, value);
                if (_settings != null) _settings.XUnitPath = value;
            }
        }
    }
}

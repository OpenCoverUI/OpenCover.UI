using System.Windows.Controls;
using Microsoft.Win32;
using OpenCover.UI.Framework.ViewModel;

namespace OpenCover.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        private SettingsViewModel _vm;

        public SettingsControl()
        {
            InitializeComponent();
            _vm = new SettingsViewModel(OpenCoverUISettings.Default);
            this.DataContext = _vm;

            _vm.SelectNunitExeEvent += SelectNunitExeEvent;
            _vm.SelectOpenCoverExeEvent += SelectOpenCoverExeEvent;
        }

        void SelectOpenCoverExeEvent(object sender, System.EventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "OpenCover Executable (opencover.console.exe)|opencover.console.exe" };
            if (dialog.ShowDialog() == true)
            {
                _vm.OpenCoverExePath = dialog.FileName;
            }
        }

        void SelectNunitExeEvent(object sender, System.EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Nunit 2.x Executable (nunit-console*.exe)|nunit-console*.exe"
                      + "|Nunit 3.x Executable|nunit3-console.exe"
            };
            if (dialog.ShowDialog() == true)
            {
                _vm.NUnitExePath = dialog.FileName;
            }
        }
    }
}

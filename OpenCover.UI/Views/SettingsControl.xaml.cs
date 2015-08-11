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
            var dialog = new OpenFileDialog { Filter = "Executables (*.exe)|*.exe" };
            if (dialog.ShowDialog() == true)
            {
                _vm.OpenCoverExePath = dialog.FileName;
            }
        }

        void SelectNunitExeEvent(object sender, System.EventArgs e)
        {
            var dialog = new OpenFileDialog {Filter = "Executables (*.exe)|*.exe"};
            if (dialog.ShowDialog() == true)
            {
                _vm.NUnitExePath = dialog.FileName;
            }
        }
    }
}

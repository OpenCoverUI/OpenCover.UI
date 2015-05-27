using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.Settings;

namespace OpenCover.UI
{
    /// <summary>
    /// Settings container for this plugin
    /// </summary>
    public class Settings: INotifyPropertyChanged
    {
        private bool _showLinesColored;
        private bool _showCoverageGlyphs;
        private WritableSettingsStore _store;
        private const string SETTINGS_PATH = "OpenCoverUI";

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes the settings
        /// </summary>
        /// <param name="configurationSettingsStore">The settings store</param>
        public Settings(WritableSettingsStore configurationSettingsStore)
        {
            _store = configurationSettingsStore;

            _showLinesColored = _store.GetBoolean(SETTINGS_PATH, SettingNames.ShowLinesColored, false);
            _showCoverageGlyphs = _store.GetBoolean(SETTINGS_PATH, SettingNames.ShowCoverageGlyphs, true);
        }

        /// <summary>
        /// Gets or sets whether colored lines should be shown for coverage visualisation
        /// </summary>
        public bool ShowLinesColored 
        {
            get { return _showLinesColored; }
            set
            {
                if (value != ShowLinesColored)
                {
                    _showLinesColored = value;
                    RaisePropertyChanged();
                    WriteBoolean(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether glyphs for lines should be shown for coverage visualisation
        /// </summary>
        public bool ShowCoverageGlyphs
        {
            get { return _showCoverageGlyphs; }
            set
            {
                if (value != _showCoverageGlyphs)
                {
                    _showCoverageGlyphs = value;
                    RaisePropertyChanged();
                    WriteBoolean(value);
                }
            }
        }

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName"></param>
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Writes a boolean value to the settings store
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        private void WriteBoolean(bool value, [CallerMemberName] string propertyName = null)
        {
            if (!_store.CollectionExists(SETTINGS_PATH))
                _store.CreateCollection(SETTINGS_PATH);

            _store.SetBoolean(SETTINGS_PATH, propertyName, value);
        }

        public static class SettingNames
        {
            public const string ShowLinesColored = "ShowLinesColored";
            public const string ShowCoverageGlyphs = "ShowCoverageGlyphs";
        }
    }
}

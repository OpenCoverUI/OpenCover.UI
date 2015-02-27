using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using OpenCover.UI.Views;
using OpenCover.UI.Model.Test;
using OpenCover.UI.Helpers;

namespace OpenCover.UI.Commands
{
    /// <summary>
    /// Commands for the code coverage results toolbar
    /// </summary>
    internal class ResultsToolbarCommands
    {
        private static bool _initialized;		
		private static List<OleMenuCommand> _allCommands;

		/// <summary>
        /// Static constructor for ResultsToolbarCommands class.
		/// </summary>
        static ResultsToolbarCommands()
		{
			if (!_initialized)
			{
				Initialize();
			}

			OpenCoverUIPackage.Instance.VSEventsHandler.SolutionOpened += () => EnableDisableCommands(true);
			OpenCoverUIPackage.Instance.VSEventsHandler.SolutionClosing += () => EnableDisableCommands(false);
		}

        /// <summary>
        /// Gets all the commands.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        internal static IEnumerable<OleMenuCommand> Commands
        {
            get
            {
                return _allCommands;
            }
        }

        /// <summary>
        /// Enables or disables commands.
        /// </summary>
        /// <param name="state">Sets Enabled property of all commands to 'state'</param>
        private static void EnableDisableCommands(bool state)
        {
            _allCommands.ForEach(c => c.Enabled = state);
        }

        /// <summary>
        /// Initializes all test explorer toolbar commands.
        /// </summary>
        private static void Initialize()
        {
            if (!_initialized)
            {
                _allCommands = new List<OleMenuCommand>();

                // Show colored lines command
                _allCommands.Add(new OleMenuCommand((s, e) => ToggleShowColoredLines((OleMenuCommand)s),
                                    new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
                                                    (int)PkgCmdIDList.OpenCoverResultsToolbarShowLinesColoredButton)) { Enabled = false, Checked = OpenCoverUIPackage.Instance.Settings.ShowLinesColored });

                // Show coverage glyphs command
                _allCommands.Add(new OleMenuCommand((s, e) => ToggleShowCoverageGlyphs((OleMenuCommand)s),
                                    new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
                                                    (int)PkgCmdIDList.OpenCoverResultsToolbarShowCoverageGlyphsButton)) { Enabled = false, Checked = OpenCoverUIPackage.Instance.Settings.ShowCoverageGlyphs });

                _initialized = true;
            }
        }
        
        /// <summary>
        /// Action executed when user clicks on toolbar button
        /// </summary>
        private static void ToggleShowColoredLines(OleMenuCommand command)
        {
            OpenCoverUIPackage.Instance.Settings.ShowLinesColored = !OpenCoverUIPackage.Instance.Settings.ShowLinesColored;
            command.Checked = OpenCoverUIPackage.Instance.Settings.ShowLinesColored;
        }

        /// <summary>
        /// Action executed when user clicks on toolbar button
        /// </summary>
        private static void ToggleShowCoverageGlyphs(OleMenuCommand command)
        {
            OpenCoverUIPackage.Instance.Settings.ShowCoverageGlyphs = !OpenCoverUIPackage.Instance.Settings.ShowCoverageGlyphs;
            command.Checked = OpenCoverUIPackage.Instance.Settings.ShowCoverageGlyphs;
        }
    }
}

//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using OpenCover.Framework.Model;
using OpenCover.UI.Commands;
using OpenCover.UI.Helpers;
using OpenCover.UI.Processors;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using OpenCover.UI.Views.OptionPages;

namespace OpenCover.UI
{
	/// <summary>
	/// Visual Studio integration plugin for OpenCover
	/// </summary>
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource(1000, 1)]
	// This attribute registers a tool window exposed by this package. 
	[ProvideToolWindow(typeof(CodeCoverageResultsToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed,
		Orientation = ToolWindowOrientation.Bottom, Window = EnvDTE.Constants.vsWindowKindOutput)]
	[ProvideToolWindow(typeof(TestExplorerToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed,
		Orientation = ToolWindowOrientation.Left, Window = EnvDTE.Constants.vsWindowKindClassView)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    [ProvideOptionPageAttribute(typeof(OpenCoverUIOptionPage), "OpenCover.UI Options", "General", 100, 101, true, new string[] { "Change OpenCover.UI Options" })]
	[Guid(GuidList.GuidOpenCoverUIPkgString)]
	public sealed class OpenCoverUIPackage : Package
	{
		Dictionary<string, string> fileList = new Dictionary<string, string>();
		List<string> _openFiles = new List<string>();
		private Dictionary<string, string> _keysDictionary = new Dictionary<string, string>();
        private Settings _settings;

		internal static OpenCoverUIPackage Instance
		{
			get;
			private set;
		}

		internal EnvDTE.DTE DTE
		{
			get;
			private set;
		}

		internal VSEventsHandler VSEventsHandler
		{
			get;
			private set;
		}

		internal List<ToolWindowPane> ToolWindows
		{
			get;
			private set;
		}

		internal List<Command> Commands
		{
			get;
			private set;
		}

        /// <summary>
        /// Gets access to the package settings
        /// </summary>
        internal Settings Settings
        {
            get { return _settings; }
        }

		/// <summary>
		/// Default constructor of the package.
		/// </summary>
		public OpenCoverUIPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
			Commands = new List<Command>();
			ToolWindows = new List<ToolWindowPane>();
		}

		internal T GetToolWindow<T>() where T : ToolWindowPane
		{
			var toolWindow = ToolWindows.OfType<T>().FirstOrDefault();
			if (toolWindow == null)
			{
				Dispatcher.CurrentDispatcher.Invoke(new Action(() => { toolWindow = AddToolWindow<T>(); }));
			}

			return toolWindow;
		}

		private T AddToolWindow<T>() where T : ToolWindowPane
		{
			try
			{
				T toolWindow = FindToolWindow(typeof(T), 0, true) as T;

				ToolWindows.Add(toolWindow);

				if (toolWindow == null || toolWindow.Frame == null)
				{
					throw new NotSupportedException(Resources.CanNotCreateWindow);
				}
				else
				{
					((IVsWindowFrame)toolWindow.Frame).ShowNoActivate();
				}

				return toolWindow;
			}
			catch (Exception ex)
			{
				IDEHelper.WriteToOutputWindow(ex.Message);
				IDEHelper.WriteToOutputWindow(ex.StackTrace);
			}

			return null;
		}

		private void ShowMessageBox(string message)
		{
			IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
			Guid clsid = Guid.Empty;
			int result;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
					   0,
					   ref clsid,
					   "Code Coverage",
					   string.Format(CultureInfo.CurrentCulture, message, ToString()),
					   string.Empty,
					   0,
					   OLEMSGBUTTON.OLEMSGBUTTON_OK,
					   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
					   OLEMSGICON.OLEMSGICON_INFO,
					   0,        // false
					   out result));
		}

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", ToString()));
			base.Initialize();

			Instance = this;
			DTE = (Package.GetGlobalService(typeof(EnvDTE.DTE))) as EnvDTE.DTE;

            LoadSettings();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null != mcs)
			{
				VSEventsHandler = new VSEventsHandler(this);

				IVsUIShell uiShell = GetService(typeof(IVsUIShell)) as IVsUIShell;

				var executeSelectedTestsCommand = new ExecuteSelectedTestsCommand(this);
				mcs.AddCommand(executeSelectedTestsCommand);

				var codeCoverageToolWindowCommand = new CodeCoverageToolWindowCommand(this);
				mcs.AddCommand(codeCoverageToolWindowCommand);

				var testExplorerToolWindowCommand = new TestExplorerToolWindowCommand(this);
				mcs.AddCommand(testExplorerToolWindowCommand);

				var testSettingsFileSelectorCommand = new MSTestSettingsFileSelectorCommand(this);
				mcs.AddCommand(testSettingsFileSelectorCommand);

				foreach (var command in TestsExplorerToolbarCommands.Commands)
				{
					mcs.AddCommand(command);
				}

                foreach (var command in ResultsToolbarCommands.Commands)
                {
                    mcs.AddCommand(command);
                }

				Commands.Add(executeSelectedTestsCommand);
				Commands.Add(codeCoverageToolWindowCommand);
				Commands.Add(testExplorerToolWindowCommand);
				Commands.Add(testSettingsFileSelectorCommand);
			}
		}

        private void LoadSettings()
        {
            SettingsManager settingsManager = new ShellSettingsManager(this);
            WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            _settings = new Settings(configurationSettingsStore);
        }

        //internal IWpfTextViewHost GetCurrentViewHost()
        //{
        //    // code to get access to the editor's currently selected text cribbed from
        //    // http://msdn.microsoft.com/en-us/library/dd884850.aspx
        //    IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
        //    IVsTextView vTextView = null;
        //    int mustHaveFocus = 1;
        //    txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
        //    IVsUserData userData = vTextView as IVsUserData;
  
        //    if (userData == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        IWpfTextViewHost viewHost;
        //        object holder;
        //        Guid guidViewHost = DefGuidList.guidIWpfTextViewHost;
        //        userData.GetData(ref guidViewHost, out holder);
        //        viewHost = (IWpfTextViewHost)holder;
        //        return viewHost;
        //    }
        //}
	}
}
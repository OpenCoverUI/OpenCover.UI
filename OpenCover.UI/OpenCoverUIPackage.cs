//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
	[Guid(GuidList.GuidOpenCoverUIPkgString)]
	public sealed class OpenCoverUIPackage : Package
	{
		Dictionary<string, string> fileList = new Dictionary<string, string>();
		List<string> _openFiles = new List<string>();
		private Dictionary<string, string> _keysDictionary = new Dictionary<string, string>();

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
		/// Default constructor of the package.
		/// </summary>
		public OpenCoverUIPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
			Commands = new List<Command>();
			ToolWindows = new List<ToolWindowPane>();
		}

		private void AddToolWindow<T>() where T : ToolWindowPane
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

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null != mcs)
			{
				VSEventsHandler = new VSEventsHandler(this);

				AddToolWindow<CodeCoverageResultsToolWindow>();
				AddToolWindow<TestExplorerToolWindow>();

				IVsUIShell uiShell = GetService(typeof(IVsUIShell)) as IVsUIShell;

				var executeSelectedTestsCommand = new ExecuteSelectedTestsCommand(this);
				mcs.AddCommand(executeSelectedTestsCommand);

				var codeCoverageToolWindowCommand = new CodeCoverageToolWindowCommand(this);
				mcs.AddCommand(codeCoverageToolWindowCommand);

				var testExplorerToolWindowCommand = new TestExplorerToolWindowCommand(this);
				mcs.AddCommand(testExplorerToolWindowCommand);

				foreach (var command in TestsExplorerToolbarCommands.Commands)
				{
					mcs.AddCommand(command);
				}

				Commands.Add(executeSelectedTestsCommand);
				Commands.Add(codeCoverageToolWindowCommand);
				Commands.Add(testExplorerToolWindowCommand);
			}
		}
	}

}
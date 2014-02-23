//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.Model;
using Microsoft.VisualStudio.TestWindow.UI;
using OpenCover.Framework.Model;
using OpenCover.UI.Commands;
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
	[ProvideMenuResource("Menus.ctmenu", 1)]
	// This attribute registers a tool window exposed by this package. 
	[ProvideToolWindow(typeof(CodeCoverageResultsToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed,
		Orientation = ToolWindowOrientation.Bottom, Window = EnvDTE.Constants.vsWindowKindClassView)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
	[Guid(GuidList.GuidOpenCoverUIPkgString)]
	public sealed class OpenCoverUIPackage : Package
	{
		Dictionary<string, string> fileList = new Dictionary<string, string>();
		List<string> _openFiles = new List<string>();
		private Dictionary<string, string> _keysDictionary = new Dictionary<string, string>();
		private ExecuteSelectedTestsCommand _executeSelectedTestsCommand;
		private CodeCoverageToolWindowCommand _codeCoverageToolWindowCommand;

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

		internal CodeCoverageResultsToolWindow CodeCoverageResultsToolWindow
		{
			get;
			private set;
		}

		internal CodeCoverageResultsControl CodeCoverageResultsControl
		{
			get;
			private set;
		}

		/// <summary>
		/// Default constructor of the package.
		/// </summary>
		public OpenCoverUIPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

		private void SetCodeCoverageResultsToolWindow()
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			CodeCoverageResultsToolWindow = this.FindToolWindow(typeof(CodeCoverageResultsToolWindow), 0, true) as CodeCoverageResultsToolWindow;

			if ((null == CodeCoverageResultsToolWindow) || (null == CodeCoverageResultsToolWindow.Frame))
			{
				throw new NotSupportedException(Resources.CanNotCreateWindow);
			}
			else
			{
				CodeCoverageResultsControl = CodeCoverageResultsToolWindow.Content as CodeCoverageResultsControl;
				var frame = CodeCoverageResultsToolWindow.Frame as IVsWindowFrame;
				Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.Show());
				frame.Hide();
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
					   string.Format(CultureInfo.CurrentCulture, message, this.ToString()),
					   string.Empty,
					   0,
					   OLEMSGBUTTON.OLEMSGBUTTON_OK,
					   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
					   OLEMSGICON.OLEMSGICON_INFO,
					   0,        // false
					   out result));
		}

		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null != mcs)
			{
				IVsUIShell uiShell = GetService(typeof(IVsUIShell)) as IVsUIShell;
				_executeSelectedTestsCommand = new ExecuteSelectedTestsCommand(this, uiShell);
				mcs.AddCommand(_executeSelectedTestsCommand);

				_codeCoverageToolWindowCommand = new CodeCoverageToolWindowCommand(this);
				mcs.AddCommand(_codeCoverageToolWindowCommand);
			}

			DTE = (Package.GetGlobalService(typeof(EnvDTE.DTE))) as EnvDTE.DTE;

			SetCodeCoverageResultsToolWindow();
			
			VSEventsHandler = new VSEventsHandler(this);

			Instance = this;
		}
		#endregion

		internal void ShowResultsCodeCoverageResultsToolWindow()
		{
			this._codeCoverageToolWindowCommand.Invoke();
		}
	}

}
//
// This source code is released under the GPL License; Please read license.md file for more details
//
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace OpenCover.UI.Views
{
	[Guid("D92F85BD-AFD0-442F-9443-124A706F8D9C")]
	public class TestExplorerToolWindow : ToolWindowPane
	{
		internal TestExplorerControl TestExplorerControl
		{
			get;
			private set;
		}
		
		internal OleMenuCommandService mcs;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestExplorerToolWindow"/> class.
		/// </summary>
		public TestExplorerToolWindow()
		{
			// Set the window title reading it from the resources.
			this.Caption = Resources.TestExplorerToolWindowTitle;
			// Set the image that will appear on the tab of the window frame
			// when docked with an other window
			// The resource ID correspond to the one defined in the resx file
			// while the Index is the offset in the bitmap strip. Each image in
			// the strip being 16x16.
			this.BitmapResourceID = 301;
			this.BitmapIndex = 1;

			mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

			TestExplorerControl = new TestExplorerControl(this);

			base.Content = TestExplorerControl;

			this.ToolBar = new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet, PkgCmdIDList.OpenCoverTestExplorerToolbar);
		}

		/// <summary>
		/// This method can be overridden by the derived class to execute any code that must run after the creation of <see cref="T:Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame" />.
		/// </summary>
		public override void OnToolWindowCreated()
		{
			TestExplorerControl.Initialize(Package as OpenCoverUIPackage);
		}
	}
}

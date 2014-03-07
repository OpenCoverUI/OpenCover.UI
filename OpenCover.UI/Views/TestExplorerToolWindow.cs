using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

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
		}

		public override void OnToolWindowCreated()
		{
			TestExplorerControl.Package = Package as OpenCoverUIPackage;
			TestExplorerControl.Initialize();
		}
	}
}

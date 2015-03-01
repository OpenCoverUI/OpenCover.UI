//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;

namespace OpenCover.UI
{
	static class GuidList
	{
		public const string GuidOpenCoverUIPkgString = "8baa86ee-ed13-4508-bccf-d580eaf81949";
		public const string GuidOpenCoverUICmdSetString = "c1d4257b-f994-4cfe-adb6-f780c29f7c48";
		public const string GuidToolWindowPersistanceString = "47dfb4eb-7c9f-45a8-938e-90fde05d0387";
		public const string GuidTestExplorerToolWindowString = "E1B7D1F8-9B3C-49B1-8F4F-BFC63A88835D";
		public const string GuidTestExplorerToolWindowContextMenu = "1E198C22-5980-4E7E-92F3-F73168D1FB63";

		public static readonly Guid GuidOpenCoverUICmdSet = new Guid(GuidOpenCoverUICmdSetString);
		public static Guid GuidOpenCoverTestExplorerContextMenuCommandSet = new Guid("81F1321F-B605-47F6-AD43-FB2EC4891225");
	};
}
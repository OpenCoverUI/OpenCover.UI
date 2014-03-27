//
// This source code is released under the GPL License; Please read license.md file for more details.
//
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

namespace OpenCover.UI.Commands
{
	public class TestsExplorerToolbarCommands
	{
		private static bool _initialized;
		private static OleMenuCommand _currentSelected;
		private static List<OleMenuCommand> _allCommands;

		static TestsExplorerToolbarCommands()
		{
			if (!_initialized)
			{
				Initialize();
			}

			OpenCoverUIPackage.Instance.VSEventsHandler.SolutionOpened += () => EnableDisableCommands(true);
			OpenCoverUIPackage.Instance.VSEventsHandler.SolutionClosing += () => EnableDisableCommands(false);
		}

		public static IEnumerable<OleMenuCommand> Commands
		{
			get
			{
				return _allCommands;
			}
		}

		private static void EnableDisableCommands(bool state)
		{
			_allCommands.ForEach(c => c.Enabled = state);
		}

		private static void Initialize()
		{
			_allCommands = new List<OleMenuCommand>();

			_allCommands.Add(new OleMenuCommand(Executed, new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
								(int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByClassButton)) { Enabled = false });

			_allCommands.Add(new OleMenuCommand(Executed, new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
								(int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByTraitButton)) { Enabled = false });

			_allCommands.Add(new OleMenuCommand(Executed, new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
								(int)PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByProjectButton)) { Enabled = false });

			_allCommands.Add(new OleMenuCommand((s, e) => OpenCoverUIPackage.Instance.VSEventsHandler.BuildSolution(),
								new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
												(int)PkgCmdIDList.OpenCoverTestExplorerToolbarRefreshButton)) { Enabled = false });

			_initialized = true;
		}

		private static void Executed(object sender, EventArgs e)
		{
			var clicked = sender as OleMenuCommand;

			UpdateCheckedMethods(clicked);

			var groupingField = TestMethodGroupingField.Class;

			switch (clicked.CommandID.ID)
			{
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByClassButton:
					groupingField = TestMethodGroupingField.Class;
					break;
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByTraitButton:
					groupingField = TestMethodGroupingField.Trait;
					break;
				case PkgCmdIDList.OpenCoverTestExplorerToolbarGroupByProjectButton:
					groupingField = TestMethodGroupingField.Project;
					break;
			}

			OpenCoverUIPackage.Instance.ToolWindows.OfType<TestExplorerToolWindow>().First().TestExplorerControl.ChangeGroupBy(groupingField);
		}

		private static void UpdateCheckedMethods(OleMenuCommand clicked)
		{
			if (_currentSelected != null)
			{
				_currentSelected.Checked = false;
			}

			_currentSelected = clicked;

			_currentSelected.Checked = true;
		}
	}
}

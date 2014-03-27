//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Helpers;
using OpenCover.UI.Model.Test;
using OpenCover.UI.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

namespace OpenCover.UI.Views
{
	/// <summary>
	/// Interaction logic for TestExplorerControl.xaml
	/// </summary>
	public partial class TestExplorerControl : UserControl
	{
		private OpenCoverUIPackage _package;
		private TestExplorerToolWindow _parent;
		private TestMethodGroupingField _groupField;
		private List<TestClass> _tests;

		public event Action TestDiscoveryFinished;

		public TestExplorerControl(TestExplorerToolWindow parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		internal void Initialize(OpenCoverUIPackage package)
		{
			_package = package;
			_package.VSEventsHandler.BuildDone += DiscoverTests;
			_package.VSEventsHandler.SolutionOpened += DiscoverTests;
			_package.VSEventsHandler.SolutionClosing += ClearTestsTreeViewChildren;
		}

		internal void ChangeGroupBy(TestMethodGroupingField groupingField)
		{
			_groupField = groupingField;
			UpdateTreeView(_tests);
		}

		private void DiscoverTests()
		{
			System.Threading.Tasks.Task.Factory.StartNew(new Action(() =>
			{
				var potentialTestDLLs = IDEHelper.GetPotentialTestDLLs();
				var testDiscoverer = new Discoverer(potentialTestDLLs);

				testDiscoverer.Discover(UpdateTreeView);
			}));
		}

		/// <summary>
		/// Updates the TreeView by adding .
		/// </summary>
		/// <param name="tests">The tests.</param>
		private void UpdateTreeView(List<TestClass> tests)
		{
			_tests = tests;

			if (tests != null)
			{
				ClearTestsTreeViewChildren();

				Dispatcher.BeginInvoke(new Action(() =>
				{
					TestsTreeView.Root = new TestMethodWrapperContainer(tests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods)), _groupField);

					if (TestDiscoveryFinished != null)
					{
						TestDiscoveryFinished();
					}
				}));
			}
		}

		private void ClearTestsTreeViewChildren()
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				try
				{
					if (TestsTreeView.Root != null && TestsTreeView.Root.Children != null && TestsTreeView.Root.Children.Any())
					{
						TestsTreeView.Root.Children.Clear();
					}
				}
				catch { }
			}));
		}

		private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			OleMenuCommandService mcs = this._parent.mcs;
			if (null != mcs)
			{
				CommandID menuID = new CommandID(GuidList.GuidOpenCoverTestExplorerContextMenuCommandSet,
												PkgCmdIDList.CommandIDOpenCoverTestExplorerContextMenu);
				Point p = this.PointToScreen(e.GetPosition(this));
				mcs.ShowContextMenu(menuID, (int)p.X, (int)p.Y);
			}
		}

		private void Refresh(object sender, MouseButtonEventArgs e)
		{
			_package.VSEventsHandler.BuildSolution();
		}
	}
}

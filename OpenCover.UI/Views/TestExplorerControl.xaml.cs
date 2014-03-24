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
		public OpenCoverUIPackage Package { get; set; }
		TestExplorerToolWindow _parent;
		TestMethodGroupingField _groupField;
		List<TestClass> _tests;

		public event Action TestDiscoveryFinished;

		public TestExplorerControl(TestExplorerToolWindow parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		internal void Initialize()
		{
			Package.VSEventsHandler.BuildDone += DiscoverTests;
			Package.VSEventsHandler.SolutionOpened += DiscoverTests;
			cmbGroupBy.SelectionChanged += cmbGroupBy_SelectionChanged;
		}

		void cmbGroupBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch(cmbGroupBy.SelectedIndex)
			{
				case 0: _groupField = TestMethodGroupingField.Class;
					break;
				case 1: _groupField = TestMethodGroupingField.Trait;
					break;
				default:
					_groupField = TestMethodGroupingField.Class;
					break;
			}

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
					TestsTreeView.Root = new TestMethodWrapperContainer(tests.OrderBy(test => test.Name)
											.Select(test => new TestMethodWrapper(test.Name, test.TestMethods)), _groupField);

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
				if (TestsTreeView.Root != null && TestsTreeView.Root.Children != null)
				{
					TestsTreeView.Root.Children.Clear();
				}
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

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DiscoverTests();
		}
	}
}

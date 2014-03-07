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
		}

		private void DiscoverTests()
		{
			System.Threading.Tasks.Task.Factory.StartNew(new Action(() =>
			{
				var potentialTestDLLs = IDEHelper.GetPotentialTestDLLs();
				var testDiscoverer = new Discoverer(potentialTestDLLs);
				var tests = testDiscoverer.Discover();

				Dispatcher.BeginInvoke(new Action(() =>
				{
					TestsTreeView.Root = new TestClassContainer(tests.OrderBy(test => test.Name));

					if (TestDiscoveryFinished != null)
					{
						TestDiscoveryFinished();
					}
				}));
			}));
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DiscoverTests();
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
	}
}

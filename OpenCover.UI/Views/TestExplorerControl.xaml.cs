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
using System.Text;

namespace OpenCover.UI.Views
{
	/// <summary>
	/// Interaction logic for TestExplorerControl.xaml
	/// </summary>
	public partial class TestExplorerControl : UserControl
	{
		private OpenCoverUIPackage _package;
		private TestExplorerToolWindow _parent;
		private TestMethodGroupingField _currentGroupingField;

		internal IEnumerable<TestClass> Tests { get; private set; }

		internal static event Action TestDiscoveryFinished;

		public TestExplorerControl(TestExplorerToolWindow parent)
		{
			_parent = parent;
			InitializeComponent();
		}

		/// <summary>
		/// Initializes the control by adding handlers to BuildDone, SolutionOpened & SolutionClosing events.
		/// </summary>
		/// <param name="package">The package.</param>
		internal void Initialize(OpenCoverUIPackage package)
		{
			_package = package;
			_package.VSEventsHandler.BuildDone += DiscoverTests;
			_package.VSEventsHandler.SolutionOpened += DiscoverTests;
			_package.VSEventsHandler.SolutionClosing += ClearTestsTreeViewChildren;
		}

		/// <summary>
		/// Changes the group by in OpenCover Test Explorer.
		/// </summary>
		/// <param name="groupingField">The grouping field.</param>
		internal void ChangeGroupBy(TestMethodGroupingField groupingField)
		{
			_currentGroupingField = groupingField;
			UpdateTreeView(Tests);
		}

		internal void Update()
		{
			Dispatcher.BeginInvoke(new Action(() =>
			{
				ExpandNodes(TestsTreeView.Root);
			}));
		}

		private static void ExpandNodes(ICSharpCode.TreeView.SharpTreeNode parentNode)
		{
			if (parentNode == null || parentNode.Children == null)
			{
				return;
			}

			var selectedNodes = parentNode.Children.Where(c => c.IsExpanded);

			foreach (var node in selectedNodes)
			{
				node.IsExpanded = false;
				node.IsExpanded = true;

				ExpandNodes(node);
			}
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
		private void UpdateTreeView(IEnumerable<TestClass> tests)
		{
			Tests = tests;

			if (tests != null)
			{
				ClearTestsTreeViewChildren();

				Dispatcher.BeginInvoke(new Action(() =>
				{
					//TODO: Refactor code to make grouping dynamic. Works for now but becomes difficult to manage if we need to support multiple Test Frameworks.

					var msTests = tests.Where(tc => tc.TestType == TestType.MSTest);
					var nUnitTests = tests.Where(tc => tc.TestType == TestType.NUnit);

					bool hasMSTests = msTests.Any();
					bool hasNUnitTests = nUnitTests.Any();

					if (hasMSTests && hasNUnitTests)
					{
						TestsTreeView.Root = new ICSharpCode.TreeView.SharpTreeNode();
						TestsTreeView.Root.Children.Add(new TestMethodWrapperContainer("MSTest", msTests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, TestType.MSTest));
						TestsTreeView.Root.Children.Add(new TestMethodWrapperContainer("NUnit Test", nUnitTests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, TestType.NUnit));
					}
					else
					{
						TestType testType = hasMSTests ? TestType.MSTest : hasNUnitTests ? TestType.NUnit : TestType.MSTest;

						TestsTreeView.Root = new TestMethodWrapperContainer(null, tests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, testType);
					}

					if (TestDiscoveryFinished != null)
					{
						TestDiscoveryFinished();
					}
				}));
			}
		}

		/// <summary>
		/// Clears the tests TreeView children.
		/// </summary>
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

		/// <summary>
		/// Handles the MouseRightButtonDown event of the Grid control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
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

		/// <summary>
		/// Refreshes Test Explorer control.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
		private void Refresh(object sender, MouseButtonEventArgs e)
		{
			_package.VSEventsHandler.BuildSolution();
		}
	}
}

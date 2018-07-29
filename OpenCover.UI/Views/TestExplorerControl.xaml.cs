//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Shell;
using OpenCover.UI.Commands;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using OpenCover.UI.Model.Test;
using OpenCover.UI.Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenCover.UI.Views
{
    /// <summary>
    /// Interaction logic for TestExplorerControl.xaml
    /// </summary>
    public partial class TestExplorerControl : UserControl, INotifyPropertyChanged
    {
        private OpenCoverUIPackage _package;
        private TestExplorerToolWindow _parent;
        private TestMethodGroupingField _currentGroupingField;
        private TestResultsViewModel _testResult;

        internal IEnumerable<TestClass> Tests { get; private set; }

        public TestResultsViewModel TestResult
        {
            get
            {
                return _testResult;
            }
            private set
            {
                _testResult = value;
                NotifyPropertyChanged("TestResult");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        internal static event Action TestDiscoveryFinished;

        public TestExplorerControl(TestExplorerToolWindow parent)
        {
            _parent = parent;
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Initializes the control by adding handlers to BuildDone, SolutionOpened & SolutionClosing events.
        /// </summary>
        /// <param name="package">The package.</param>
        internal void Initialize(OpenCoverUIPackage package)
        {
            _package = package;
            _package.VSEventsHandler.BuildSucceeded += DiscoverTests;
            _package.VSEventsHandler.BuildFailed += () => { IDEHelper.WriteToOutputWindow("Build failed. Please make sure your solution builds properly before refreshing this window."); };
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
            TestsExplorerToolbarCommands.UpdateSelectedGroupBy(groupingField);

            UpdateTreeView(Tests);
        }

        /// <summary>
        /// Discovers the tests.
        /// </summary>
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
                    var xUnitTests = tests.Where(tc => tc.TestType == TestType.XUnit);

                    bool hasMSTests = msTests.Any();
                    bool hasNUnitTests = nUnitTests.Any();
                    bool hasXUnitTests = xUnitTests.Any();

                    bool hasMoreThanOneTestType = hasMSTests ? (hasNUnitTests || hasXUnitTests) : (hasNUnitTests && hasXUnitTests);

                    if (hasMoreThanOneTestType)
                    {
                        TestsTreeView.Root = new ICSharpCode.TreeView.SharpTreeNode();

                        if (hasMSTests)
                            TestsTreeView.Root.Children.Add(new TestMethodWrapperContainer("MSTest", msTests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, TestType.MSTest));
                        if (hasNUnitTests)
                            TestsTreeView.Root.Children.Add(new TestMethodWrapperContainer("NUnit Test", nUnitTests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, TestType.NUnit));
                        if (hasXUnitTests)
                            TestsTreeView.Root.Children.Add(new TestMethodWrapperContainer("XUnit Test", xUnitTests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, TestType.XUnit));

                    }
                    else
                    {
                        TestType testType = hasMSTests ? TestType.MSTest : hasNUnitTests ? TestType.NUnit : hasXUnitTests ? TestType.XUnit : TestType.MSTest;

                        TestsTreeView.Root = new TestMethodWrapperContainer(null, tests.Select(test => new TestMethodWrapper(test.Name, test.TestMethods, String.Format("{0}.{1}", test.Namespace, test.Name))), _currentGroupingField, testType);
                    }

                    if (TestDiscoveryFinished != null)
                    {
                        TestDiscoveryFinished();
                    }

                    TestSelectionChanged(null, null);
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

        /// <summary>
        /// Handler for selection change event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void TestSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TestsTreeView.SelectedItems.Count == 1 && TestsTreeView.SelectedItems[0] is TestMethod)
            {
                var testMethod = TestsTreeView.SelectedItems[0] as TestMethod;
                TestResult = new TestResultsViewModel(testMethod.Name,
                                                      testMethod.ExecutionResult,
                                                      null);
            }
            else
            {
                dynamic groupedTests = Tests.SelectMany(t => t.TestMethods)
                                                          .GroupBy(t => t.ExecutionResult.Status)
                                                          .Select(t => new { Status = t.Key, Count = t.Count() })
                                                          .Where(t => t.Count > 0);

                TestResult = new TestResultsViewModel("Summary", null, groupedTests);
            }
        }

        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        private void TestsTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var treeView = sender as ICSharpCode.TreeView.SharpTreeView;
                var selectedItem = treeView.SelectedItem;

                var testMethod = selectedItem as TestMethod;
                if (testMethod != null)
                {
                    var fullyQualifiedMethodName = testMethod.FullyQualifiedName;
                    IDEHelper.WriteToOutputWindow("Navigating to test method: {0}", fullyQualifiedMethodName);

                    IDEHelper.OpenFileByFullyQualifiedMethodName(fullyQualifiedMethodName);
                }
            }
            catch (Exception exception)
            {
                IDEHelper.WriteToOutputWindow(exception.Message);
            }
        }
    }

    public class TestResultsViewModel
    {
        public TestResultsViewModel(string caption, TestResult testResult, dynamic executionStatus)
        {
            Result = testResult;
            Caption = caption;
            ExecutionStatus = executionStatus;
        }

        public string Caption { get; set; }

        public dynamic ExecutionStatus { get; set; }

        public TestResult Result { get; set; }
    }
}

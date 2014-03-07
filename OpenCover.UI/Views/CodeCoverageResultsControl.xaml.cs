//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.Shell;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenCover.UI.Views
{
	/// <summary>
	/// Interaction logic for CodeCoverageResultsControl.xaml
	/// </summary>
	public partial class CodeCoverageResultsControl : UserControl
	{
		/// <summary>
		/// The last file selected by user to see coverage details
		/// </summary>
		private string _lastSelectedFile;

		/// <summary>
		/// This variable holds temporary information whether the current file open operation was initiated by this control (to show code coverage).
		/// Its value will be set to false after the IsFileOpening property is accessed the first time.
		/// </summary>
		private bool _fileOpening;

		/// <summary>
		/// The coverage session for current OpenCoer run
		/// </summary>
		public CoverageSession CoverageSession;

		/// <summary>
		/// Gets or sets the package.
		/// </summary>
		/// <value>
		/// The package.
		/// </value>
		public OpenCoverUIPackage Package { get; set; }

		/// <summary>
		/// Gets a value indicating whether the control initiated a File Open command. If so, it will set it to false before returning true.
		/// This should only be called by TextTagger class
		/// </summary>
		/// <value>
		///   <c>true</c> if [is file opening]; otherwise, <c>false</c>.
		/// </value>
		public bool IsFileOpening
		{
			get
			{
				if (_fileOpening)
				{
					_fileOpening = false;
					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeCoverageResultsControl"/> class.
		/// </summary>
		public CodeCoverageResultsControl()
		{
			//TODO: Add busy indicator

			InitializeComponent();
			DataContext = this;
		}

		/// <summary>
		/// Updates the coverage results for current OpenCover run on the UI thread.
		/// </summary>
		/// <param name="data">The CoverageSession data.</param>
		public void UpdateCoverageResults(CoverageSession data)
		{
			CoverageSession = data;

			if (CoverageSession != null)
			{
				Dispatcher.BeginInvoke(new Action(() =>
				{
					CodeCoverageResultsTreeView.Root = new CoverageNode(data);
				}), null);
			}
		}

		/// <summary>
		/// Clears the TreeView.
		/// </summary>
		public void ClearTreeView()
		{
			if (CodeCoverageResultsTreeView.Root != null && CodeCoverageResultsTreeView.Root.Children != null)
			{
				CodeCoverageResultsTreeView.Root.Children.Clear(); 
			}
		}

		/// <summary>
		/// Returns the active document's sequence points.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<SequencePoint> GetActiveDocumentSequencePoints()
		{
			return CoverageSession.GetSequencePoints(_lastSelectedFile);
		}

		/// <summary>
		/// TreeViewItem double click event handler.
		/// Opens the corresponding file if the clicked node represents either a class or a method. 
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
		private void TreeViewItemDoubleClicked(object sender, MouseButtonEventArgs e)
		{
			Method method = null;

			var treeView = sender as ICSharpCode.TreeView.SharpTreeView;

			if (treeView.SelectedItem is ClassNode)
			{
				method = (treeView.SelectedItem as ClassNode).Class.CoveredMethods.FirstOrDefault();
			}
			else if (treeView.SelectedItem is MethodNode)
			{
				method = (treeView.SelectedItem as MethodNode).Method;
			}

			if (method != null)
			{
				var sequencePoints = CoverageSession.GetSequencePoints().Where(ig => ig.Key == method.FileRef.UniqueId);

				var coveredFiles = CoverageSession.GetFiles();

				if (coveredFiles != null)
				{
					var file = coveredFiles.FirstOrDefault(f => f.UniqueId == method.FileRef.UniqueId);

					IDEHelper.CloseFile(Package.DTE, file.FullPath);

					_fileOpening = true;
					_lastSelectedFile = file.FullPath;
					try
					{

						IDEHelper.OpenFile(Package.DTE, file.FullPath);
						IDEHelper.GoToLine(Package.DTE, method.SequencePoints.FirstOrDefault().StartLine);
					}
					catch {}
				}

				e.Handled = true;
			}
		}
	}
}
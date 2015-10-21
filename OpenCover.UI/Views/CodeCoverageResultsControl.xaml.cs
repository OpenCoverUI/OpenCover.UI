//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Shell;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using OpenCover.UI.Model.ResultNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenCover.UI.Views
{
	/// <summary>
	/// Interaction logic for CodeCoverageResultsControl.xaml
	/// </summary>
	public partial class CodeCoverageResultsControl : UserControl, INotifyPropertyChanged
	{
		/// <summary>
		/// The last file selected by user to see coverage details
		/// </summary>
		private string _lastSelectedFile;

		/// <summary>
		/// The _package
		/// </summary>
		private OpenCoverUIPackage _package;

		private bool _isLoading;

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
		/// Gets or sets a value indicating whether [is loading].
		/// </summary>
		/// <value>
		///   <c>true</c> if [is loading]; otherwise, <c>false</c>.
		/// </value>
		public bool IsLoading
		{
			get
			{
				return _isLoading;
			}
			set
			{
				_isLoading = value;
				if (PropertyChanged != null)
				{
					PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
				}
			}
		}

        /// <summary>
        /// Will be raised when the code coverage process was done
        /// </summary>
        public event EventHandler NewCoverageDataAvailable;

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
			InitializeComponent();
			DataContext = this;
		}

		internal void Initialize(OpenCoverUIPackage package)
		{
			_package = package;
			_package.VSEventsHandler.SolutionClosing += ClearTreeView;
			_package.VSEventsHandler.SolutionOpened += ClearTreeView;

            _package.Settings.PropertyChanged += PackageSettings_PropertyChanged;

		}

	    private void PackageSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
	    {
	        if (string.Equals(e.PropertyName, "ShowUncoveredClasses"))
	        {
	            UpdateCoverageResults();
	        }
	    }

	    /// <summary>
		/// Updates the coverage results for current OpenCover run on the UI thread.
		/// </summary>
		/// <param name="data">The CoverageSession data.</param>
		public void UpdateCoverageResults(CoverageSession data)
		{
		    CoverageSession = data;

		    UpdateCoverageResults();
		}

	    public void UpdateCoverageResults()
	    {
	        if (CoverageSession != null)
	        {
	            Dispatcher.BeginInvoke(new Action(() =>
	            {
	                CodeCoverageResultsTreeView.Root = new CoverageNode(CoverageSession);
	                IsLoading = false;

	                if (NewCoverageDataAvailable != null)
	                    NewCoverageDataAvailable(this, EventArgs.Empty);
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
				method = (treeView.SelectedItem as ClassNode).Class.Methods.FirstOrDefault();
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
					try
					{
						var file = coveredFiles.FirstOrDefault(f => f.UniqueId == method.FileRef.UniqueId);

						IDEHelper.CloseFile(_package.DTE, file.FullPath);

						_fileOpening = true;
						_lastSelectedFile = file.FullPath;


						IDEHelper.OpenFile(_package.DTE, file.FullPath);
						IDEHelper.GoToLine(_package.DTE, method.SequencePoints.FirstOrDefault().StartLine);
					}
					catch { }
				}

				e.Handled = true;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
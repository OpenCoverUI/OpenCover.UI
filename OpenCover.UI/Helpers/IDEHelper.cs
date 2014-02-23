//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestWindow.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace OpenCover.UI.Helpers
{
	public static class IDEHelper
	{
		/// <summary>
		/// Returns selected Test cases.
		/// </summary>
		/// <returns>Selected Test cases</returns>
		public static TestTreeControl GetTestTreeControl(IVsUIShell uiShell)
		{
			// TODO: Refactor this method to make it more elegant. 
			// Currently, it uses reflection to get the TestsTreeView.ItemsSource which is a list of all tests. It works for now!

			var guid = new Guid(GuidList.GuidTestExplorerToolWindowString);
			IVsWindowFrame frame = null;
			var toolWindow = uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fFindFirst, ref guid, out frame);

			var windowFrame = frame as WindowFrame;
			var contentPresenter = windowFrame.FrameView.Content;
			var frameworkElement = GetPropertyValue<Object>(contentPresenter, "Content");
			var frameworkElementContent = GetPropertyValue<Object>(frameworkElement, "Content");
			var hasTestsGrid = GetPropertyValue<Grid>(frameworkElementContent, "HasTestsGrid");

			var multipleGrid = hasTestsGrid.Children.Cast<object>().FirstOrDefault(c => c.GetType() == typeof(Grid) && ((Grid)c).Uid == "MultipleGrid") as Grid;
			var leftGrid = multipleGrid.Children.Cast<object>().FirstOrDefault(c => c.GetType() == typeof(Grid) && ((Grid)c).Uid == "LeftGrid") as Grid;
			var summaryControl = leftGrid.Children.Cast<Object>().FirstOrDefault(c => c.GetType() == typeof(SummaryControl)) as SummaryControl;

			return summaryControl.TestsTreeView as TestTreeControl;
		}

		/// <summary>
		/// Opens the file in Visual Studio.
		/// </summary>
		/// <param name="file">The file path.</param>
		public static void OpenFile(EnvDTE.DTE DTE, string file)
		{
			try
			{
				if (System.IO.File.Exists(file))
				{
					DTE.ItemOperations.OpenFile(file);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Closes the file.
		/// </summary>
		/// <param name="DTE">The DTE.</param>
		/// <param name="fileName">Name of the file.</param>
		public static void CloseFile(EnvDTE.DTE DTE, string fileName)
		{
			foreach (EnvDTE.Document document in DTE.Documents)
			{
				if (fileName.Equals(document.FullName, StringComparison.InvariantCultureIgnoreCase))
				{
					document.Close();
					break;
				}
			}
		}

		/// <summary>
		/// Moves the caret to line number.
		/// </summary>
		/// <param name="DTE">The DTE.</param>
		/// <param name="lineNumber">The line number.</param>
		public static void GoToLine(EnvDTE.DTE DTE, int lineNumber)
		{
			DTE.ExecuteCommand("GotoLn", lineNumber.ToString());
		}

		/// <summary>
		/// Returns the property value .
		/// </summary>
		/// <typeparam name="T">Generic Type for value of the property</typeparam>
		/// <param name="obj">The object.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>Value of the property as T</returns>
		private static T GetPropertyValue<T>(Object obj, string propertyName) where T : class
		{
			return obj.GetType().GetProperty(propertyName).GetValue(obj) as T;
		}
	}
}

//
// This source code is released under the MIT License;
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using OpenCover.Framework.Model;

namespace OpenCover.UI.Views
{
	/// <summary>
	/// This class implements the tool window exposed by this package and hosts a user control.
	///
	/// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
	/// usually implemented by the package implementer.
	///
	/// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
	/// implementation of the IVsUIElementPane interface.
	/// </summary>
	[Guid("47dfb4eb-7c9f-45a8-938e-90fde05d0387")]
	public class CodeCoverageResultsToolWindow : ToolWindowPane
	{
		CodeCoverageResultsControl _codeCoverageResultsControl;
		/// <summary>
		/// Standard constructor for the tool window.
		/// </summary>
		public CodeCoverageResultsToolWindow() :
			base(null)
		{
			// Set the window title reading it from the resources.
			this.Caption = Resources.ToolWindowTitle;
			// Set the image that will appear on the tab of the window frame
			// when docked with an other window
			// The resource ID correspond to the one defined in the resx file
			// while the Index is the offset in the bitmap strip. Each image in
			// the strip being 16x16.
			this.BitmapResourceID = 301;
			this.BitmapIndex = 1;

			// This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
			// we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
			// the object returned by the Content property.
			_codeCoverageResultsControl = new CodeCoverageResultsControl();

			base.Content = _codeCoverageResultsControl;
		}

		/// <summary>
		/// This method can be overridden by the derived class to execute any code that must run after the creation of <see cref="T:IVsWindowFrame" />.
		/// </summary>
		public override void OnToolWindowCreated()
		{
			_codeCoverageResultsControl.Package = Package as OpenCoverUIPackage;
		}
	}
}

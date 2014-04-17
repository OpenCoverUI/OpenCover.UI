//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using VSLangProj;
using VSLangProj80;
using OpenCover.UI.Views;
using OpenCover.UI.Helpers;
using EnvDTE;

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// Handles IDE level events
	/// </summary>
	public class VSEventsHandler
	{
		private OpenCoverUIPackage _package;
        private SolutionEvents _solutionEvents;
		/// <summary>
		/// Initializes a new instance of the <see cref="VSEventsHandler"/> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public VSEventsHandler(OpenCoverUIPackage package)
		{
			_package = package;
            _solutionEvents = _package.DTE.Events.SolutionEvents;
            _solutionEvents.Opened += OnSolutionOpened;
            _solutionEvents.AfterClosing += OnSolutionClosing;
		}

		public event Action BuildDone;
		public event Action SolutionOpened;
		public event Action SolutionClosing;

		/// <summary>
		/// Builds the solution.
		/// </summary>
		/// <param name="action">The event handler.</param>
		public void BuildSolution()
		{
			try
			{
				_package.DTE.ExecuteCommand("Build.BuildSolution");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}

		/// <summary>
		/// Called when solution is built.
		/// </summary>
		/// <param name="Scope">The scope.</param>
		/// <param name="Action">The action.</param>
		void OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
		{
			if (BuildDone != null)
			{
				BuildDone();
			}
		}

		/// <summary>
		/// Event handler for Solution Opened Event
		/// </summary>
		private void OnSolutionOpened()
		{
			_package.DTE.Events.BuildEvents.OnBuildDone += OnBuildDone;

			if (SolutionOpened != null)
			{
				SolutionOpened();
			}
		}

		/// <summary>
		/// Event handler for Solution Closing Event.
		/// </summary>
		private void OnSolutionClosing()
		{
			_package.ToolWindows.OfType<CodeCoverageResultsToolWindow>().First().CodeCoverageResultsControl.ClearTreeView();

			if (SolutionClosing != null)
			{
				SolutionClosing();
			}

			_package.DTE.Events.BuildEvents.OnBuildDone -= OnBuildDone;
		}
	}
}

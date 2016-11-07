//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.Linq;
using OpenCover.UI.Views;
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
		private bool _building;
		private bool _buildSuccessful;

		public event Action BuildSucceeded;
		public event Action BuildFailed;
		public event Action SolutionOpened;
		public event Action SolutionClosing;

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
			_buildSuccessful = true;
		}

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
			if (_buildSuccessful)
			{
				if (BuildSucceeded != null)
				{
					BuildSucceeded();
				}
			}
			else
			{
				if (BuildFailed != null)
				{
					BuildFailed();
				}
			}

			_building = false;
			_buildSuccessful = true;
		}

		/// <summary>
		/// Event handler for Solution Opened Event
		/// </summary>
		private void OnSolutionOpened()
		{
			_package.DTE.Events.BuildEvents.OnBuildProjConfigDone += OnBuildProjConfigDone;
			_package.DTE.Events.BuildEvents.OnBuildDone += OnBuildDone;

			if (SolutionOpened != null)
			{
				SolutionOpened();
			}
		}

		private void OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
		{
			if (!_building)
			{
				_building = true;
			}

			if (!success)
			{
				_buildSuccessful = false;
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
			_package.DTE.Events.BuildEvents.OnBuildProjConfigDone -= OnBuildProjConfigDone;
		}
	}
}

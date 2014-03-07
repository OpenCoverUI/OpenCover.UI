//
// This source code is released under the MIT License;
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

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// Handles IDE level events
	/// </summary>
	public class VSEventsHandler
	{
		private OpenCoverUIPackage _package;
		private Dictionary<string, string> _keysDictionary = new Dictionary<string, string>();
		private Dictionary<string, string> _fileList = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="VSEventsHandler"/> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public VSEventsHandler(OpenCoverUIPackage package)
		{
			_package = package;

			_package.DTE.Events.SolutionEvents.Opened += OnSolutionOpened;
			_package.DTE.Events.SolutionEvents.AfterClosing += OnSolutionClosing;
			_package.DTE.Events.BuildEvents.OnBuildDone += OnBuildDone;
		}

		/// <summary>
		/// Gets the files in solution.
		/// </summary>
		/// <value>
		/// The files in solution.
		/// </value>
		public Dictionary<string, string> FilesInSolution
		{
			get
			{
				return _fileList;
			}
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
				_package.DTE.Events.BuildEvents.OnBuildDone += OnBuildDone;
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

			_package.DTE.Events.BuildEvents.OnBuildDone -= OnBuildDone;
		}

		/// <summary>
		/// Event handler for Solution Opened Event
		/// </summary>
		private void OnSolutionOpened()
		{
			Task.Factory.StartNew(() => BuildFilesList());

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
			_keysDictionary.Clear();
			_fileList.Clear();
			_package.ToolWindows.OfType<CodeCoverageResultsToolWindow>().First().CodeCoverageResultsControl.ClearTreeView();

			if (SolutionClosing != null)
			{
				SolutionClosing();
			}
		}

		/// <summary>
		/// Builds the list containing all files present in the solution.
		/// </summary>
		private void BuildFilesList()
		{
			foreach (EnvDTE.Project project in _package.DTE.Solution.Projects)
			{
				this.AddProjectItems(project.ProjectItems);
			}
		}

		/// <summary>
		/// Adds files in current project to the Files list.
		/// </summary>
		/// <param name="projectItem">The project item.</param>
		private void AddProjectItems(EnvDTE.ProjectItems projectItem)
		{
			foreach (EnvDTE.ProjectItem item in projectItem)
			{
				if (item.Kind == EnvDTE.Constants.vsProjectItemKindPhysicalFolder ||
					item.Kind == EnvDTE.Constants.vsProjectItemKindVirtualFolder)
				{
					if (item.ProjectItems != null)
					{
						AddProjectItems(item.ProjectItems);
					}
				}
				else if (item.Kind == EnvDTE.Constants.vsProjectItemKindSolutionItems)
				{
					if (item.SubProject != null && item.SubProject.ProjectItems != null)
					{
						AddProjectItems(item.SubProject.ProjectItems);
					}
				}
				else
				{
					string projectFullPath = null;
					if (_keysDictionary.ContainsKey(item.ContainingProject.Name))
					{
						projectFullPath = _keysDictionary[item.ContainingProject.Name];
					}
					else
					{
						projectFullPath = IDEHelper.GetOutputPath(item.ContainingProject);
						_keysDictionary.Add(item.ContainingProject.Name, projectFullPath);
					}

					if (item.Properties != null && item.Properties.Item("FullPath") != null)
					{
						string fullPath = item.Properties.Item("FullPath").Value.ToString().ToLower();

						if (!_fileList.ContainsKey(fullPath))
						{
							_fileList.Add(fullPath, projectFullPath);
						}
					}
				}
			}

		}
	}
}

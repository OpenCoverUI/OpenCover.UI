//
// This source code is released under the MIT License;
//
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// Handles IDE level events
	/// </summary>
	public class VSEventsHandler
	{
		private OpenCoverUIPackage _package;
		private Dictionary<string, string> _keysDictionary = new Dictionary<string, string>();
		Dictionary<string, string> _fileList = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="VSEventsHandler"/> class.
		/// </summary>
		/// <param name="package">The Visual Studio Extension Package.</param>
		public VSEventsHandler(OpenCoverUIPackage package)
		{
			_package = package;

			_package.DTE.Events.SolutionEvents.Opened += SolutionOpened;
			_package.DTE.Events.SolutionEvents.AfterClosing += SolutionClosing;
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

		/// <summary>
		/// Event handler for Solution Opened Event
		/// </summary>
		private void SolutionOpened()
		{
			BuildFilesList();
		}

		/// <summary>
		/// Event handler for Solution Closing Event.
		/// </summary>
		void SolutionClosing()
		{
			_keysDictionary.Clear();
			_fileList.Clear();
			_package.CodeCoverageResultsControl.ClearTreeView();
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
						projectFullPath = GetOutputPath(item.ContainingProject);
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

		/// <summary>
		/// Returns the output path of the project.
		/// </summary>
		/// <param name="project">The project.</param>
		/// <returns>Output path</returns>
		private static string GetOutputPath(EnvDTE.Project project)
		{
			string outputPath = project.ConfigurationManager != null && project.ConfigurationManager.ActiveConfiguration != null
				? project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString() : null;

			if (outputPath == null)
			{
				return null;
			}

			string absoluteOutputPath;
			string projectFolder;

			if (outputPath.StartsWith(String.Format("{0}{0}", Path.DirectorySeparatorChar)))
			{
				// This is the case 1: "\\server\folder"
				absoluteOutputPath = outputPath;
			}
			else if (outputPath.Length >= 2 && outputPath[1] == Path.VolumeSeparatorChar)
			{
				// This is the case 2: "drive:\folder"
				absoluteOutputPath = outputPath;
			}
			else if (outputPath.IndexOf("..\\") != -1)
			{
				// This is the case 3: "..\..\folder"
				projectFolder = Path.GetDirectoryName(project.FullName);
				while (outputPath.StartsWith("..\\"))
				{
					outputPath = outputPath.Substring(3);
					projectFolder = Path.GetDirectoryName(projectFolder);
				}

				absoluteOutputPath = System.IO.Path.Combine(projectFolder, outputPath);
			}
			else
			{
				// This is the case 4: "folder"
				projectFolder = System.IO.Path.GetDirectoryName(project.FullName);
				absoluteOutputPath = System.IO.Path.Combine(projectFolder, outputPath);
			}

			return Path.Combine(absoluteOutputPath, project.Properties.Item("OutputFileName").Value.ToString());
		}
	}
}

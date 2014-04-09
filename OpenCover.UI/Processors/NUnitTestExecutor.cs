using Microsoft.Win32;
//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
namespace OpenCover.UI.Processors
{
	internal class NUnitTestExecutor : TestExecutor
	{
		private string _runListFile;
		private string _nUnitPath;

		internal NUnitTestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
			: base(package, selectedTests)
		{
			var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			var programFilesDirectoryInfo = new DirectoryInfo(programFiles);
			var nUnitDirectories = programFilesDirectoryInfo.GetDirectories("NUnit*");

			if (nUnitDirectories != null)
			{
				var latestInstalledNUnitDirectory = nUnitDirectories.OrderByDescending(d => d.LastWriteTime).FirstOrDefault();
				if (latestInstalledNUnitDirectory != null)
				{
					_nUnitPath = Path.Combine(latestInstalledNUnitDirectory.FullName, "bin", "nunit-console.exe");

					if (!File.Exists(_nUnitPath))
					{
						_nUnitPath = null;
					}
				}
			}

			if (_nUnitPath == null)
			{
				MessageBox.Show("NUnit not found at its default path. Please select the Nunit executable", Resources.MessageBoxTitle, MessageBoxButton.OK);
				var dialog = new OpenFileDialog();
				dialog.Filter = "Executables (*.exe)|*.exe";

				if (dialog.ShowDialog() == true)
				{
					_nUnitPath = dialog.FileName;
				}
			}
		}

		protected override void SetOpenCoverCommandlineArguments()
		{
			var fileFormat = DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_ms");
			var dllPaths = BuildDLLPath();

			SetOpenCoverResultsFilePath();

			_testResultsFile = Path.Combine(_currentWorkingDirectory.FullName, String.Format("{0}.xml", fileFormat));
			_runListFile = Path.Combine(_currentWorkingDirectory.FullName, String.Format("{0}.txt", fileFormat));

			_commandLineArguments = String.Format(_commandlineStringFormat,
													_nUnitPath,
													String.Format("{0} /runlist=\\\"{1}\\\" /nologo /noshadow /result=\\\"{2}\\\"", dllPaths, _runListFile, _testResultsFile),
													_openCoverResultsFile);

			CreateRunListFile();
		}

		internal override void Cleanup()
		{
			base.Cleanup();

			if (File.Exists(_runListFile))
			{
				File.Delete(_runListFile);
			}
		}

		/// <summary>
		/// Creates the run list file.
		/// </summary>
		private void CreateRunListFile()
		{
			using (var file = File.OpenWrite(_runListFile))
			{
				using (var writer = new StreamWriter(file))
				{
					foreach (var test in _selectedTests.Item2)
					{
						writer.WriteLine(test);
					}
				}
			}
		}
	}
}

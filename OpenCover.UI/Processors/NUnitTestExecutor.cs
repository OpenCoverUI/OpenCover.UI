using Microsoft.Win32;
using OpenCover.UI.Model;
//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using OpenCover.UI.Helpers;

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// NUnit Test Executor
	/// </summary>
	internal class NUnitTestExecutor : TestExecutor
	{
		private string _runListFile;
		private string _nUnitPath;
		private Dictionary<string, IEnumerable<TestResult>> _executionStatus;

		/// <summary>
		/// Initializes a new instance of the <see cref="NUnitTestExecutor"/> class.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="selectedTests">The selected tests.</param>
		internal NUnitTestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
			: base(package, selectedTests)
		{
			SetNUnitPath();

			_executionStatus = new Dictionary<string, IEnumerable<TestResult>>();
		}

		/// <summary>
		/// Sets the OpenCover commandline arguments.
		/// </summary>
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

		/// <summary>
		/// Reads the test results file.
		/// </summary>
		protected override void ReadTestResults()
		{
			var testResultsFile = XDocument.Load(_testResultsFile);

			_executionStatus.Clear();

			var assemblies = testResultsFile.Descendants("test-suite").Where(ts => ts.Attribute("type") != null && ts.Attribute("type").Value == "Assembly");

			foreach (var assembly in assemblies)
			{
				var testMethods = assembly.Descendants("test-case").Select(tc =>
									{
										var failure = tc.Element("failure");
										var errorMessage = GetElementValue(failure, "message");
										var stackTrace = GetElementValue(failure, "stack-trace");

										return new TestResult(tc.Attribute("name").Value,
																GetTestExecutionStatus(tc.Attribute("result").Value),
																Decimal.Parse(tc.Attribute("time").Value),
																errorMessage,
																stackTrace);
									});

				_executionStatus.Add(assembly.Attribute("name").Value, testMethods);
			}
		}

		/// <summary>
		/// Updates the test methods execution.
		/// </summary>
		internal override void UpdateTestMethodsExecution(IEnumerable<TestClass> tests)
		{
			var execution = _executionStatus.SelectMany(t => t.Value.Select(tm => new { dll = t.Key, result = tm }));

			var executedTests = tests.SelectMany(t => t.TestMethods)
										.Join(execution,
												t => new { d = t.Class.DLLPath, n = t.FullyQualifiedName },
												t => new { d = t.dll, n = t.result.MethodName },
												(testMethod, result) => new { TestMethod = testMethod, Result = result });

			foreach (var test in executedTests)
			{
				test.TestMethod.ExecutionStatus = test.Result.result.Status;
			}
		}

		/// <summary>
		/// Delete temporary files created.
		/// </summary>
		internal override void Cleanup()
		{
			base.Cleanup();

			if (File.Exists(_runListFile))
			{
				File.Delete(_runListFile);
			}
		}

		/// <summary>
		/// Sets the NUnit path.
		/// </summary>
		private void SetNUnitPath()
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

		/// <summary>
		/// Gets the test execution status enum.
		/// </summary>
		/// <param name="status">The status.</param>
		private TestExecutionStatus GetTestExecutionStatus(string status)
		{
			switch (status.ToLower())
			{
				case "success":
					return TestExecutionStatus.Successful;
				case "failure":
					return TestExecutionStatus.Error;
				case "inconclusive":
					return TestExecutionStatus.Inconclusive;
				default:
					return TestExecutionStatus.NotRun;
			}
		}

		/// <summary>
		/// Gets the element's value.
		/// </summary>
		/// <param name="element">The element.</param>
		private string GetElementValue(XElement element, string attribute)
		{
			if (element != null)
			{
				var xAttribute = element.Attribute(attribute);
				if (xAttribute != null)
				{
					return xAttribute.Value;
				}
			}

			return null;
		}
	}
}

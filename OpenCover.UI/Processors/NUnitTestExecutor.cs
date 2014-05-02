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
			try
			{
				if (File.Exists(_testResultsFile))
				{
					var testResultsFile = XDocument.Load(_testResultsFile);

					_executionStatus.Clear();

					var assemblies = GetElementsByAttribute(testResultsFile, "test-suite", "type", "Assembly");

					foreach (var assembly in assemblies)
					{
						decimal tempTime = 0;

						var testCases = GetElementsByAttribute(assembly, "test-suite", "type", "TestFixture");

						var testMethods = testCases.Elements("results").Elements("test-case").Select(tc =>
											{
												var failure = tc.Element("failure");
												var errorMessage = GetElementValue(failure, "message");
												var stackTrace = GetElementValue(failure, "stack-trace");

												return new TestResult(GetAttributeValue(tc, "name"),
																		GetTestExecutionStatus(GetAttributeValue(tc, "result")),
																		Decimal.TryParse(GetAttributeValue(tc, "time"), out tempTime) ? tempTime : 0,
																		errorMessage,
																		stackTrace,
																		null);
											});

						testMethods = testMethods.Union(testCases.Elements("results").Elements("test-suite").Select(ts =>
						{
							var testCasesInTestSuite = ts.Element("results").Elements("test-case");
							var testResults = new List<TestResult>();

							foreach (var testCase in testCasesInTestSuite)
							{
								testResults.Add(GetTestResult(testCase, null)); 
							}

							var testResult = GetTestResult(ts, testResults);
							if (testResults.Any())
							{
								var testCaseName = testResults.First();
								testResult.MethodName = testCaseName.MethodName.Substring(0, testCaseName.MethodName.IndexOf("("));
							}

							return testResult;
						}));

						_executionStatus.Add(assembly.Attribute("name").Value, testMethods);
					}
				}
				else
				{
					IDEHelper.WriteToOutputWindow("Test Results File does not exist: {0}", _testResultsFile);
				}
			}
			catch (Exception ex)
			{
				IDEHelper.WriteToOutputWindow(ex.Message);
				IDEHelper.WriteToOutputWindow(ex.StackTrace);
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

		private TestResult GetTestResult(XElement element, List<TestResult> testCases)
		{
			var failure = element.Element("failure");
			decimal tempTime = -1;

			return new TestResult(GetAttributeValue(element, "name"),
											GetTestExecutionStatus(GetAttributeValue(element, "result")),
											Decimal.TryParse(GetAttributeValue(element, "time"), out tempTime) ? tempTime : 0,
											GetElementValue(failure, "message"),
											GetElementValue(failure, "stack-trace"),
											testCases);
		}

		/// <summary>
		/// Gets the element's value.
		/// </summary>
		/// <param name="element">The element.</param>
		private string GetAttributeValue(XElement element, string attribute)
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

		/// <summary>
		/// Gets the element's value.
		/// </summary>
		/// <param name="element">The element.</param>
		private string GetElementValue(XElement element, string childElement)
		{
			// TODO: Refactor code to remove the duplicated methods - GetElementValue and GetAttributeValue. 
			// The only difference in these methods is accessing Element/Attribute methods.
			if (element != null)
			{
				var child = element.Element(childElement);
				if (child != null)
				{
					return child.Value;
				}
			}

			return null;
		}

		private IEnumerable<XElement> GetElementsByAttribute<T>(T parent, string elementName, string attributeName, string attributeValue)
			where T : XContainer
		{
			return parent.Descendants(elementName).Where(ts => ts.Attribute(attributeName) != null && ts.Attribute(attributeName).Value == attributeValue);
		}
	}
}

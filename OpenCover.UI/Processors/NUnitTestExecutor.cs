//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using EnvDTE;
using Microsoft.Win32;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using OpenCover.UI.Model.Test;

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// NUnit Test Executor
	/// </summary>
	internal class NUnitTestExecutor : TestExecutor
	{
		private string _runListFile;
		private string _nUnitPath;

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

			if (Path.GetFileName(_nUnitPath) == "nunit3-console.exe")
			{
				_commandLineArguments = String.Format(CommandlineStringFormat,
										_nUnitPath,
										String.Format("{0} --testlist=\\\"{1}\\\" --noheader --result=\\\"{2}\\\";format=nunit2", dllPaths, _runListFile, _testResultsFile),
										_openCoverResultsFile);
			}
			else
			{
				_commandLineArguments = String.Format(CommandlineStringFormat,
										_nUnitPath,
										String.Format("{0} /runlist=\\\"{1}\\\" /nologo /noshadow /result=\\\"{2}\\\"", dllPaths, _runListFile, _testResultsFile),
										_openCoverResultsFile);
			}

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
						var testCases = GetElementsByAttribute(assembly, "test-suite", "type", "TestFixture");
						var testMethods = testCases.Elements("results").Elements("test-case").Select(tc => GetTestResult(tc, null));
						testMethods = testMethods.Union(testCases.Elements("results").Elements("test-suite").Select(ts => ReadTestCase(ts)));

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
				test.TestMethod.ExecutionResult = test.Result.result;
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

	   private IEnumerable<DirectoryInfo> ProgramFilesFolders()
	   {
	      var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
	      if (Environment.Is64BitOperatingSystem)
	      {
	         if (path.EndsWith(" (x86)"))
	         {
	            yield return new DirectoryInfo(path.Replace(" (x86)", ""));
	         }
	         else
	         {
	            yield return new DirectoryInfo(path + " (x86)");
	         }
	      }
         yield return new DirectoryInfo(path);
	   }

		/// <summary>
		/// Sets the NUnit path.
		/// </summary>
		private void SetNUnitPath()
		{
		   _nUnitPath = OpenCoverUISettings.Default.NUnitPath;
		   if (!File.Exists(_nUnitPath))
		   {
                var consoleName = "nunit-console.exe";
               if (Environment.Is64BitOperatingSystem && _selectedTests != null && _selectedTests.Item3 != null && _selectedTests.Item3.Any())
               {
                   // check whether any test assembly was build as x86.
                   if (IsX86Build(_selectedTests.Item3.First()))
                       consoleName = "nunit-console-x86.exe";
               }

               var nunits =
		         from programDir in ProgramFilesFolders()
		         from nunitDir in programDir.GetDirectories("NUnit*")
		         orderby nunitDir.LastWriteTime descending
                 let nunitPath = Path.Combine(nunitDir.FullName, "bin", consoleName)
		         where File.Exists(nunitPath)
		         select nunitPath;

		      _nUnitPath = nunits.FirstOrDefault();               
		         
		      if (_nUnitPath == null)
		      {
		         MessageBox.Show("NUnit not found at its default path. Please select the Nunit executable",
		            Resources.MessageBoxTitle, MessageBoxButton.OK);
		         var dialog = new OpenFileDialog {Filter = "Executables (*.exe)|*.exe"};
		         if (dialog.ShowDialog() == true)
		         {
		            _nUnitPath = dialog.FileName;
		            OpenCoverUISettings.Default.NUnitPath = _nUnitPath;
		            OpenCoverUISettings.Default.Save();
		         }
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
		/// Reads the test case.
		/// </summary>
		/// <param name="ts">The test-suite element.</param>
		private TestResult ReadTestCase(XElement ts)
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
                var bracketPositionInUnnamedTestCase = testCaseName.MethodName.IndexOf("(");

                if(bracketPositionInUnnamedTestCase > 0)
                    testResult.MethodName = testCaseName.MethodName.Substring(0, bracketPositionInUnnamedTestCase);
            }

			return testResult;
		}

		/// <summary>
		/// Gets the test result.
		/// </summary>
		/// <param name="element">The element.</param>
		/// <param name="testCases">The test cases.</param>
		private TestResult GetTestResult(XElement element, List<TestResult> testCases)
		{
			var failure = element.Element("failure");
			decimal tempTime = -1;

			return new TestResult(GetAttributeValue(element, "name"),
								  GetTestExecutionStatus(GetAttributeValue(element, "result")),
								  Decimal.TryParse(GetAttributeValue(element, "time"), out tempTime) ? tempTime : 0,
								  GetElementValue(failure, "message", XNamespace.None),
								  GetElementValue(failure, "stack-trace", XNamespace.None),
								  testCases);
		}

		/// <summary>
		/// Gets the elements by attribute.
		/// </summary>
		/// <typeparam name="T">XContainer derivative</typeparam>
		/// <param name="parent">The parent.</param>
		/// <param name="elementName">Name of the element.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="attributeValue">The attribute value.</param>
		/// <returns></returns>
		private IEnumerable<XElement> GetElementsByAttribute<T>(T parent, string elementName, string attributeName, string attributeValue)
			where T : XContainer
		{
			return parent.Descendants(elementName).Where(ts => ts.Attribute(attributeName) != null && ts.Attribute(attributeName).Value == attributeValue);
		}

        /// <summary>
        /// Determines whether the given assembly was build for x86 platform.
        /// </summary>
        /// <param name="assemblyFileName">Path to the assembly to check.</param>
        /// <returns></returns>
        private bool IsX86Build(string assemblyFileName)
        {
            var assemblyInfo = System.Reflection.AssemblyName.GetAssemblyName(assemblyFileName);               
            
            return assemblyInfo.ProcessorArchitecture == System.Reflection.ProcessorArchitecture.X86;
        }
	}
}

//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// Executes the tests with code coverage using OpenCover
	/// </summary>
	internal class TestExecutor
	{
		private readonly string _openCoverPath;
		private readonly string _vsTestPath;

		private Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> _selectedTests;
		private string _openCoverResultsFile;
		private string _testResultsFile;
		private OpenCoverUIPackage _package;
		private TestMethodGroupingField _groupingField;
		private string _commandLineArguments;
		private DirectoryInfo _currentWorkingDirectory;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestExecutor"/> class.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="selectedTests">The selected tests.</param>
		public TestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests, TestMethodGroupingField groupingField)
		{
			_package = package;
			_selectedTests = selectedTests;
			_groupingField = groupingField;

			_openCoverPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
												@"Apps\OpenCover\OpenCover.Console.exe");
			_vsTestPath = Path.Combine(Path.GetDirectoryName(_package.DTE.FullName), @"CommonExtensions\Microsoft\TestWindow\vstest.console.exe");
		}

		internal bool ValidateCommandLineArgumentsLength()
		{
			SetOpenCoverCommandlineArguments();

			if (_commandLineArguments.Length > 32767)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Starts OpenCover.Console.exe to start CodeCoverage session.
		/// </summary>
		/// <returns>Test results (trx) and OpenCover results files' paths</returns>
		public Tuple<string, string> Execute()
		{
			var openCoverStartInfo = GetOpenCoverProcessInfo(_commandLineArguments);

			if (!System.IO.File.Exists(openCoverStartInfo.FileName))
			{
				MessageBox.Show("Please install OpenCover and execute tests!", "OpenCover not found!", MessageBoxButton.OK);
				return null;
			}

			Process process = Process.Start(openCoverStartInfo);

			var consoleOutputReaderBuilder = new StringBuilder();

			// TODO: See if this loop has any performance bottlenecks
			while (true)
			{
				if (process.HasExited)
				{
					break;
				}

				string nextLine = process.StandardOutput.ReadLine();
				if (!String.IsNullOrWhiteSpace(nextLine) && nextLine.StartsWith("Results File:"))
				{
					_testResultsFile = nextLine.Replace("Results File: ", "");
				}

				IDEHelper.WriteToOutputWindow(nextLine);
				consoleOutputReaderBuilder.AppendLine(nextLine);
			}

			process.WaitForExit();

			IDEHelper.WriteToOutputWindow(process.StandardError.ReadToEnd());

			IDEHelper.OpenFile(_package.DTE, _testResultsFile);

			return new Tuple<string, string>(_testResultsFile, _openCoverResultsFile);
		}

		/// <summary>
		/// Deserializes the results.xml file to CoverageSession
		/// </summary>
		/// <returns>OpenCover execution results</returns>
		public CoverageSession GetExecutionResults()
		{
			CoverageSession coverageSession = null;

			try
			{
				var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(OpenCover.Framework.Model.File), typeof(Class) });
				using (var stream = System.IO.File.Open(this._openCoverResultsFile, FileMode.Open))
				{
					coverageSession = serializer.Deserialize(stream) as CoverageSession;
				}

				System.IO.File.Delete(this._openCoverResultsFile);

			}
			catch (Exception ex)
			{
				IDEHelper.WriteToOutputWindow(ex.Message);
				IDEHelper.WriteToOutputWindow(ex.StackTrace);
			}


			return coverageSession;
		}

		/// <summary>
		/// Returns start information to launch OpenCover.Console.exe
		/// </summary>
		/// <returns>Open Cover process start information</returns>
		private ProcessStartInfo GetOpenCoverProcessInfo(string arguments)
		{
			var openCoverStartInfo = new ProcessStartInfo(this._openCoverPath, arguments)
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = _currentWorkingDirectory.FullName
			};

			IDEHelper.WriteToOutputWindow(openCoverStartInfo.Arguments);

			return openCoverStartInfo;
		}

		private void SetOpenCoverCommandlineArguments()
		{
			var builder = new StringBuilder();

			foreach (var testDLL in _selectedTests.Item3)
			{
				builder.AppendFormat("\\\"{0}\\\" ", testDLL);
			}

			string dllPaths = builder.ToString();

			builder.Length = 0;

			switch (_groupingField)
			{
				case TestMethodGroupingField.Trait:
					{
						builder.Append("/TestCaseFilter:\\\"");

						BuildTestCategoryCommandlineString(builder, "TestCategory", _selectedTests.Item1);

						BuildTestCategoryCommandlineString(builder, "Name", _selectedTests.Item2);

						DeleteLastCharacter(builder);

						builder.Append("\\\"");
						break;
					}
				case TestMethodGroupingField.Class:
					{
						BuildTestsCommandlineStringForClassAndProjectTypes(_selectedTests.Item1.Union(_selectedTests.Item2), builder);
						break;
					}
				case TestMethodGroupingField.Project:
					{
						BuildTestsCommandlineStringForClassAndProjectTypes(_selectedTests.Item2, builder);
						break;
					}
			}

			SetOpenCoverResultsFilePath();

			_commandLineArguments = String.Format("-target:\"{0}\" -targetargs:\"{1}{2} /Logger:trx\" -output:\"{3}\" -hideskipped:All -register:user -excludebyattribute:*.ExcludeFromCodeCoverage*",
										this._vsTestPath,
										dllPaths,
										builder.ToString(),
										_openCoverResultsFile);
		}

		private void BuildTestCategoryCommandlineString(StringBuilder builder, string caption, IEnumerable<string> selection)
		{
			foreach (var member in selection)
			{
				builder.AppendFormat("{0}={1}|", caption, member);
			}
		}

		private static void BuildTestsCommandlineStringForClassAndProjectTypes(IEnumerable<string> tests, StringBuilder builder)
		{
			if (tests.Any())
			{
				builder.Append("/Tests:");
				builder.Append(String.Join(",", tests));
			}
		}

		private static void DeleteLastCharacter(StringBuilder builder)
		{
			if (builder.Length > 0)
			{
				builder.Length = builder.Length - 1;
			}
		}

		private void SetOpenCoverResultsFilePath()
		{
			var solution = _package.DTE.Solution as EnvDTE.SolutionClass;

			// Create a working directory
			_currentWorkingDirectory = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(solution.FileName), "OpenCover"));

			_openCoverResultsFile = Path.Combine(_currentWorkingDirectory.FullName, String.Format("{0}.xml", Guid.NewGuid()));
		}
	}
}

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
	internal abstract class TestExecutor
	{
		protected const string _commandlineStringFormat = "-target:\"{0}\" -targetargs:\"{1}\" -output:\"{2}\" -hideskipped:All -register:user -excludebyattribute:*.ExcludeFromCodeCoverage*";
		protected readonly string _openCoverPath;
		
		protected string _openCoverResultsFile;
		protected string _testResultsFile;
		protected string _commandLineArguments;

		protected Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> _selectedTests;
		protected OpenCoverUIPackage _package;
		protected DirectoryInfo _currentWorkingDirectory;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestExecutor"/> class.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="selectedTests">The selected tests.</param>
		internal TestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
		{
			_package = package;
			_selectedTests = selectedTests;

			_openCoverPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
												@"Apps\OpenCover\OpenCover.Console.exe");
		}

		/// <summary>
		/// Validates the length of the command line arguments.
		/// </summary>
		/// <returns>Validation result</returns>
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
		/// Sets the OpenCover commandline arguments.
		/// </summary>
		protected abstract void SetOpenCoverCommandlineArguments();

		/// <summary>
		/// Reads the test results.
		/// </summary>
		protected abstract void ReadTestResults();

		/// <summary>
		/// Updates the test methods execution.
		/// </summary>
		internal abstract void UpdateTestMethodsExecution(IEnumerable<TestClass> tests);

		/// <summary>
		/// Do cleanup here.
		/// </summary>
		internal virtual void Cleanup()
		{ 
		}

		/// <summary>
		/// Starts OpenCover.Console.exe to start CodeCoverage session.
		/// </summary>
		/// <returns>Test results (trx) and OpenCover results files' paths</returns>
		internal Tuple<string, string> Execute()
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

			ReadTestResults();

			return new Tuple<string, string>(_testResultsFile, _openCoverResultsFile);
		}

		/// <summary>
		/// Deserializes the results.xml file to CoverageSession
		/// </summary>
		/// <returns>OpenCover execution results</returns>
		internal CoverageSession GetExecutionResults()
		{
			CoverageSession coverageSession = null;

			try
			{
				var serializer = new XmlSerializer(typeof(CoverageSession), new[] { typeof(Module), typeof(OpenCover.Framework.Model.File), typeof(Class) });
				using (var stream = System.IO.File.Open(_openCoverResultsFile, FileMode.Open))
				{
					coverageSession = serializer.Deserialize(stream) as CoverageSession;
				}

				System.IO.File.Delete(_openCoverResultsFile);
			}
			catch (Exception ex)
			{
				IDEHelper.WriteToOutputWindow(ex.Message);
				IDEHelper.WriteToOutputWindow(ex.StackTrace);
			}

			return coverageSession;
		}

		/// <summary>
		/// Builds the DLL path.
		/// </summary>
		protected string BuildDLLPath()
		{
			var builder = new StringBuilder();
			foreach (var testDLL in _selectedTests.Item3)
			{
				builder.AppendFormat("\\\"{0}\\\" ", testDLL);
			}

			return builder.ToString().Trim();
		}

		/// <summary>
		/// Sets the OpenCover results file path.
		/// </summary>
		protected void SetOpenCoverResultsFilePath()
		{
			var solution = _package.DTE.Solution as EnvDTE.SolutionClass;

			// Create a working directory
			_currentWorkingDirectory = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(solution.FileName), "OpenCover"));

			_openCoverResultsFile = Path.Combine(_currentWorkingDirectory.FullName, String.Format("{0}.xml", Guid.NewGuid()));
		}

		/// <summary>
		/// Returns start information to launch OpenCover.Console.exe
		/// </summary>
		/// <returns>Open Cover process start information</returns>
		private ProcessStartInfo GetOpenCoverProcessInfo(string arguments)
		{
			var openCoverStartInfo = new ProcessStartInfo(_openCoverPath, arguments)
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
	}
}

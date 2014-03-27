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
		private readonly IEnumerable<TestMethod> _selectedTests;

		private string _openCoverResultsFile;
		private string _testResultsFile;
		private OpenCoverUIPackage _package;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestExecutor"/> class.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="selectedTests">The selected tests.</param>
		public TestExecutor(OpenCoverUIPackage package, IEnumerable<TestMethod> selectedTests)
		{
			_package = package;
			_selectedTests = selectedTests;
			_openCoverPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
												@"Apps\OpenCover\OpenCover.Console.exe");
			_vsTestPath = Path.Combine(Path.GetDirectoryName(_package.DTE.FullName), @"CommonExtensions\Microsoft\TestWindow\vstest.console.exe");
		}

		/// <summary>
		/// Starts OpenCover.Console.exe to start CodeCoverage session.
		/// </summary>
		/// <returns>Test results (trx) and OpenCover results files' paths</returns>
		public Tuple<string, string> Execute()
		{
			var openCoverStartInfo = GetOpenCoverProcessInfo();
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
		private ProcessStartInfo GetOpenCoverProcessInfo()
		{
			var selectedUnitTests = String.Join(",", _selectedTests.Select(t => t.FullyQualifiedName));
			var builder = new StringBuilder();

			foreach (var testDLL in _selectedTests.Select(t => t.Class.DLLPath).Distinct())
			{
				builder.AppendFormat("\\\"{0}\\\" ", testDLL);
			}

			var solution = _package.DTE.Solution as EnvDTE.SolutionClass;
			
			// Create a working directory
			var directory = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(solution.FileName), "OpenCover"));

			_openCoverResultsFile = Path.Combine(directory.FullName, String.Format("{0}.xml", Guid.NewGuid()));

			var openCoverStartInfo = new ProcessStartInfo(this._openCoverPath,
											String.Format("-target:\"{0}\" -targetargs:\"{1}/Tests:{2} /Logger:trx\" -output:\"{3}\" -hideskipped:All -register:user",
															this._vsTestPath,
															builder.ToString(),
															selectedUnitTests,
															_openCoverResultsFile))
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				WorkingDirectory = directory.FullName
			};

			IDEHelper.WriteToOutputWindow(openCoverStartInfo.Arguments);

			return openCoverStartInfo;
		}
	}
}

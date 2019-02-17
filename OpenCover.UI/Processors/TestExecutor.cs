//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using OpenCover.UI.Model.Test;
using Microsoft.Win32;

namespace OpenCover.UI.Processors
{
    /// <summary>
    /// Executes the tests with code coverage using OpenCover
    /// </summary>
    internal abstract class TestExecutor
    {
        private const string _fixedCommandLineParameters = "-target:\"{0}\" -targetargs:\"{1}\" -output:\"{2}\"";

        private const string _defaultCustomizableCommandLineParameters =
            "-hideskipped:All -register:user";

        private readonly ConfigurationReader _commandLineParameterReader = new ConfigurationReader();

        /// <summary>
        /// Gets the commandline string format.
        /// </summary>
        /// <value>
        /// The commandline string format.
        /// </value>
        protected string CommandlineStringFormat
        {
            get
            {
                var customizableParameters = _defaultCustomizableCommandLineParameters;
                if (_commandLineParameterReader.ReadConfiguration(_currentWorkingDirectory))
                {
                    customizableParameters = String.Join(" ", _commandLineParameterReader.Parameters);
                }

                return String.Format("{0} {1}", _fixedCommandLineParameters, customizableParameters);
            }
        }

        protected string _openCoverPath;

        protected string _openCoverResultsFile;
        protected string _testResultsFile;
        protected string _commandLineArguments;

        protected Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> _selectedTests;
        protected OpenCoverUIPackage _package;
        protected DirectoryInfo _currentWorkingDirectory;
        protected DirectoryInfo _currentTestWorkingDirectory;
        protected Dictionary<string, IEnumerable<TestResult>> _executionStatus;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestExecutor"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="selectedTests">The selected tests.</param>
        internal TestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
        {
            _package = package;
            _selectedTests = selectedTests;

            SetOpenCoverPath();

            _executionStatus = new Dictionary<string, IEnumerable<TestResult>>();
        }

        /// <summary>
        /// Sets the OpenCover path.
        /// </summary>
        private void SetOpenCoverPath()
        {
            _openCoverPath = OpenCoverUISettings.Default.OpenCoverPath;
            if (!System.IO.File.Exists(_openCoverPath))
            {
                MessageBox.Show("OpenCover not found. Please select the OpenCover executable",
                    Resources.MessageBoxTitle, MessageBoxButton.OK);
                var dialog = new OpenFileDialog { Filter = "Executables (*.exe)|*.exe" };
                if (dialog.ShowDialog() == true)
                {
                    _openCoverPath = dialog.FileName;
                    OpenCoverUISettings.Default.OpenCoverPath = _openCoverPath;
                    OpenCoverUISettings.Default.Save();
                }
            }
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
        { }

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

            IsExecuting = true;

            var consoleOutputReaderBuilder = new StringBuilder();

            // TODO: See if this loop has any performance bottlenecks
            while (true)
            {
                if (process.HasExited)
                {
                    break;
                }

                string nextLine = process.StandardOutput.ReadLine();
                IDEHelper.WriteToOutputWindow(nextLine);
                consoleOutputReaderBuilder.AppendLine(nextLine);
            }

            process.WaitForExit();

            IDEHelper.WriteToOutputWindow(process.StandardError.ReadToEnd());

            ReadTestResults();

            IsExecuting = false;

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

                if (_commandLineParameterReader.ReadConfiguration(_currentWorkingDirectory))
                {
                    ExecuteTestResultPostProcessor(_testResultsFile);
                    ExecuteCoverageResultPostProcessor(_openCoverResultsFile);
                }

                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    System.IO.File.Delete(_openCoverResultsFile);
                }
            }
            catch (Exception ex)
            {
                IDEHelper.WriteToOutputWindow(ex.Message);
                IDEHelper.WriteToOutputWindow(ex.StackTrace);
            }

            return coverageSession;
        }

        /// <summary>
        /// Executes the test result post processor.
        /// </summary>
        /// <param name="testResultsFile">The test results file.</param>
        private void ExecuteTestResultPostProcessor(string testResultsFile)
        {
            var command = _commandLineParameterReader.TestResultPostProcessorCommand;
            ExecutePostProcessor(command, testResultsFile);
        }

        /// <summary>
        /// Executes the coverage result post processor.
        /// </summary>
        /// <param name="testCoverageResultsFile">The test coverage results file.</param>
        private void ExecuteCoverageResultPostProcessor(string testCoverageResultsFile)
        {
            var command = _commandLineParameterReader.CoverageResultPostProcessorCommand;
            ExecutePostProcessor(command, testCoverageResultsFile);
        }

        /// <summary>
        /// Executes the post processor.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="resultsFile">The results file.</param>
        private void ExecutePostProcessor(string command, string resultsFile)
        {
            var normalizedCommand = Path.Combine(_currentWorkingDirectory.FullName, command);
            if (System.IO.File.Exists(normalizedCommand) && System.IO.File.Exists(resultsFile))
            {
                if (!String.IsNullOrWhiteSpace(String.Format("cmd /C {0}", normalizedCommand)))
                {
                    var postProcessorInfo = new ProcessStartInfo(normalizedCommand, resultsFile)
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = _currentWorkingDirectory.FullName
                    };

                    IDEHelper.WriteToOutputWindow("{0} {1}", command, postProcessorInfo.Arguments);

                    Process process = Process.Start(postProcessorInfo);
                    Debug.Assert(process != null, "process != null");
                    while (!process.HasExited)
                    {
                        var nextLine = process.StandardOutput.ReadLine();
                        IDEHelper.WriteToOutputWindow(nextLine);
                    }

                    process.WaitForExit();

                    IDEHelper.WriteToOutputWindow(process.StandardError.ReadToEnd());
                }
            }
            else
            {
                IDEHelper.WriteToOutputWindow("Cannot find '{0}', when executing on {1}", normalizedCommand, resultsFile);
            }
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
            _currentTestWorkingDirectory = Directory.CreateDirectory(Path.Combine(_currentWorkingDirectory.FullName, Guid.NewGuid().ToString()));

            _openCoverResultsFile = Path.Combine(_currentTestWorkingDirectory.FullName, String.Format("{0}.xml", Guid.NewGuid()));
        }

        /// <summary>
        /// Gets the element's value.
        /// </summary>
        /// <param name="element">The element.</param>
        protected string GetAttributeValue(XElement element, string attribute)
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
        protected string GetElementValue(XElement element, string childElement, XNamespace ns)
        {
            // TODO: Refactor code to remove the duplicated methods - GetElementValue and GetAttributeValue. 
            // The only difference in these methods is accessing Element/Attribute methods.
            if (element != null)
            {
                var child = element.Element(ns + childElement);
                if (child != null)
                {
                    return child.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the test execution status enum.
        /// </summary>
        /// <param name="status">The status.</param>
        protected TestExecutionStatus GetTestExecutionStatus(string status)
        {
            switch (status.ToLower())
            {
                case "success":
                case "passed":
                case "pass":
                    return TestExecutionStatus.Successful;
                case "failure":
                case "failed":
                case "fail":
                    return TestExecutionStatus.Error;
                case "inconclusive":
                    return TestExecutionStatus.Inconclusive;
                default:
                    return TestExecutionStatus.NotRun;
            }
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
                WorkingDirectory = _currentTestWorkingDirectory.FullName
            };

            IDEHelper.WriteToOutputWindow(openCoverStartInfo.Arguments);

            return openCoverStartInfo;
        }

        internal bool IsExecuting { get; private set; }
    }
}

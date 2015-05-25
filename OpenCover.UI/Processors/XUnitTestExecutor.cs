using Microsoft.Win32;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace OpenCover.UI.Processors
{
    internal class XUnitTestExecutor : TestExecutor
    {
        private string _runListFile;
        private string _xUnitPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="XUnitTestExecutor"/> class.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <param name="selectedTests">The selected tests.</param>
        internal XUnitTestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
            : base(package, selectedTests)
        {
            SetXUnitPath();

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

            _commandLineArguments = String.Format(CommandlineStringFormat,
                                                    _xUnitPath,
                                                    String.Format("{0} -nologo -noshadow -xml {1}", dllPaths, _testResultsFile),
                                                    _openCoverResultsFile);            
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

                    var rootAssemblies = testResultsFile.Elements("assemblies");
                    var assemblies = rootAssemblies.Elements().Where(e => e.Name == "assembly");

                    foreach (var assembly in assemblies)
                    {
                        var testCases = assembly.Elements("collection");
                        var testMethods = testCases.Elements("test").Select(tc => GetTestResult(tc, null));                        
                        _executionStatus.Add(assembly.Attribute("name").Value.ToLower(), testMethods);
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
                                                t => new { d = t.Class.DLLPath.ToUpperInvariant(), n = t.FullyQualifiedName },
                                                t => new { d = t.dll.ToUpperInvariant(), n = t.result.MethodName },
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
        /// Sets the XUnit path.
        /// </summary>
        private void SetXUnitPath()
        {
            _xUnitPath = OpenCoverUISettings.Default.XUnitPath;
            if (!File.Exists(_xUnitPath))
            {
                var consoleName = "xunit.console.exe";
                if (Environment.Is64BitOperatingSystem && _selectedTests != null && _selectedTests.Item3 != null && _selectedTests.Item3.Any())
                {
                    // check whether any test assembly was build as x86.
                    if (IsX86Build(_selectedTests.Item3.First()))
                        consoleName = "xunit.console.x86.exe";
                }

                var xunits =
                  from programDir in ProgramFilesFolders()
                  from xunitDir in programDir.GetDirectories("XUnit*")
                  orderby xunitDir.LastWriteTime descending
                  let xunitPath = Path.Combine(xunitDir.FullName, consoleName)
                  where File.Exists(xunitPath)
                  select xunitPath;

                _xUnitPath = xunits.FirstOrDefault();

                if (_xUnitPath == null)
                {
                    MessageBox.Show("XUnit not found at its default path. Please select the Xunit executable",
                       Resources.MessageBoxTitle, MessageBoxButton.OK);
                    var dialog = new OpenFileDialog { Filter = "Executables (*.exe)|*.exe" };
                    if (dialog.ShowDialog() == true)
                    {
                        _xUnitPath = dialog.FileName;
                        OpenCoverUISettings.Default.XUnitPath = _xUnitPath;
                        OpenCoverUISettings.Default.Save();
                    }
                }
            }
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

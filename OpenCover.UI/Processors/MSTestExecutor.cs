//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Commands;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenCover.UI.Processors
{
	internal class MSTestExecutor : TestExecutor
	{
		protected TestMethodGroupingField _groupingField;
		private string _vsTestPath;
		private IEnumerable<Tuple<string, TestResult>> _execution;

		internal MSTestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
			: base(package, selectedTests)
		{
			_vsTestPath = Path.Combine(Path.GetDirectoryName(_package.DTE.FullName), @"CommonExtensions\Microsoft\TestWindow\vstest.console.exe");
		}

		protected override void SetOpenCoverCommandlineArguments()
		{
			string dllPaths = BuildDLLPath();
			var builder = new StringBuilder();

			switch (TestsExplorerToolbarCommands.CurrentSelectedGroupBy)
			{
				case TestMethodGroupingField.Trait:
					{
						BuildTargetArgsForGroupByTrait(builder);
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

			string testSettingsFile = _package.Commands.OfType<MSTestSettingsFileSelectorCommand>().FirstOrDefault().SelectedFile;

			_commandLineArguments = String.Format(CommandlineStringFormat,
										_vsTestPath,
										String.Format("{0} {1} /Logger:trx{2}", dllPaths, builder.ToString(), String.IsNullOrWhiteSpace(testSettingsFile) ? String.Empty : String.Format(" /Settings:\\\"{0}\\\"", testSettingsFile)),
										_openCoverResultsFile);
		}

		private void BuildTargetArgsForGroupByTrait(StringBuilder builder)
		{
			builder.Append("/TestCaseFilter:\\\"");

			BuildTestCategoryCommandlineString(builder, "TestCategory", _selectedTests.Item1);

			BuildTestCategoryCommandlineString(builder, "FullyQualifiedName", _selectedTests.Item2);

			DeleteLastCharacter(builder);

			builder.Append("\\\"");
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

		/// <summary>
		/// Deletes the last character from string builder.
		/// </summary>
		/// <param name="builder">The string builder.</param>
		private static void DeleteLastCharacter(StringBuilder builder)
		{
			if (builder.Length > 0)
			{
				builder.Length = builder.Length - 1;
			}
		}

		protected override void ReadTestResults()
		{
			if (File.Exists(_testResultsFile))
			{
				XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
				var testResultsFile = XElement.Load(_testResultsFile);

				_executionStatus.Clear();

				var unitTests = testResultsFile
									.Element(ns + "TestDefinitions")
									.Elements(ns + "UnitTest")
									.Select(ut =>
									{
										var testMethod = ut.Element(ns + "TestMethod");

										return new
										{
											dll = GetAttributeValue(testMethod, "codeBase"),
											testId = GetAttributeValue(ut, "id"),
											executionId = GetAttributeValue(ut.Element(ns + "Execution"), "id"),
											methodName = String.Format("{0}.{1}", GetAttributeValue(testMethod, "className"), GetAttributeValue(testMethod, "name"))
										};
									});

				var results = testResultsFile.Element(ns + "Results")
											.Elements(ns + "UnitTestResult")
											.Select(ut =>
											{
												return new
														{
															result = GetTestResult(ut, ns),
															executionId = GetAttributeValue(ut, "executionId"),
															testId = GetAttributeValue(ut, "testId"),
														};

											});

				_execution = unitTests.Join(results, 
					f => new { f.executionId, f.testId }, 
					r => new { r.executionId, r.testId }, 
					(f, r) =>
					{
						r.result.MethodName = f.methodName;
						return new Tuple<string, TestResult>(f.dll, r.result);
					});
			}
		}

		private TestResult GetTestResult(XElement ut, XNamespace ns)
		{
			var output = ut.Element(ns + "Output");
											XElement errorInfo = null;
											if (output != null)
											{
												errorInfo = output.Element(ns + "ErrorInfo");
											}

											var errorMessage = errorInfo != null ? GetElementValue(errorInfo, "Message", ns) : null;
											var stackTrace = errorInfo != null ? GetElementValue(errorInfo, "StackTrace", ns) : null;

			return new TestResult(null,
								GetTestExecutionStatus(GetAttributeValue(ut, "outcome")),
								(Decimal)TimeSpan.Parse(GetAttributeValue(ut, "duration")).TotalSeconds,
								errorMessage,
								stackTrace,
								null);

		}

		internal override void UpdateTestMethodsExecution(IEnumerable<TestClass> tests)
		{
			var executedTests = tests.SelectMany(t => t.TestMethods)
										.Join(_execution,
												t => new { d = t.Class.DLLPath, n = t.FullyQualifiedName },
												t => new { d = t.Item1, n = t.Item2.MethodName },
												(testMethod, result) => new { TestMethod = testMethod, Result = result });

			foreach (var test in executedTests)
			{
				test.TestMethod.ExecutionResult = test.Result.Item2;
			}

			if (File.Exists(_testResultsFile))
			{
				IDEHelper.OpenFile(OpenCoverUIPackage.Instance.DTE, _testResultsFile);
			}
		}
	}
}

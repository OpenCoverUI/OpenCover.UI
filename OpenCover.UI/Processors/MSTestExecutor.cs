//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenCover.UI.Processors
{
	internal class MSTestExecutor : TestExecutor
	{
		protected TestMethodGroupingField _groupingField;
		private string _vsTestPath;

		internal MSTestExecutor(OpenCoverUIPackage package, Tuple<IEnumerable<String>, IEnumerable<String>, IEnumerable<String>> selectedTests)
			: base(package, selectedTests)
		{
			_vsTestPath = Path.Combine(Path.GetDirectoryName(_package.DTE.FullName), @"CommonExtensions\Microsoft\TestWindow\vstest.console.exe");
		}

		protected override void SetOpenCoverCommandlineArguments()
		{
			string dllPaths = BuildDLLPath();
			var builder = new StringBuilder();

			switch (_groupingField)
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

			_commandLineArguments = String.Format(_commandlineStringFormat,
										_vsTestPath,
										String.Format("{0} {1} /Logger:trx", dllPaths, builder.ToString()),
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
		}

		internal override void UpdateTestMethodsExecution(IEnumerable<TestClass> tests)
		{
		}
	}
}

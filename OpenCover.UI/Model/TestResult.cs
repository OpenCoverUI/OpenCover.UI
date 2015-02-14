//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model
{
	/// <summary>
	/// Represents a test Result file.
	/// </summary>
	public class TestResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestResult"/> class.
		/// </summary>
		/// <param name="methodName">Name of the method.</param>
		/// <param name="status">The test execution status.</param>
		/// <param name="executionTime">The test execution time.</param>
		/// <param name="failureMessages">The failure messages.</param>
		public TestResult(string methodName, TestExecutionStatus status, decimal? executionTime, string errorMessage, string stackTrace, List<TestResult> testCases)
		{
			MethodName = methodName;
			Status = status;
			ExecutionTime = executionTime;
			TestCases = testCases;

			if (errorMessage != null || stackTrace != null)
			{
				FailureMessages = new TestResultError(errorMessage, stackTrace);
			}
		}

		/// <summary>
		/// Gets the name of the method.
		/// </summary>
		public string MethodName
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the execution status.
		/// </summary>
		public TestExecutionStatus Status
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the test execution time.
		/// </summary>
		public decimal? ExecutionTime
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the failure messages.
		/// </summary>
		public TestResultError FailureMessages
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the test cases.
		/// </summary>
		public List<TestResult> TestCases
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is test case.
		/// </summary>
		public bool IsTestCase
		{
			get
			{
				return TestCases != null;
			}
		}

		internal TestResult Clone()
		{
			return new TestResult(
				MethodName, 
				Status, 
				ExecutionTime, FailureMessages != null ? FailureMessages.Message : null, 
				FailureMessages != null ? FailureMessages.StackTrace : null, 
				TestCases != null ? TestCases.Select(tc => tc.Clone()).ToList() : null);
		}
	}

	/// <summary>
	/// Represents an Error class
	/// </summary>
	public class TestResultError
	{
		/// <summary>
		/// Gets the message.
		/// </summary>
		public string Message
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the stack trace.
		/// </summary>
		public string StackTrace
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TestResultError"/> class.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="stackTrace">The stack trace.</param>
		public TestResultError(string message, string stackTrace)
		{
			Message = message;
			StackTrace = stackTrace;
		}
	}
}

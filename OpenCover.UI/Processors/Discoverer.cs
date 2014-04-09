//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using OpenCover.UI.Helpers;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace OpenCover.UI.Processors
{
	/// <summary>
	/// Discovers tests in the given dlls.
	/// </summary>
	internal class Discoverer
	{
		private IEnumerable<string> _dlls;

		/// <summary>
		/// Initializes a new instance of the <see cref="Discoverer"/> class.
		/// </summary>
		/// <param name="dlls">The DLLS.</param>
		public Discoverer(IEnumerable<string> dlls)
		{
			_dlls = dlls;
		}

		/// <summary>
		/// Discovers all tests in the selected assemblies.
		/// </summary>
		/// <returns></returns>
		public void Discover(Action<List<TestClass>> discoveryDone)
		{
			if (_dlls != null)
			{
				var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var testDiscovererPath = Path.Combine(assemblyPath, "OpenCover.UI.TestDiscoverer.exe");

				if (File.Exists(testDiscovererPath))
				{
					var builder = new StringBuilder();

					foreach (var dll in _dlls)
					{
						builder.AppendFormat("\"{0}\" ", dll);
					}

					if (builder.Length == 0)
					{
						return;
					}

					StartTestDiscovery(discoveryDone, testDiscovererPath, builder.ToString());
				}
				else
				{
					IDEHelper.WriteToOutputWindow("{0} not found. OpenCover cannot discover tests", testDiscovererPath);
				}
			}

			return;
		}

		/// <summary>
		/// Starts the test discovery by starting the process OpenCover.UI.TestDiscoverer.exe.
		/// </summary>
		/// <param name="discoveryDone">The delegate that needs to be called after test discovery is done.</param>
		/// <param name="testDiscovererPath">The test discoverer path.</param>
		/// <param name="tests">The tests.</param>
		private void StartTestDiscovery(Action<List<TestClass>> discoveryDone, string testDiscovererPath, String testsDLLs)
		{
			List<TestClass> tests = new List<TestClass>();
			string pipeGuid = Guid.NewGuid().ToString();
			var pipeServer = new NamedPipeServerStream(pipeGuid, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

			pipeServer.BeginWaitForConnection(res =>
			{
				if (res.IsCompleted)
				{
					pipeServer.EndWaitForConnection(res);

					var newTests = ReadObject<OpenCover.UI.Model.Test.TestClass[]>(pipeServer);
					if (newTests != null && newTests.Length > 0)
					{
						tests.AddRange(newTests);
					}

					tests.ForEach(TestMethodWrapper => TestMethodWrapper.UpdateChildren());

					IDEHelper.WriteToOutputWindow("{0} tests found", tests.Sum(test => test.TestMethods != null ? test.TestMethods.Length : 0));
					discoveryDone(tests);
				}
			}, null);

			var processInfo = new ProcessStartInfo(testDiscovererPath, String.Format("{0} {1}", pipeGuid, testsDLLs))
			{
				CreateNoWindow = true,
				UseShellExecute = false	
			};

			Process.Start(processInfo);
		}

		public T ReadObject<T>(Stream stream)
		{
			var jsSerializer = new JavaScriptSerializer();
			var streamReader = new StreamReader(stream);
			var json = streamReader.ReadToEnd();
			var tests = jsSerializer.Deserialize<T>(json);

			return (T)tests;
		}
	}
}
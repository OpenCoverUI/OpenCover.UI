//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCover.UI.Helpers;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
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
			List<TestClass> tests = new List<TestClass>();

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

					string pipeGuid = Guid.NewGuid().ToString();
					var pipeServer = new NamedPipeServerStream(pipeGuid, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

					Process.Start(new ProcessStartInfo(testDiscovererPath, String.Format("{0} {1}", pipeGuid, builder.ToString())));

					pipeServer.BeginWaitForConnection(res =>
					{
						if (res.IsCompleted)
						{
							pipeServer.EndWaitForConnection(res);

							tests.AddRange(ReadObject<OpenCover.UI.Model.Test.TestClass[]>(pipeServer));
							tests.ForEach(testClass => testClass.UpdateChildren());

							IDEHelper.WriteToOutputWindow("{0} tests found", tests.Sum(test => test.TestMethods != null ? test.TestMethods.Length : 0));
							discoveryDone(tests);
						}
					}, null);
				}
				else
				{
					IDEHelper.WriteToOutputWindow("{0} not found. OpenCover cannot discover tests", testDiscovererPath);
				}
			}

			return;
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
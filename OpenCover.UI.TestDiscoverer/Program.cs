using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OpenCover.UI.TestDiscoverer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length > 1)
				{
					// var pipe = GetPipeStream(args[0]);

					NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", args[0], PipeDirection.InOut);
					pipeClient.Connect();

					Discover(args, pipeClient);

					pipeClient.WaitForPipeDrain();

					pipeClient.Close();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		private static void Discover(string[] args, NamedPipeClientStream stream)
		{
			if (args != null && args.Length > 0)
			{
				var tests = new Discoverer(args.Skip(1)).Discover();
				string serialized = String.Empty;

				if (tests != null)
				{
					var jsSerializer = new JavaScriptSerializer();
					serialized = jsSerializer.Serialize(tests);
				}

				Write(stream, serialized);
			}
		}

		private static void Write(Stream stream, string json)
		{
			//Debugger.Launch();
			var writer = new StreamWriter(stream);
			writer.Write(json);
			writer.Flush();
		}
	}
}
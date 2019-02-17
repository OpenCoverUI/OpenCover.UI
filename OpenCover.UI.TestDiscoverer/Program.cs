//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Web.Script.Serialization;

namespace OpenCover.UI.TestDiscoverer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Log("START");
                //args = $@"bdf0d464-5bcc-4a4f-aaa3-0d2d1bafe5e4, I:\ConsoleApp1\ConsoleApp1.Tests\bin\Debug\ConsoleApp1.Tests.dll".Split(',').Select(s => s.Trim()).ToArray();
                if (args.Length > 1)
                {
                    Log(string.Join(", ", args));
                    Log("Creating client");
                    NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", args[0], PipeDirection.InOut);
                    Log("Client connecting");
                    pipeClient.Connect();
                    Log("Starting discovery");
                    Discover(args, pipeClient);

                    Log("Waiting for pipedrain");
                    pipeClient.WaitForPipeDrain();

                    Log("Closing connection");
                    pipeClient.Close();
                    Log("Connection closed");
                }
            }
            catch (Exception ex)
            {
                Log("EXCEPTION");
                Log(ex.Message);
                Console.WriteLine(ex.Message);
            }
        }
        public static void Log(string logMessage)
        {
            string filePath = $@"C:\myFile.txt";
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }
            using (StreamWriter w = File.AppendText(filePath))
            {
                w.WriteLine(logMessage + System.Environment.NewLine);
            }
        }

        private static void Discover(string[] args, NamedPipeClientStream stream)
        {
            Log("In discovery");
            if (args != null && args.Length > 0)
            {
                var dlls = args.Skip(1);

                Log("DLLS: " + dlls);
                var tests = new Discoverer(dlls).Discover();
                Log("Tests" + tests ?? "Geen tests gevonden");
                string serialized = String.Empty;

                if (tests != null)
                {
                    var jsSerializer = new JavaScriptSerializer();
                    serialized = jsSerializer.Serialize(tests);
                }
                Log("Tests" + serialized);

                Write(stream, serialized);
            }
        }

        private static void Write(Stream stream, string json)
        {
            var writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
        }
    }
}
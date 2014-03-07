using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCover.UI.Processors.Communication;
using OpenCover.UI.TestDiscoverer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCoverUI.Tests
{
	[TestClass]
	public class DiscovererFixture
	{
		[TestMethod]
		public void ValidTestDLL_ShouldListAllTests()
		{
			var discoverer = new Discoverer(new string[] { @"D:\API\Olympus\Main\Source\Server\Build\Test\APISoftware.Navigator.Services.Test.dll" });

			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			var tests = discoverer.Discover();
			int counter = 0;

			foreach (var test in tests)
			{
				foreach (var method in test.TestMethods)
				{
					counter++;
					Console.WriteLine(method.Name);
				}
			}

			sw.Stop();

			Console.WriteLine(counter);
			Console.WriteLine(sw.ElapsedMilliseconds / 1024.0);
		}
	}
}

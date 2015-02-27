//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using NUnit.Framework;
using OpenCover.UI.Model.Test;
using OpenCover.UI.TestDiscoverer.MSTest;
using OpenCover.UI.TestDiscoverer.NUnit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenCover.UI.TestDiscoverer
{
	/// <summary>
	/// Discovers tests in the given dlls.
	/// </summary>
	internal class Discoverer : IDiscoverer
	{
        private IList<IDiscoverer> discoverers = new List<IDiscoverer>();

		/// <summary>
		/// Initializes a new instance of the <see cref="Discoverer"/> class.
		/// </summary>
		/// <param name="dlls">The DLLS.</param>
		public Discoverer(IEnumerable<string> dlls)
		{
            discoverers.Add(new NUnitDiscoverer(dlls));
            discoverers.Add(new MSTestDiscoverer(dlls));

		}

		/// <summary>
		/// Discovers all tests in the selected assemblies.
		/// </summary>
		/// <returns></returns>
		public List<TestClass> Discover()
		{
			List<TestClass> tests = new List<TestClass>();

            if (discoverers != null)
			{
                foreach (var discoverer in discoverers)
				{
                    tests.AddRange(discoverer.Discover());
				}
			}

			return tests;
		}

	}
}

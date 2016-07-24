using Mono.Cecil;
using OpenCover.UI.Model.Test;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.TestDiscoverer
{
	internal class XUnitDiscoverer : DiscovererBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XUnitDiscoverer"/> class.
		/// </summary>
		/// <param name="dlls">The DLLS.</param>
		public XUnitDiscoverer(IEnumerable<string> dlls)
			: base(dlls)
		{}

		/// <summary>
		/// Discovers the tests in the Assembly.
		/// </summary>
		/// <param name="dllPath">The path to the DLL.</param>
		/// <param name="assembly">The loaded Assembly.</param>
		/// <returns>Tests in the Assembly</returns>
		protected override List<TestClass> DiscoverTestsInAssembly(string dll, AssemblyDefinition assembly)
		{
			var classes = new List<TestClass>();
			foreach (var type in assembly.MainModule.Types)
			{
				bool isXunitTest = false;

				try
				{
					if(type.HasMethods)
					{
						isXunitTest = type.Methods
							.Any(m => m.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(Xunit.FactAttribute).FullName)
									|| m.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(Xunit.TheoryAttribute).FullName));                      
					}                    
				}
				catch { }

				if (isXunitTest)
				{
					var TestClass = new TestClass
					{
						DLLPath = dll,
						Name = type.Name,
						Namespace = type.Namespace,
						TestType = TestType.XUnit
					};

					TestClass.TestMethods = DiscoverTestsInClass(type, TestClass);
					classes.Add(TestClass);
				}
			}
			return classes;
		}

		/// <summary>
		/// Discovers the tests in class.
		/// </summary>
		/// <param name="type">Type of the class.</param>
		/// <returns>Tests in the class</returns>
		private TestMethod[] DiscoverTestsInClass(TypeDefinition type, TestClass @class)
		{
			var tests = new List<TestMethod>();
			foreach (var method in type.Methods)
			{
				bool isTestMethod = false;
				var trait = new List<string>();

				try
				{
					foreach (var attribute in method.CustomAttributes)
					{

						if (attribute.AttributeType.FullName == typeof(Xunit.FactAttribute).FullName
							|| attribute.AttributeType.FullName == typeof(Xunit.TheoryAttribute).FullName)
						{
							isTestMethod = true;
						}
					}
				}
				catch { }

				if (isTestMethod)
				{
					TestMethod testMethod = new TestMethod();
					testMethod.Name = method.Name;
					testMethod.Traits = trait.Count > 0 ? trait.ToArray() : new[] { "No Traits" };
					tests.Add(testMethod);
				}
			}

			return tests.ToArray();
		}
	}
}

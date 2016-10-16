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
        /// Recursively loops through the typeDefinition to search for MSTest TestClassAttribute
        /// and returns the found TestClasses in the list.
        /// </summary>
        /// <param name="typeDefinition">A typeDefinition contains in the test assembly, can have nested types</param>
        /// <param name="dll">the dll being worked on, just being passed through</param>
        /// <returns></returns>
	    public List<TestClass> FindXUnitTestClassInType(TypeDefinition typeDefinition, string dll)
        {
            List<TestClass> testClasses = new List<TestClass>();

            foreach (var nestedType in typeDefinition.NestedTypes)
            {
                List<TestClass> subTestClasses = FindXUnitTestClassInType(nestedType, dll); // recursive call
                if (subTestClasses != null)
                {
                    testClasses.AddRange(subTestClasses);
                }
            }


            bool isXunitTest = false;

            try
            {
                if (typeDefinition.HasMethods)
                {
                    isXunitTest = isXUnitTest(typeDefinition);
                }
            }
            catch { }

            if (isXunitTest)
            {
                AddTestClass(dll, typeDefinition, testClasses);
            }
           
            return testClasses;
        }

        private void AddTestClass(string dll, TypeDefinition type, List<TestClass> classes2)
        {
            string nameSpace = GetNameSpace(type);
            var TestClass = new TestClass
            {
                DLLPath = dll,
                Name = type.Name,
                Namespace = nameSpace,
                TestType = TestType.XUnit
            };

            TestClass.TestMethods = DiscoverTestsInClass(type, TestClass);
            classes2.Add(TestClass);
        }
        /// <summary>
        /// Discovers the tests in the Assembly.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="assembly">The loaded Assembly.</param>
        /// <returns>Tests in the Assembly</returns>
        protected override List<TestClass> DiscoverTestsInAssembly(string dllPath, AssemblyDefinition assembly)
		{

            var classes2 = new List<TestClass>();
            foreach (var type in assembly.MainModule.Types)
            {
                classes2.AddRange(FindXUnitTestClassInType(type, dllPath));
            }
            return classes2;
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

	    private bool isXUnitTest(TypeDefinition typeDefinition)
	    {
            return typeDefinition.Methods
                            .Any(m => m.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(Xunit.FactAttribute).FullName)
                                    || m.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(Xunit.TheoryAttribute).FullName));
        }
	}
}

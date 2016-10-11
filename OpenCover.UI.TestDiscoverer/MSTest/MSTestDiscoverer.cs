//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using NUnit.Framework;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Collections.Generic;

namespace OpenCover.UI.TestDiscoverer.MSTest
{
	/// <summary>
	/// Discovers tests in the given dlls.
	/// </summary>
	internal class MSTestDiscoverer : DiscovererBase
	{
		/// <summary>
        /// Initializes a new instance of the <see cref="MSTestDiscoverer"/> class.
		/// </summary>
		/// <param name="dlls">The DLLS.</param>
        public MSTestDiscoverer(IEnumerable<string> dlls)
            : base(dlls)
		{

		}

        /// <summary>
        /// Recursively loops through the typeDefinition to search for MSTest TestClassAttribute
        /// and returns the found TestClasses in the list.
        /// </summary>
        /// <param name="typeDefinition">A typeDefinition contains in the test assembly, can have nested types</param>
        /// <param name="dll">the dll being worked on, just being passed through</param>
        /// <returns></returns>
	    public List<TestClass> FindMSTestClassInType(TypeDefinition typeDefinition, string dll)
	    {
	        List<TestClass> testClasses = new List<TestClass>(); 

            foreach (var nestedType in typeDefinition.NestedTypes)
            {
                List<TestClass> subTestClasses = FindMSTestClassInType(nestedType, dll); // recursive call
                if (subTestClasses != null)
                {
                    testClasses.AddRange(subTestClasses);
                }
            }

	        bool isMSTest = false;
            var customAttributes = typeDefinition.CustomAttributes;
            if (customAttributes != null)
            {
                isMSTest = customAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(TestClassAttribute).FullName);
            }
	        if (isMSTest)
	        {
	            AddTestClass(dll, typeDefinition, testClasses);
	        }
	        return testClasses;
	    }

	    /// <summary>
	    /// Discovers the tests in the Assembly.
	    /// </summary>
	    /// <param name="dllPath">The path to the DLL.</param>
	    /// <param name="assembly">The loaded Assembly.</param>
	    /// <returns>Tests in the Assembly</returns>
	    protected override List<TestClass> DiscoverTestsInAssembly(string dll, AssemblyDefinition assembly)
	    {
	        var classes2 = new List<TestClass>();
	        foreach (var type in assembly.MainModule.Types)
	        {
	            classes2.AddRange(FindMSTestClassInType(type, dll));
	        }
	        return classes2;
	    }

	    private void AddTestClass(string dll, TypeDefinition type, List<TestClass> classes2)
	    {
            string nameSpace = GetNameSpace(type);
            var TestClass = new TestClass
	        {
	            DLLPath = dll,
	            Name = type.Name,
	            Namespace = nameSpace,
	            TestType = TestType.MSTest
	        };

	        TestClass.TestMethods = DiscoverTestsInClass(type, TestClass);
	        classes2.Add(TestClass);
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

                        if (attribute.AttributeType.FullName == typeof(TestMethodAttribute).FullName)
                        {
                            isTestMethod = true;
                        }

                        AddTraits(trait, attribute, typeof(TestCategoryAttribute));
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

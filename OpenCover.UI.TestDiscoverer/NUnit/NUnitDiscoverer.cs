//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using Mono.Cecil;
using NUnit.Framework;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenCover.UI.TestDiscoverer.NUnit
{
	/// <summary>
	/// Discovers tests in the given dlls.
	/// </summary>
	internal class NUnitDiscoverer : DiscovererBase
	{
		/// <summary>
        /// Initializes a new instance of the <see cref="NUnitDiscoverer"/> class.
		/// </summary>
		/// <param name="dlls">The DLLS.</param>
        public NUnitDiscoverer(IEnumerable<string> dlls)
            : base(dlls)
        {

        }

        /// <summary>
        /// Discovers the tests in the Assembly.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="assembly">The loaded Assembly.</param>
        /// <returns>Tests in the Assembly</returns>
        protected override List<TestClass> DiscoverTestsInAssembly(string dllPath, AssemblyDefinition assembly)
        {
            var classes = new List<TestClass>();
            foreach (var type in assembly.MainModule.Types)
            {
                bool isNUnitTest = false;

                try
                {
                    isNUnitTest = IsNUnitTest(type);
                }
                catch { }

                if (isNUnitTest)
                {
                    var TestClass = new TestClass
                    {
                        DLLPath = dllPath,
                        Name = type.Name,
                        Namespace = type.Namespace,
                        TestType = TestType.NUnit
                    };

                    TestClass.TestMethods = DiscoverTestsInClass(type, TestClass);

                    classes.Add(TestClass);
                }
            }
            return classes;
        }

		/// <summary>
		/// Determines whether the Type has TestFixtrue Attribute on itself or on one of its parents
		/// </summary>
		/// <param name="type">The type.</param>
		private bool IsNUnitTest(TypeDefinition type)
		{
			if (type == null)
			{
				return false;
			}

			if (type.CustomAttributes != null && type.CustomAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(TestFixtureAttribute).FullName))
			{
				return true;
			}

			if (type.BaseType != null && type.BaseType is TypeDefinition)
			{
				return IsNUnitTest(type.BaseType as TypeDefinition);
			}

			return false;
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
                        if (attribute.AttributeType.FullName == typeof(TestAttribute).FullName 
                            || attribute.AttributeType.FullName == typeof(TestCaseAttribute).FullName)
                        {
                            isTestMethod = true;
                        }

                        AddTraits(trait, attribute, typeof(CategoryAttribute));
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

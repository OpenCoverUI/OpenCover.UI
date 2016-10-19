using Mono.Cecil;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.TestDiscoverer
{
    internal abstract class DiscovererBase : IDiscoverer
    {
        protected IEnumerable<string> _dlls;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscovererBase"/> class.
        /// </summary>
        /// <param name="dlls">The DLLS.</param>
        protected DiscovererBase(IEnumerable<string> dlls)
        {
            _dlls = dlls;
        }

        /// <summary>
        /// Discovers all tests in the selected assemblies.
        /// </summary>
        /// <returns></returns>
        public virtual List<TestClass> Discover()
        {
            List<TestClass> tests = new List<TestClass>();

            if (_dlls != null)
            {
                foreach (var dll in _dlls)
                {
                    tests.AddRange(DiscoverTestsInDLL(dll));
                }
            }

            return tests;
        }

        /// <summary>
        /// Discovers the tests in DLL.
        /// </summary>
        /// <param name="dll">The DLL.</param>
        /// <returns>Tests in the DLL</returns>
        protected virtual List<TestClass> DiscoverTestsInDLL(string dllPath)
        {
            var classes = new List<TestClass>();

            if (File.Exists(dllPath))
            {
                AssemblyDefinition assembly = null;
                try
                {
                    assembly = LoadAssembly(dllPath);
                }
                catch { }

                if (assembly != null)
                {
                    var testClassesInAssembly = DiscoverTestsInAssembly(dllPath, assembly);
                    classes.AddRange(testClassesInAssembly);
                }
            }

            return classes;
        }

        /// <summary>
        /// Discovers the tests in the Assembly.
        /// </summary>
        /// <param name="dllPath">The path to the DLL.</param>
        /// <param name="assembly">The loaded Assembly.</param>
        /// <returns>Tests in the Assembly</returns>
        protected abstract List<TestClass> DiscoverTestsInAssembly(string dllPath, AssemblyDefinition assembly);

        /// <summary>
        /// Loads the assembly and all its referenced assemblies.
        /// </summary>
        /// <param name="dll">The DLL.</param>
        /// <returns>Loaded assembly</returns>
        protected static AssemblyDefinition LoadAssembly(string dll)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(dll));

            AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dll);
            return assembly;
        }

        /// <summary>
        /// Adds the traits.
        /// </summary>
        /// <param name="traits">The traits.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="attributeType">Type of the attribute.</param>
        protected static void AddTraits(List<string> traits, CustomAttribute attribute, Type attributeType)
        {
            if (attribute.AttributeType.FullName == attributeType.FullName)
            {
                if (attribute.ConstructorArguments != null && attribute.ConstructorArguments.Count > 0)
                {
                    var trait = attribute.ConstructorArguments[0].Value.ToString();
                    if (!traits.Contains(trait))
                    {
                        traits.Add(trait);
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the assembly has a reference to the NUnit library.
        /// </summary>
        /// <param name="assembly">Assembly to check.</param>
        protected static bool AssemblyHasReferenceTo(AssemblyDefinition assembly, string referenceName)
        {
            bool hasNUnitReference = false;

            foreach (var anrRef in assembly.MainModule.AssemblyReferences)
            {
                if (string.Equals(anrRef.Name, referenceName, StringComparison.CurrentCultureIgnoreCase))
                {
                    hasNUnitReference = true;
                    break;
                }
            }
            return hasNUnitReference;
        }

        /// <summary>
        /// Retrieves the namespace for a type, will search up the tree of 
        /// DeclaringTypes when necessary
        /// </summary>
        /// <param name="typeDefinition"></param>
        /// <returns></returns>
        protected string GetNameSpace(TypeDefinition typeDefinition)
        {
            TypeDefinition declaringType = typeDefinition.DeclaringType;
            string name_space = typeDefinition.Namespace;
            while (declaringType != null)
            {
                name_space = declaringType.Namespace;
                declaringType = declaringType.DeclaringType;
            }
            return name_space;
        }
    }
}

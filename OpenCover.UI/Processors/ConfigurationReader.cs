using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OpenCover.UI.Processors
{
    internal class ConfigurationReader
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IEnumerable<string> Parameters { get; private set; }

        /// <summary>
        /// Gets the post processor commands.
        /// </summary>
        /// <value>
        /// The post processor commands.
        /// </value>
        public IDictionary<string, string> PostProcessorCommands { get; private set; }

        /// <summary>
        /// Gets the test result post processor command.
        /// </summary>
        /// <value>
        /// The test result post processor command.
        /// </value>
        public string TestResultPostProcessorCommand
        {
            get
            {
                var result = SafeGetPostProcessorCommand("testresult");
                return result;
            }
        }

        /// <summary>
        /// Gets the coverage result post processor command.
        /// </summary>
        /// <value>
        /// The coverage result post processor command.
        /// </value>
        public string CoverageResultPostProcessorCommand
        {
            get
            {
                var result = SafeGetPostProcessorCommand("coverageresult");
                return result;
            }
        }

        /// <summary>
        /// Safely gets the post processor command.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string SafeGetPostProcessorCommand(string key)
        {
            var result = string.Empty;
            if (PostProcessorCommands.ContainsKey(key))
            {
                result = PostProcessorCommands[key];
            }

            return result;
        }

        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="currentWorkingDirectory">The current working directory.</param>
        /// <returns></returns>
        public bool ReadConfiguration(DirectoryInfo currentWorkingDirectory)
        {
            var configPath = Path.Combine(currentWorkingDirectory.FullName, "OpenCover.UI.Config");
            var configFileExists = File.Exists(configPath);
            if (configFileExists)
            {
                ReadConfiguration(configPath);
            }

            return configFileExists;
        }
        
        /// <summary>
        /// Reads the configuration.
        /// </summary>
        /// <param name="configPath">The configuration path.</param>
        private void ReadConfiguration(string configPath)
        {
            var doc = XDocument.Load(configPath);
            if (doc.Root != null)
            {
                ReadPostProcessorCommands(doc);
                ReadParameters(doc);
            }
        }

        /// <summary>
        /// Reads the post processor commands.
        /// </summary>
        /// <param name="doc">The document.</param>
        private void ReadPostProcessorCommands(XDocument doc)
        {
            Debug.Assert(doc.Root != null, "doc.Root != null");
            PostProcessorCommands = doc.Root
                .Elements("PostProcessors")
                .Elements("Command")
                .ToDictionary(x => (string)x.Attribute("key"), x => x.Value);
        }

        /// <summary>
        /// Reads the parameters.
        /// </summary>
        /// <param name="doc">The document.</param>
        private void ReadParameters(XDocument doc)
        {
            Debug.Assert(doc.Root != null, "doc.Root != null");
            var parameters = doc.Root
                .Elements("Parameters")
                .Elements("Parameter")
                .Select(x => Tuple.Create((string)x.Attribute("name"), x.Value.Trim()))
                .Where(x => !String.IsNullOrWhiteSpace(x.Item1))
                .Select(x => String.Format("-{0}{1}{2}", x.Item1.Trim(), !String.IsNullOrWhiteSpace(x.Item2) ? ":" : "", x.Item2))
                .ToList();

            Parameters = parameters.ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OpenCover.UI.Processors
{
    internal class CommandLineParameterReader
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public IEnumerable<string> Parameters { get; private set; }

        /// <summary>
        /// Reads the parameters.
        /// </summary>
        /// <param name="currentWorkingDirectory">The current working directory.</param>
        /// <returns></returns>
        public bool ReadParameters(DirectoryInfo currentWorkingDirectory)
        {
            var configPath = Path.Combine(currentWorkingDirectory.FullName, "OpenCover.UI.Config");
            var configFileExists = File.Exists(configPath);
            if (configFileExists)
            {
                ReadParameters(configPath);
            }

            return configFileExists;
        }

        /// <summary>
        /// Reads the parameters.
        /// </summary>
        /// <param name="configPath">The configuration path.</param>
        private void ReadParameters(string configPath)
        {
            var doc = XDocument.Load(configPath);
            if (doc.Root != null)
            {
                var parameters = doc.Root
                    .Elements("Parameters")
                    .Elements("Parameter")
                    .Select(x => Tuple.Create((string) x.Attribute("name"), x.Value.Trim()))
                    .Where(x => !String.IsNullOrWhiteSpace(x.Item1))
                    .Select(x => String.Format("-{0}{1}{2}", x.Item1.Trim(), !String.IsNullOrWhiteSpace(x.Item2) ? ":" : "", x.Item2))
                    .ToList();

                Parameters = parameters.ToList();
            }
        }
    }
}
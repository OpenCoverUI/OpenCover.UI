//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
	/// <summary>
	/// A coverage session
	/// </summary>
	public class CoverageSession
	{
		/// <summary>
		/// initialise a coverage session
		/// </summary>
		public CoverageSession()
		{
			Modules = new Module[0];
			Summary = new Summary();
		}
		/// <summary>
		/// A unique session identifier
		/// </summary>
		public string SessionId { get; set; }

		/// <summary>
		/// A Summary of results for the session
		/// </summary>
		public Summary Summary { get; set; }

		/// <summary>
		/// A list of modules that have been profiled under the session
		/// </summary>
		public Module[] Modules { get; set; }

		public IEnumerable<Module> CoveredModules
		{
			get
			{
				if (this.Modules != null && this.Modules.Length > 0)
				{
					return this.Modules.Where(c => c.Summary.SequenceCoverage > 0);
				}

				return null;
			}
		}

		public IEnumerable<IGrouping<uint, SequencePoint[]>> GetSequencePoints()
		{
			if (Modules != null)
			{
				return Modules.Where(module => module.Summary.SequenceCoverage > 0).SelectMany(module => module.GetSequencePoints());
			}
			
			return null;
		}

		public IEnumerable<File> GetFiles()
		{
			if (Modules != null)
			{
				return Modules.Where(module => module.Summary.SequenceCoverage > 0).SelectMany(module => module.Files);
			}

			return null;
		}

		public IEnumerable<SequencePoint> GetSequencePoints(string fileName)
		{
			var allSequencePoints = GetSequencePoints();
			var allFiles = GetFiles();
			IEnumerable<SequencePoint> sequencePoints = null;

			if (allFiles != null && allSequencePoints != null) 
			{
				var selectedFile = allFiles.FirstOrDefault(file => file.FullPath.Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
				if (selectedFile != null)
				{
					var sequencePointsForTheFile = allSequencePoints.Where(sp => sp != null && sp.Key == selectedFile.UniqueId);
					sequencePoints = sequencePointsForTheFile.SelectMany(sp => sp).SelectMany(sp => sp); 
				}
			}

			return sequencePoints;
		}
	}
}

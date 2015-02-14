//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
	/// <summary>
	/// An instrumentable point
	/// </summary>
	public class InstrumentationPoint
	{
		/// <summary>
		/// Store the number of visits
		/// </summary>
		[XmlAttribute("vc")]
		public int VisitCount { get; set; }

		/// <summary>
		/// A unique number
		/// </summary>
		[XmlAttribute("uspid")]
		public UInt32 UniqueSequencePoint { get; set; }

		/// <summary>
		/// An order of the point within the method
		/// </summary>
		[XmlAttribute("ordinal")]
		public UInt32 Ordinal { get; set; }

		/// <summary>
		/// The IL offset of the point
		/// </summary>
		[XmlAttribute("offset")]
		public int Offset { get; set; }

		/// <summary>
		/// Used to hide an instrumentation point
		/// </summary>
		[XmlIgnore]
		public bool IsSkipped { get; set; }

	}
}

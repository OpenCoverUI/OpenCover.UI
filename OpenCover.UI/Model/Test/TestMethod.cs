//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenCover.UI.Model.Test
{
	/// <summary>
	/// Represents a test method
	/// </summary>
	[DataContract]
	internal partial class TestMethod 
	{
		/// <summary>
		/// Gets or sets the name of method.
		/// </summary>
		/// <value>
		/// The full name.
		/// </value>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the trait (TestCategory in MSTest.
		/// </summary>
		/// <value>
		/// The trait.
		/// </value>
		[DataMember]
		public string[] Traits { get; set; }
	}
}

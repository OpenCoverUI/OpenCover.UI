//
// This source code is released under the MIT License; Please read license.md file for more details.
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
		[DataMember(Name="n")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the trait (TestCategory in MSTest.
		/// </summary>
		[DataMember(Name="t")]
		public string[] Traits { get; set; }
	}
}

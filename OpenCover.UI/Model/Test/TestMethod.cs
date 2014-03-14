//
// This source code is released under the MIT License;
//
using System;
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
		public string Trait { get; set; }

	}
}

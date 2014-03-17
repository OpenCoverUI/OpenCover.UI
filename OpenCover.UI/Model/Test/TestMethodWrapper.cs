using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OpenCover.UI.Model.Test
{
	/// <summary>
	/// Represents a test class
	/// </summary>
	[DataContract]
	internal partial class TestMethodWrapper
	{
		/// <summary>
		/// Gets or sets the full name of a TestClass.
		/// </summary>
		/// <value>
		/// The full name.
		/// </value>
		[DataMember(Name="n")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the DLL path.
		/// </summary>
		/// <value>
		/// The DLL path.
		/// </value>
		[DataMember(Name="p")]
		public string DLLPath { get; set; }

		/// <summary>
		/// Gets or sets the test methods.
		/// </summary>
		/// <value>
		/// The test methods.
		/// </value>
		[DataMember(Name="m")]
		public TestMethod[] TestMethods { get; set; }

		/// <summary>
		/// Gets or sets the namespace.
		/// </summary>
		/// <value>
		/// The namespace.
		/// </value>
		[DataMember(Name="ns")]
		public string Namespace { get; set; }
	}
}

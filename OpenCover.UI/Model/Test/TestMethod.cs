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
		/// Gets or sets the test execution status.
		/// </summary>
		/// <value>
		/// The execution status.
		/// </value>
		[DataMember]
		public TestExecutionStatus ExecutionStatus { get; set; }

		/// <summary>
		/// Gets or sets the class that this method belongs to.
		/// </summary>
		/// <value>
		/// The class.
		/// </value>
		[DataMember]
		public TestClass Class { get; set; }

		public string FullyQualifiedName
		{ 
			get
			{
				return Name;
				//return string.Format("{0}.{1}", Class.FullName, Name);
			}
		}
	}
}

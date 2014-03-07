//
// This source code is released under the MIT License;
//
using System;
namespace OpenCover.UI.Model.Test
{
	/// <summary>
	/// Represents a test method
	/// </summary>
	[Serializable]
	public partial class TestMethod 
	{
		/// <summary>
		/// Gets or sets the name of method.
		/// </summary>
		/// <value>
		/// The full name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the test execution status.
		/// </summary>
		/// <value>
		/// The execution status.
		/// </value>
		public TestExecutionStatus ExecutionStatus { get; set; }

		/// <summary>
		/// Gets or sets the class that this method belongs to.
		/// </summary>
		/// <value>
		/// The class.
		/// </value>
		public TestClass Class { get; set; }

		public string FullyQualifiedName
		{ 
			get
			{
				return string.Format("{0}.{1}", Class.FullName, Name);
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace OpenCover.UI.Model.Test
{
	/// <summary>
	/// Represents a test class
	/// </summary>
	[Serializable]
	public partial class TestClass
	{
		/// <summary>
		/// Gets or sets the full name of a TestClass.
		/// </summary>
		/// <value>
		/// The full name.
		/// </value>
		public string FullName { get; set; }

		/// <summary>
		/// Gets or sets the DLL path.
		/// </summary>
		/// <value>
		/// The DLL path.
		/// </value>
		public string DLLPath { get; set; }

		/// <summary>
		/// Gets or sets the test methods.
		/// </summary>
		/// <value>
		/// The test methods.
		/// </value>
		public List<TestMethod> TestMethods { get; set; }

	}
}

/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.Text;
using System.Reflection;
using Microsoft.VsSDK.UnitTestLibrary;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using OpenCover.UI;
using OpenCover.UI.Views;

namespace OpenCoverUI_UnitTests.CodeCoverageResultsToolWindowTest
{
	/// <summary>
	///This is a test class for CodeCoverageResultsToolWindowTest and is intended
	///to contain all CodeCoverageResultsToolWindowTest Unit Tests
	///</summary>
	[TestClass()]
	public class CodeCoverageResultsToolWindowTest
	{

		/// <summary>
		///CodeCoverageResultsToolWindow Constructor test
		///</summary>
		[TestMethod()]
		public void CodeCoverageResultsToolWindowConstructorTest()
		{

			var target = new CodeCoverageResultsToolWindow();
			Assert.IsNotNull(target, "Failed to create an instance of CodeCoverageResultsToolWindow");

			MethodInfo method = target.GetType().GetMethod("get_Content", BindingFlags.Public | BindingFlags.Instance);
			Assert.IsNotNull(method.Invoke(target, null), "MyControl object was not instantiated");

		}

		/// <summary>
		///Verify the Content property is valid.
		///</summary>
		[TestMethod()]
		public void WindowPropertyTest()
		{
			CodeCoverageResultsToolWindow target = new CodeCoverageResultsToolWindow();
			Assert.IsNotNull(target.Content, "Content property was null");
		}

	}
}

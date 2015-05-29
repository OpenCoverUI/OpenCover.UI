//
// This source code is released under the MIT License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace OpenCover.UI.Tagger
{
	/// <summary>
	/// Type exports for background color classification type definition 
	/// </summary>
	public static class CoveredTextBackgroundTypeExports
	{
		[Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassificationTypes.TextBackgroundCovered)]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}

	/// <summary>
	/// Type exports for background color classification type definition 
	/// </summary>
	public static class NotCoveredTextBackgroundTypeExports
	{
		[Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassificationTypes.TextBackgroundNotCovered)]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}

	/// <summary>
    /// Class defining background color for covered classes, can be customized by the user through the Text Editor settings
    /// in Visual Studio
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypes.TextBackgroundCovered)]
    [Name(ClassificationTypes.TextBackgroundCovered)]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class CoveredTextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoveredTextBackground"/> class.
		/// </summary>
		public CoveredTextBackground()
		{
            DisplayName = "OpenCover.UI Covered Text Background";
            BackgroundColor = Color.FromRgb(207, 231, 209);	
		}
	}

	/// <summary>
	/// Class defining background color for not covered classes, can be customized by the user through the Text Editor settings
	/// in Visual Studio
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassificationTypes.TextBackgroundNotCovered)]
    [Name(ClassificationTypes.TextBackgroundNotCovered)]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class NotCoveredTextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoveredTextBackground"/> class.
		/// </summary>
		public NotCoveredTextBackground()
		{
            DisplayName = "OpenCover.UI Not Covered Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);			
		}
	}
}

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
		[Name("text-background-covered")]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}

	/// <summary>
	/// Type exports for background color classification type definition 
	/// </summary>
	public static class NotCoveredTextBackgroundTypeExports
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name("text-background-notcovered")]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}

	/// <summary>
	/// Class defining background color for covered classes
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "text-background-covered")]
	[Name("text-background-covered")]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class CoveredTextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoveredTextBackground"/> class.
		/// </summary>
		public CoveredTextBackground()
		{
			DisplayName = "Covered Text Background";
            BackgroundColor = Color.FromRgb(207, 231, 209);	
		}
	}

	/// <summary>
	/// Class defining background color for covered classes
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "text-background-notcovered")]
	[Name("text-background-notcovered")]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class NotCoveredTextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CoveredTextBackground"/> class.
		/// </summary>
		public NotCoveredTextBackground()
		{
			DisplayName = "Not Covered Text Background";
            BackgroundColor = Color.FromRgb(255, 217, 217);			
		}
	}
}

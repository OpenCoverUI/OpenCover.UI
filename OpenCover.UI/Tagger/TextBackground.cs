//
// This source code is released under the MIT License;
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
	public static class TypeExports
	{
		[Export(typeof(ClassificationTypeDefinition))]
		[Name("text-background")]
		public static ClassificationTypeDefinition OrdinaryClassificationType;
	}

	/// <summary>
	/// Class defining background color for covered classes
	/// </summary>
	[Export(typeof(EditorFormatDefinition))]
	[ClassificationType(ClassificationTypeNames = "text-background")]
	[Name("text-background")]
	[UserVisible(true)]
	[Order(After = Priority.High)]
	public sealed class TextBackground : ClassificationFormatDefinition
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TextBackground"/> class.
		/// </summary>
		public TextBackground()
		{
			DisplayName = "Text Background";
			BackgroundColor = Colors.LightGreen;
		}
	}
}

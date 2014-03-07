//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.Tagger
{
	/// <summary>
	/// Text tagger to produce tags to change background color for covered lines
	/// </summary>
	public sealed class TextTagger : ITagger<ClassificationTag>
	{
		private readonly ITextView _textView;
		private readonly ITextSearchService _searchService;
		private readonly IClassificationType _type;
		private NormalizedSnapshotSpanCollection _currentSpans;
		private CodeCoverageResultsControl _codeCoverageResultsControl;

		/// <summary>
		/// Occurs when tags are changed
		/// </summary>
		public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };

		/// <summary>
		/// Initializes a new instance of the <see cref="TextTagger"/> class.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="searchService">The search service.</param>
		/// <param name="type">The type.</param>
		public TextTagger(ITextView view, ITextSearchService searchService, IClassificationType type)
		{
			if (OpenCoverUIPackage.Instance == null)
			{
				return;
			}

			_textView = view;
			_searchService = searchService;
			_type = type;

			_codeCoverageResultsControl = OpenCoverUIPackage.Instance.ToolWindows.OfType<CodeCoverageResultsToolWindow>().First().CodeCoverageResultsControl;

			_currentSpans = GetWordSpans(_textView.TextSnapshot);

			_textView.GotAggregateFocus += SetupSelectionChangedListener;
		}

		/// <summary>
		/// Setups the selection changed listener.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void SetupSelectionChangedListener(object sender, EventArgs e)
		{
			if (_textView != null)
			{
				_textView.LayoutChanged += ViewLayoutChanged;
				_textView.GotAggregateFocus -= SetupSelectionChangedListener;
			}
		}

		/// <summary>
		/// Updates tags when the view layout is changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="TextViewLayoutChangedEventArgs"/> instance containing the event data.</param>
		private void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			if (e.OldSnapshot != e.NewSnapshot)
			{
				_currentSpans = GetWordSpans(e.NewSnapshot);
				TagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(e.NewSnapshot, 0, e.NewSnapshot.Length)));
			}
		}

		/// <summary>
		/// Returns the word spans based on covered lines.
		/// </summary>
		/// <param name="snapshot">The text snapshot of file being opened.</param>
		/// <returns>Collection of word spans</returns>
		private NormalizedSnapshotSpanCollection GetWordSpans(ITextSnapshot snapshot)
		{
			var wordSpans = new List<SnapshotSpan>();

			// If the file was opened by CodeCoverageResultsControl,
			if (_codeCoverageResultsControl != null && _codeCoverageResultsControl.IsFileOpening)
			{
				// Get covered sequence points
				var sequencePoints = _codeCoverageResultsControl.GetActiveDocumentSequencePoints();

				if (sequencePoints != null)
				{
					foreach (var sequencePoint in sequencePoints)
					{
						if (sequencePoint.VisitCount == 0)
						{
							continue;
						}

						int sequencePointStartLine = sequencePoint.StartLine - 1;
						int sequencePointEndLine = sequencePoint.EndLine - 1;

						var startLine = snapshot.Lines.FirstOrDefault(line => line.LineNumber == sequencePointStartLine);
						int totalCharacters = 0;

						if (sequencePoint.EndLine == sequencePoint.StartLine )
						{
							totalCharacters = sequencePoint.EndColumn - sequencePoint.StartColumn + 1;
						}
						else
						{
							// Get selected lines
							var selectedLines = snapshot.Lines
													.Where(line => line.LineNumber >= sequencePointStartLine &&
																	line.LineNumber <= sequencePointEndLine);

							// Measure the length of each sequence point
							foreach (var selectedLine in selectedLines)
							{
								if (selectedLine.LineNumber == sequencePointStartLine)
								{
									totalCharacters += selectedLine.Length - sequencePoint.StartColumn;
								}
								else if (selectedLine.LineNumber == sequencePointEndLine)
								{
									totalCharacters += selectedLine.Length - sequencePoint.EndColumn;
								}
								else
								{
									totalCharacters += selectedLine.Length;
								}
							}
						}

						// Create a snapshot for each sequence point covered
						var snapshotPoint = new SnapshotSpan(snapshot, new Span(startLine.Extent.Start.Position + sequencePoint.StartColumn - 1, totalCharacters));
						wordSpans.Add(snapshotPoint);
					}
				}
			}

			return new NormalizedSnapshotSpanCollection(wordSpans);
		}

		/// <summary>
		/// Generates tags based on Coverage information.
		/// </summary>
		/// <param name="spans">The spans.</param>
		/// <returns>Tags for the current file based on coverage information</returns>
		public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (spans == null || spans.Count == 0 || _currentSpans.Count == 0)
				yield break;

			ITextSnapshot snapshot = _currentSpans[0].Snapshot;
			spans = new NormalizedSnapshotSpanCollection(spans.Select(s => s.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive)));

			foreach (var span in NormalizedSnapshotSpanCollection.Intersection(_currentSpans, spans))
			{
				yield return new TagSpan<ClassificationTag>(span, new ClassificationTag(_type));
			}
		}
	}
}

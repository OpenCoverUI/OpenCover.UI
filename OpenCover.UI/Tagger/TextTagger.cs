//
// This source code is released under the GPL License; Please read license.md file for more details.
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
		private readonly IClassificationType _coveredType;
		private readonly IClassificationType _notCoveredType;
		private readonly Dictionary<SnapshotSpan, bool> _spanCoverage;
		private List<SnapshotSpan> _currentSpans;
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
		/// <param name="coveredType">The type.</param>
		public TextTagger(ITextView view, ITextSearchService searchService, IClassificationType coveredType, IClassificationType notCoveredType)
		{
			if (OpenCoverUIPackage.Instance == null)
			{
				return;
			}

			_textView = view;
			_searchService = searchService;
			_coveredType = coveredType;
			_notCoveredType = notCoveredType;

			_spanCoverage = new Dictionary<SnapshotSpan, bool>();
			_codeCoverageResultsControl = OpenCoverUIPackage.Instance
															.ToolWindows
															.OfType<CodeCoverageResultsToolWindow>()
															.First()
															.CodeCoverageResultsControl;

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
		/// Adds the word span.
		/// </summary>
		/// <param name="wordSpans">The word spans.</param>
		/// <param name="snapshot">The snapshot.</param>
		/// <param name="startColumn">The start column.</param>
		/// <param name="totalCharacters">The total characters.</param>
		/// <param name="covered">if set to <c>true</c> [covered].</param>
		private void AddWordSpan(List<SnapshotSpan> wordSpans, ITextSnapshot snapshot, int startColumn, int totalCharacters, bool covered)
		{
			var snapshotPoint = new SnapshotSpan(snapshot, new Span(startColumn, totalCharacters));
			wordSpans.Add(snapshotPoint);

			_spanCoverage.Add(snapshotPoint, covered);
		}

		/// <summary>
		/// Returns the word spans based on covered lines.
		/// </summary>
		/// <param name="snapshot">The text snapshot of file being opened.</param>
		/// <returns>Collection of word spans</returns>
		private List<SnapshotSpan> GetWordSpans(ITextSnapshot snapshot)
		{
			var wordSpans = new List<SnapshotSpan>();

			// If the file was opened by CodeCoverageResultsControl,
			if (_codeCoverageResultsControl != null && _codeCoverageResultsControl.IsFileOpening)
			{
				// Get covered sequence points
				var sequencePoints = _codeCoverageResultsControl.GetActiveDocumentSequencePoints();

				if (sequencePoints != null)
				{
					var covered = false;

					foreach (var sequencePoint in sequencePoints)
					{
						if (sequencePoint.VisitCount == 0)
						{
							covered = false;
						}
						else
						{
							covered = true;
						}

						int sequencePointStartLine = sequencePoint.StartLine - 1;
						int sequencePointEndLine = sequencePoint.EndLine - 1;

						var startLine = snapshot.Lines.FirstOrDefault(line => line.LineNumber == sequencePointStartLine);

						if (sequencePoint.EndLine == sequencePoint.StartLine)
						{
							AddWordSpan(wordSpans, snapshot, 
										startLine.Extent.Start.Position + sequencePoint.StartColumn - 1, 
										sequencePoint.EndColumn - sequencePoint.StartColumn + 1, covered);
						}
						else
						{
							// Get selected lines
							AddWordSpansForSequencePointsCoveringMultipleLines(snapshot, wordSpans, sequencePoint, 
																				sequencePointStartLine, sequencePointEndLine, covered);
						}
					}
				}
			}

			return (wordSpans);
		}

		/// <summary>
		/// Adds the word spans for sequence points covering multiple lines.
		/// </summary>
		/// <param name="snapshot">The snapshot.</param>
		/// <param name="wordSpans">The word spans.</param>
		/// <param name="sequencePoint">The sequence point.</param>
		/// <param name="sequencePointStartLine">The sequence point start line.</param>
		/// <param name="sequencePointEndLine">The sequence point end line.</param>
		/// <param name="covered">if set to <c>true</c> [covered].</param>
		private void AddWordSpansForSequencePointsCoveringMultipleLines(ITextSnapshot snapshot, 
																		List<SnapshotSpan> wordSpans, 
																		Framework.Model.SequencePoint sequencePoint, 
																		int sequencePointStartLine, 
																		int sequencePointEndLine,
																		bool covered)
		{
			int totalCharacters = 0;

			var selectedLines = snapshot.Lines
										.Where(line => line.LineNumber >= sequencePointStartLine &&
														line.LineNumber <= sequencePointEndLine);

			foreach (var selectedLine in selectedLines)
			{
				if (selectedLine.LineNumber == sequencePointStartLine)
				{
					totalCharacters = selectedLine.Length - sequencePoint.StartColumn + 1;

					AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position + sequencePoint.StartColumn - 1, totalCharacters, covered);
				}
				else if (selectedLine.LineNumber == sequencePointEndLine)
				{
					var temp = selectedLine.Length - (sequencePoint.EndColumn - 1);
					totalCharacters = selectedLine.Length - temp;

					AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position, totalCharacters, covered);
				}
				else
				{
					AddWordSpan(wordSpans, snapshot, selectedLine.Extent.Start.Position, selectedLine.Length, covered);
				}
			}
		}

		/// <summary>
		/// Generates tags based on Coverage information.
		/// </summary>
		/// <param name="spans">The spans.</param>
		/// <returns>Tags for the current file based on coverage information</returns>
		public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
			if (_currentSpans == null || _currentSpans.Count == 0)
				yield break;

			foreach (var span in _currentSpans)
			{
				var covered = _spanCoverage.ContainsKey(span) ? _spanCoverage[span] : false;
				var tag = covered ? new ClassificationTag(_coveredType) : new ClassificationTag(_notCoveredType);
				yield return new TagSpan<ClassificationTag>(span, tag);
			}
		}
	}
}

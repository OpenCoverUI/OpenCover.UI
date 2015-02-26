//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using OpenCover.UI.Helper;
using OpenCover.UI.Helpers;
using OpenCover.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCover.UI.Tagger
{
	/// <summary>
	/// Text tagger to produce tags to change background color for covered lines
	/// </summary>
    public sealed class TextTagger : TextViewCoverageProviderBase, ITagger<ClassificationTag>
	{		
		private ITextSearchService _searchService;
		private IClassificationType _coveredType;
		private IClassificationType _notCoveredType;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextTagger"/> class.
		/// </summary>
		/// <param name="view">The view.</param>
		/// <param name="searchService">The search service.</param>
		/// <param name="coveredType">The type.</param>
		public TextTagger(ITextView view, ITextSearchService searchService, IClassificationType coveredType, IClassificationType notCoveredType) : base(view)
		{
			if (OpenCoverUIPackage.Instance == null)
			{
				return;
			}
		
			_searchService = searchService;
			_coveredType = coveredType;
			_notCoveredType = notCoveredType;

            OpenCoverUIPackage.Instance.Settings.PropertyChanged += OnSettingsChanged;   
		}

        /// <summary>
        /// Disposes the tagger
        /// </summary>
        public override void Dispose()
        {
            if (OpenCoverUIPackage.Instance != null)
                OpenCoverUIPackage.Instance.Settings.PropertyChanged -= OnSettingsChanged;


            _searchService = null;
            _coveredType = null;
            _notCoveredType = null;

            base.Dispose();
        }
	
		/// <summary>
		/// Generates tags based on Coverage information.
		/// </summary>
		/// <param name="spans">The spans.</param>
		/// <returns>Tags for the current file based on coverage information</returns>
		public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
		{
            if (_currentSpans == null || _currentSpans.Count == 0 || !OpenCoverUIPackage.Instance.Settings.ShowLinesColored)
				yield break;

			foreach (var span in _currentSpans)
			{
				var covered = _spanCoverage.ContainsKey(span) ? _spanCoverage[span] : false;
				var tag = covered ? new ClassificationTag(_coveredType) : new ClassificationTag(_notCoveredType);
				yield return new TagSpan<ClassificationTag>(span, tag);
			}
		}

        /// <summary>
        /// Will be called when the settings were changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowLinesColored")
                RaiseAllTagsChanged();
        }
	}
}

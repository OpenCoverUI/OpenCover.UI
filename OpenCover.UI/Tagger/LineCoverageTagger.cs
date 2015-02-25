using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace OpenCover.UI.Tagger
{
    /// <summary>
    /// Tagger to produce tags to create glyphs for covered lines
    /// </summary>
    public sealed class LineCoverageTagger : ITagger<LineCoverageTag>
    {
        private IClassifier _classifier;

        /// <summary>
        /// Occurs when tags are changed
        /// </summary>
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = delegate { };

        /// <summary>
        /// Initializes a new instance of the <see cref="LineCoverageTagger"/> class.
        /// </summary>
        /// <param name="classifier">The classifier helper instance.</param>
        public LineCoverageTagger(IClassifier classifier)
        {
            _classifier = classifier;
        }

        /// <summary>
        /// Generates tags based on Coverage information.
        /// </summary>
        /// <param name="spans">The spans.</param>
        /// <returns>Tags for the current file based on coverage information</returns>
        IEnumerable<ITagSpan<LineCoverageTag>> ITagger<LineCoverageTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan span in spans)
            {
                //look at each classification span 
                foreach (ClassificationSpan classification in _classifier.GetClassificationSpans(span))
                {
                    //if the classification is not a comment 
                    var classificationString = classification.ClassificationType.Classification.ToLower();
                    if (!classificationString.Contains("xml doc") && !classificationString.Contains("comment"))
                    {
                        yield return new TagSpan<LineCoverageTag>(new SnapshotSpan(classification.Span.Start, 1), new LineCoverageTag());
                    }
                }
            }
        }
    }        
}

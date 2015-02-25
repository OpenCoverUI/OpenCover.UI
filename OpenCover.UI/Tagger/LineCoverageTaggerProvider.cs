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
    /// Line coverage glyph tag provider
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(LineCoverageTag))]
    public class LineCoverageTaggerProvider : ITaggerProvider
    {
        /// <summary>
        /// Gets the <see cref="IClassifier"/> helper.
        /// </summary>
        [Import]
        internal IClassifierAggregatorService AggregatorService;

        /// <summary>
        /// Creates the tagger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer">The buffer.</param>
        /// <returns></returns>
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            return new LineCoverageTagger(AggregatorService.GetClassifier(buffer)) as ITagger<T>;
        }
    }
}

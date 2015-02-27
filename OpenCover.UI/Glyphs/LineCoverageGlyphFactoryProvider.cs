using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using OpenCover.UI.Tagger;

namespace OpenCover.UI.Glyphs
{    
    /// <summary>
    /// Creates the <see cref="LineCoverageGlyphFactory"/> instance.
    /// </summary>
    [Export(typeof(IGlyphFactoryProvider))]
    [Name("TodoGlyph")]
    [Order(After = "VsTextMarker")]
    [ContentType("code")]
    [TagType(typeof(LineCoverageTag))]
    internal sealed class LineCoverageGlyphFactoryProvider : IGlyphFactoryProvider
    {
        /// <summary>
        /// Creates the factory instance for the glyphs.
        /// </summary>
        /// <param name="view">The editor view.</param>
        /// <param name="margin">The editor margin instance.</param>
        /// <returns></returns>
        public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
        {
            return new LineCoverageGlyphFactory(view);
        }
    }
}

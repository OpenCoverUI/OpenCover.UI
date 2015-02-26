using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using OpenCover.UI.Views;
using OpenCover.UI.Helpers;
using OpenCover.Framework.Model;
using OpenCover.UI.Helper;

namespace OpenCover.UI.Glyphs
{
    /// <summary>
    /// Factory for creating the line covergage glyphs.
    /// </summary>
    public class LineCoverageGlyphFactory : TextViewCoverageProviderBase, IGlyphFactory
    {
        const double _glyphSize = 12.0;
        
        private IEnumerable<SequencePoint> _sequencePoints;
        private static Brush _redBrush = new SolidColorBrush(Color.FromRgb(196, 64, 47));
        private static Brush _greenBrush = new SolidColorBrush(Color.FromRgb(88, 196, 84));
        private static Brush _orangeBrush = new SolidColorBrush(Color.FromRgb(196, 136, 41));

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="view">The current text editor.</param>
        public LineCoverageGlyphFactory(IWpfTextView view) :base(view)
        {
            _sequencePoints = GetSequencePointsForActiveDocument();            
        }

        /// <summary>
        /// Disposes the factory
        /// </summary>
        public override void Dispose()
        {
            _sequencePoints = null;
            base.Dispose();
        }

        /// <summary>
        /// Create the glyph element.
        /// </summary>
        /// <param name="line">Editor line to create the glyph for.</param>
        /// <param name="tag">The corresponding tag.</param>
        /// <returns></returns>
        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            // get the coverage info for the current line
            LineCoverageState state = GetLineCoverageState(line);

            // no coverage info found -> exit here
            if (state == LineCoverageState.Unknown)
                return null;

            var brush = GetBrushForState(state);

            if (brush == null)
                return null;

            System.Windows.Shapes.Ellipse ellipse = new Ellipse();
            ellipse.Fill = brush;
            ellipse.Height = _glyphSize;
            ellipse.Width = _glyphSize;

            ellipse.ToolTip = GetToolTipText(state);

            return ellipse;
        }

        /// <summary>
        /// Determines the correct brush for the coverage state.
        /// </summary>
        /// <param name="state">The line coverage state.</param>
        /// <returns></returns>
        private Brush GetBrushForState(LineCoverageState state)
        {
            switch (state)
            {               
                case LineCoverageState.Covered:
                    return _greenBrush;
                case LineCoverageState.Uncovered:
                    return _redBrush;
                case LineCoverageState.Partly:
                    return _orangeBrush;
            }

            return null;
        }

        /// <summary>
        /// Determines the tooltip text the coverage state.
        /// </summary>
        /// <param name="state">The line coverage state.</param>
        /// <returns></returns>
        private string GetToolTipText(LineCoverageState state)
        {
            switch (state)
            {
                case LineCoverageState.Covered:
                    return "This line is fully covered.";
                case LineCoverageState.Uncovered:
                    return "This line is not covered by any test.";
                case LineCoverageState.Partly:
                    return "This line is partly covered by tests. Click to show detailed coverage information.";
            }

            return null;
        }

        /// <summary>
        /// Determines the coverage state for the given line.
        /// </summary>
        /// <param name="line">Editor line to get the state for.</param>
        /// <returns></returns>
        private LineCoverageState GetLineCoverageState(ITextViewLine line)
        {
            // get cover state for all spans included in this line
            var spans = _currentSpans.Where(s => (s.Start >= line.Start && s.Start <= line.End) || (s.Start < line.Start && s.End >= line.Start)).ToList();
           
            if (spans.Count > 0)
            {
                IEnumerable<bool> coverageStates = spans.Select(s => _spanCoverage[s]);

                if (coverageStates.Count() > 0)
                {
                    // all covered
                    if (coverageStates.All(v => v))
                        return LineCoverageState.Covered;
                    // none covered
                    else if (!coverageStates.Any(v => v))
                        return LineCoverageState.Uncovered;
                    else
                        return LineCoverageState.Partly;
                }
            }
            
            return LineCoverageState.Unknown;
        }
       
    }
}

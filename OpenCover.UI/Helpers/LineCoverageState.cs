using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Helpers
{
    /// <summary>
    /// Coverage state for a editor line
    /// </summary>
    public enum LineCoverageState
    {
        /// <summary>
        /// Unknown. State will be ignored.
        /// </summary>
        /// <remarks>Mainly used for no "real" code</remarks>
        Unknown = 0,

        /// <summary>
        /// Line is fully covered
        /// </summary>
        Covered = 1,

        /// <summary>
        /// Line is not covered
        /// </summary>
        Uncovered = 2,

        /// <summary>
        /// Line is partly covered
        /// </summary>
        Partly = 3
    }
}

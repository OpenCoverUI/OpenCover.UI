using System.Linq;
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;

namespace OpenCover.UI.Model.ResultNodes
{
    public class CoverageNode : SharpTreeNode
    {
        readonly CoverageSession _coverageSession;

        public CoverageNode(CoverageSession coverageSession)
        {
            _coverageSession = coverageSession;
            LazyLoading = true;
        }

        protected override void LoadChildren()
        {
            if (_coverageSession.CoveredModules != null)
            {
                Children.AddRange(_coverageSession.CoveredModules.Select(module => new ModuleNode(module))); 
            }
        }
    }
}
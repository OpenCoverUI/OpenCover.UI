using System;
using System.Linq;
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;

namespace OpenCover.UI.Model.ResultNodes
{
    public class NamespaceNode : SharpTreeNode
    {
        private Module _module;
        private string _namespace;
        decimal _visitedSequencePoints = 0;
        decimal _numSequencePoints = 0;

        public NamespaceNode(Module module, string @namespace)
        {
            _module = module;
            _namespace = @namespace;

            LazyLoading = true;


            var coveredClasses = _module.CoveredClasses;
            foreach (var @class in coveredClasses
                .Where(cc => cc.Namespace == _namespace))
            {
                _visitedSequencePoints += @class.Summary.VisitedSequencePoints;
                _numSequencePoints += @class.Summary.NumSequencePoints;
            }
        }

        public override object Text
        {
            get
            {
                return _namespace;
            }
        }

        public decimal SequenceCoverage
        {
            get
            {
                return Math.Round((_visitedSequencePoints / _numSequencePoints * 100), 2);
            }
        }

        public string VisitedSequencePoints
        {
            get
            {
                return String.Format("{0} / {1}", _visitedSequencePoints, _numSequencePoints);
            }
        }

        public override object Icon
        {
            get
            {
                return IDEHelper.GetImageURL("Resources/Namespace.png");
            }
        }

        protected override void LoadChildren()
        {
            var coveredClasses = _module.CoveredClasses;

            coveredClasses = OpenCoverUIPackage.Instance.Settings.ShowUncoveredClasses 
                ? coveredClasses.Where(cc => cc.Namespace == _namespace) 
                : coveredClasses.Where(cc => cc.Namespace == _namespace && cc.Summary.SequenceCoverage > 0);

            var classNodes = coveredClasses.Select(@class => new ClassNode(@class));
            Children.AddRange(classNodes);
        }
    }
}
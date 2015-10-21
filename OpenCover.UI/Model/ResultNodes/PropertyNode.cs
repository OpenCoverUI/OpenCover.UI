using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model.ResultNodes
{
    public class PropertyNode : SharpTreeNode
    {
        public List<Method> PropertyMethods { get; private set; }

        private string _text = "";

        public PropertyNode(string text, List<Method> propertyMethods)
        {
            _text = text;
            PropertyMethods = propertyMethods;
            LazyLoading = true;
        }

        public decimal SequenceCoverage
        {
            get
            {
                return PropertyMethods.Sum(x => x.Summary.SequenceCoverage);
            }
        }

        public string VisitedSequencePoints
        {
            get
            {
                var visitedSequencePoints = PropertyMethods.Sum(x => x.Summary.VisitedSequencePoints);
                var numSequencePoints = PropertyMethods.Sum(x => x.Summary.NumSequencePoints);
                return string.Format("{0} / {1}", visitedSequencePoints, numSequencePoints);
            }
        }

        public override object Text
        {
            get
            {
                return _text;
            }
        }

        protected override void LoadChildren()
        {
            var propertyMethodNodes = PropertyMethods.OrderBy(x => x.Name)
                .Select(method => new PropertyMethodNode(method));
            Children.AddRange(propertyMethodNodes);
        }

        public override object Icon
        {
            get
            {
                return IDEHelper.GetImageURL("Resources/Property.png");
            }
        }
    }
}

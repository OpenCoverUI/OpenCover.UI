using System;
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;

namespace OpenCover.UI.Model.ResultNodes
{
    public class MethodNode : SharpTreeNode
    {
        public Method Method { get; private set; }

        public decimal SequenceCoverage
        {
            get
            {
                return Method.Summary.SequenceCoverage;
            }
        }

        public string VisitedSequencePoints
        {
            get
            {
                return String.Format("{0} / {1}", Method.Summary.VisitedSequencePoints, Method.Summary.NumSequencePoints);
            }
        }

        public MethodNode(Method method)
        {
            Method = method;
        }

        public override object Text
        {
            get
            {
                try
                {
                    return Method.Name.Split(new[] { ':' })[2];
                }
                catch
                {
                    return Method.Name;
                }
            }
        }

        public override object Icon
        {
            get
            {
                return IDEHelper.GetImageURL("Resources/Method.png");
            }
        }
    }
}
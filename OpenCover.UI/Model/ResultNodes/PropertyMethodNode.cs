using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model.ResultNodes
{
    public class PropertyMethodNode : MethodNode
    {
        public PropertyMethodNode(Method method)
            : base(method)
        {

        }
        public override object Icon
        {
            get
            {
                return IDEHelper.GetImageURL("Resources/Property.png");
            }
        }

        public override object Text
        {
            get
            {
                return Method.IsGetter ? "get" : "set";
            }
        }
    }
}

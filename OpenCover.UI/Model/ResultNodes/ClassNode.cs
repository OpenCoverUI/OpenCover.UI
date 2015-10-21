using System.Linq;
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
using System.Collections.Generic;

namespace OpenCover.UI.Model.ResultNodes
{
    public class ClassNode : SharpTreeNode
    {
        public Class Class { get; private set; }

        public decimal SequenceCoverage
        {
            get
            {
                return Class.Summary.SequenceCoverage;
            }
        }

        public string VisitedSequencePoints
        {
            get
            {
                return string.Format("{0} / {1}", Class.Summary.VisitedSequencePoints, Class.Summary.NumSequencePoints);
            }
        }

        public ClassNode(Class @class)
        {
            Class = @class;
            LazyLoading = true;
        }

        public override object Text
        {
            get
            {
                return Class.Name;
            }
        }

        protected override void LoadChildren()
        {
            var constructorNodes = Class.Methods
                .Where(x => x.IsConstructor)
                .OrderBy(x => x.Name)
                .Select(method => new MethodNode(method));

            var propertyNodes = Class.Methods
                .Where(x => x.IsGetter || x.IsSetter)
                .GroupBy(AsPropertyName)
                .OrderBy(x => x.Key)
                .Select(group => new PropertyNode(group.Key, group.ToList()));

            var methodNodes = Class.Methods
                .Where(x => !x.IsConstructor && !(x.IsGetter || x.IsSetter))
                .OrderBy(x => x.Name)
                .Select(method => new MethodNode(method));

            Children.AddRange(propertyNodes);
            Children.AddRange(constructorNodes);
            Children.AddRange(methodNodes);
        }

        private string AsPropertyName(Method method)
        {
            if (!(method.IsGetter || method.IsSetter))
            {
                return method.Name;
            }

            try
            {
                var methodName = method.Name;
                if (methodName.Contains("set_"))
                {
                    var name = methodName.Split(new[] { "set_" }, System.StringSplitOptions.None)[1];
                    var firstParenthesis = name.IndexOf('(');
                    if (firstParenthesis != -1)
                    {
                        name.Substring(0, firstParenthesis);
                    }
                    return name;
                }
                else if (methodName.Contains("get_"))
                {
                    var type = methodName.Split(new[] { ' ' })[0];
                    var nameParts = methodName.Split(new[] { "get_" }, System.StringSplitOptions.None);
                    var getWithEmptyParentheses = nameParts[1];

                    var firstParenthesis = getWithEmptyParentheses.IndexOf('(');
                    var getWithoutEmptyParentheses = (firstParenthesis != -1)
                        ? getWithEmptyParentheses.Substring(0, firstParenthesis)
                        : getWithEmptyParentheses;
                    var name = string.Format("{0}({1})", getWithoutEmptyParentheses, type);
                    return name;
                }

                return methodName;
            }
            catch
            {
                return method.Name;
            }
        }

        public override object Icon
        {
            get
            {
                return IDEHelper.GetImageURL("Resources/Class.png");
            }
        }
    }
}
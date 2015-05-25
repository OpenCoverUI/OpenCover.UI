using ICSharpCode.TreeView;
using OpenCover.UI.Model.Test;

namespace OpenCover.UI.Helpers
{
    public static class TreeNodeExtensions
    {
        internal static TestMethodWrapperContainer GetContainer(this SharpTreeNodeCollection treeNodeCollection, TestType testType)
        {
            TestMethodWrapperContainer container = null;

            foreach (var node in treeNodeCollection)
            {
                var testContainer = node as TestMethodWrapperContainer;
                if (null != testContainer && testContainer.TestType == testType) container = testContainer;
            }

            return container;
        }
    }
}

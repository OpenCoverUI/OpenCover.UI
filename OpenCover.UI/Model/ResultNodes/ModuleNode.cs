using System;
using System.IO;
using System.Linq;
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;

namespace OpenCover.UI.Model.ResultNodes
{
    public class ModuleNode : SharpTreeNode
	{
		Module _module;

		public decimal SequenceCoverage
		{
			get
			{
				return _module.Summary.SequenceCoverage;
			}
		}

		public string VisitedSequencePoints
		{
			get
			{
				return String.Format("{0} / {1}", _module.Summary.VisitedSequencePoints, _module.Summary.NumSequencePoints);
			}
		}

		public ModuleNode(Module module)
		{
			_module = module;
			LazyLoading = true;
		}

		public override object Text
		{
			get
			{
				return Path.GetFileName(_module.FullName);
			}
		}

		protected override void LoadChildren()
		{
			var namespaces = _module.CoveredClasses.Select(@class => @class.Namespace).Distinct();
			Children.AddRange(namespaces.Select(ns => new NamespaceNode(_module, ns)));
		}

		public override object Icon
		{
			get
			{
				return IDEHelper.GetImageURL("Resources/Library.png");
			}
		}
	}
}

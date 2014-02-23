//
// This source code is released under the MIT License;
//
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCover.UI.Model
{
	public class CoverageNode : SharpTreeNode
	{
		CoverageSession _coverageSession;

		public CoverageNode(CoverageSession coverageSession)
		{
			_coverageSession = coverageSession;
			LazyLoading = true;
		}

		protected override void LoadChildren()
		{
			Children.AddRange(_coverageSession.CoveredModules.Select(module => new ModuleNode(module)));
		}
	}

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

		public override string ToString()
		{
			return _module.ModuleName;
		}

		public override object Text
		{
			get
			{
				return _module.ModuleName;
			}
		}

		protected override void LoadChildren()
		{
			Children.AddRange(_module.CoveredClasses.Select(@class => new ClassNode(@class)));
		}
	}

	public class ClassNode : SharpTreeNode
	{
		public Class Class{get; private set;}

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
				return String.Format("{0} / {1}", Class.Summary.VisitedSequencePoints, Class.Summary.NumSequencePoints);
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
				return Class.FullName;
			}
		}

		public override string ToString()
		{
			return Class.FullName;
		}

		protected override void LoadChildren()
		{
			Children.AddRange(Class.CoveredMethods.Select(method => new MethodNode(method)));
		}
	}

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
				return Method.Name;
			}
		}
	}
}

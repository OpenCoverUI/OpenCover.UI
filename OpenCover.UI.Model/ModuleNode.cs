//
// This source code is released under the MIT License;
//
using ICSharpCode.TreeView;
using OpenCover.Framework.Model;
using OpenCover.UI.Helpers;
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

		public override object Text
		{
			get
			{
				return System.IO.Path.GetFileName(_module.FullName);
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
			
			foreach (var @class in _module.CoveredClasses
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
			Children.AddRange(_module.CoveredClasses.Where(cc => cc.Namespace == _namespace).Select(@class => new ClassNode(@class)));
		}
	}

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
				return Class.Name;
			}
		}

		protected override void LoadChildren()
		{
			Children.AddRange(Class.CoveredMethods.Select(method => new MethodNode(method)));
		}

		public override object Icon
		{
			get
			{
				return IDEHelper.GetImageURL("Resources/Class.png");
			}
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
				try
				{
					var methodName = Method.Name.Split(new[] { ':' })[2];

					string searchText = Method.IsGetter ? "get_" : Method.IsSetter ? "set_" : null;

					if (searchText != null)
					{
						methodName = methodName.Replace(searchText, "");
						methodName = methodName.Substring(0, methodName.IndexOf("("));
					}

					return methodName;
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
				return IDEHelper.GetImageURL(String.Format("Resources/{0}", Method.IsGetter || Method.IsSetter ? "Property.png" : "Method.png"));
			}
		}
	}
}

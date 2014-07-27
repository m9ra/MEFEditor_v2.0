using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

using Utilities;

namespace RecommendedExtensions.Core.TypeDefinitions.CompositionEngine
{
    /// <summary>
    /// Reference to instance occurance, used for manipulating instances during composition method creation
    /// </summary>
    class ComponentRef : InstanceRef
    {
        private Dictionary<Import, JoinPoint> _importPoints = new Dictionary<Import, JoinPoint>();
        private Dictionary<Export, JoinPoint> _exportPoints = new Dictionary<Export, JoinPoint>();


        internal readonly ComponentInfo ComponentInfo;

        internal bool HasSatisfiedPreImports;

        internal bool IsSatisfied;

        /// <summary>
        /// Determine that composition of instance has failed
        /// </summary>
        internal bool ComposingFailed { get; private set; }

        internal bool HasImports { get { return ComponentInfo.Imports.Length > 0; } }

        internal bool HasImportingConstructor { get { return ComponentInfo.ImportingConstructor != null; } }

        public IEnumerable<Import> Imports { get { return ComponentInfo.Imports; } }

        public IEnumerable<JoinPoint> ImportPoints { get { return _importPoints.Values; } }

        public IEnumerable<JoinPoint> ExportPoints { get { return _exportPoints.Values; } }

        public readonly Instance Component;

        public IEnumerable<JoinPoint> Points
        {
            get
            {
                foreach (var point in _exportPoints.Values)
                    yield return point;

                foreach (var point in _importPoints.Values)
                    yield return point;
            }
        }


        public ComponentRef(CompositionContext context, bool isConstructed, ComponentInfo componentInfo, Instance component)
            : base(context, component.Info, isConstructed)
        {
            Component = component;
            ComponentInfo = componentInfo;

            HasSatisfiedPreImports = isConstructed;

            if (ComponentInfo != null)
            {
                foreach (var exp in ComponentInfo.Exports)
                    addExport(exp);
                foreach (var exp in ComponentInfo.SelfExports)
                    addExport(exp);
                foreach (var imp in ComponentInfo.Imports)
                    addImport(imp);
            }
        }

        internal void CompositionError(string p)
        {
            throw new NotImplementedException();
        }

        internal JoinPoint GetPoint(Import import)
        {
            return _importPoints[import];
        }

        private void addExport(Export export)
        {
            var point = new JoinPoint(this, export);

            _exportPoints.Add(export, point);
        }

        private void addImport(Import import)
        {
            var point = new JoinPoint(this, import);

            _importPoints.Add(import, point);
        }
    }
}

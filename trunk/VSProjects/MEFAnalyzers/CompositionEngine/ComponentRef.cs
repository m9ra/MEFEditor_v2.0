using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;

using Utilities;

namespace MEFAnalyzers.CompositionEngine
{
    /// <summary>
    /// Reference to instance occurance, used for manipulating instances during composition method creation
    /// </summary>
    class ComponentRef
    {
        private Dictionary<Import, JoinPoint> _importPoints = new Dictionary<Import, JoinPoint>();
        private Dictionary<Export, JoinPoint> _exportPoints = new Dictionary<Export, JoinPoint>();

        private readonly CompositionContext _context;
        private Instance _component;

        internal readonly InstanceInfo Type;
        internal readonly ComponentInfo ComponentInfo;

        internal bool HasSatisfiedPreImports;

        internal bool IsSatisfied;


        internal bool IsComponent { get { return ComponentInfo != null; } }

        internal bool NeedsPrerequisitySatisfiing { get { return !HasSatisfiedPreImports && !ComposingFailed; } }

        /// <summary>
        /// Determine that composition of instance has failed
        /// </summary>
        internal bool ComposingFailed { get; private set; }

        internal bool HasImports { get { return ComponentInfo.Imports.Length > 0; } }

        internal bool IsConstructed { get; private set; }

        internal bool HasImportingConstructor { get { return ComponentInfo.ImportingConstructor != null; } }



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

        public IEnumerable<Import> Imports { get { return ComponentInfo.Imports; } }

        public IEnumerable<JoinPoint> ImportPoints { get { return _importPoints.Values; } }

        public IEnumerable<JoinPoint> ExportPoints { get { return _exportPoints.Values; } }

        public ComponentRef(CompositionContext context, Instance component, ComponentInfo componentInfo)
        {
            _component = component;
            _context = context;
            Type = component.Info;
            ComponentInfo = componentInfo;

            if (!IsComponent)
            {
                return;
            }

            foreach (var exp in ComponentInfo.Exports)
                addExport(exp);
            foreach (var exp in ComponentInfo.SelfExports)
                addExport(exp);
            foreach (var imp in ComponentInfo.Imports)
                addImport(imp);
        }

        internal void CompositionError(string p)
        {
            throw new NotImplementedException();
        }

        internal JoinPoint GetPoint(Import import)
        {
            throw new NotImplementedException();
        }

        internal void Construct(MethodID constructor, ComponentRef[] instance)
        {
            throw new NotImplementedException();
        }

        internal void Call(MethodID methodID, ComponentRef inst)
        {
            throw new NotImplementedException();
        }

        private void addExport(Export export)
        {
            var point = new JoinPoint(this, export);

            _exportPoints.Add(export, point);
        }

        private void addImport(Import import){
            var point = new JoinPoint(this, import);

            _importPoints.Add(import, point);
        }
    }
}

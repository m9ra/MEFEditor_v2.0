using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    public class ComponentInfoBuilder
    {
        private readonly List<Export> _selfExports = new List<Export>();

        private readonly List<Export> _exports = new List<Export>();

        private readonly List<Import> _imports = new List<Import>();

        private readonly List<CompositionPoint> _explicitCompositionPoints = new List<CompositionPoint>();

        private MethodID _importingCtor;

        public readonly TypeDescriptor ComponentType;

        public bool IsEmpty
        {
            get
            {
                return
                   _selfExports.Count == 0 &&
                   _exports.Count == 0 &&
                   _imports.Count == 0 &&
                   _explicitCompositionPoints.Count == 0
                   ;
            }
        }

        public ComponentInfoBuilder(TypeDescriptor componentType)
        {
            ComponentType = componentType;
        }

        public void AddExport(TypeDescriptor exportType, string propertyName, string contract = null)
        {
            if (contract == null)
            {
                contract = exportType.TypeName;
            }

            var getterID = Naming.Method(ComponentType, Naming.GetterPrefix + propertyName, false, new ParameterTypeInfo[0]);
            var export = new Export(exportType, getterID, contract);
            AddExport(export);
        }

        public void AddSelfExport(string contract)
        {
            var export = new Export(ComponentType, null, contract);

            _selfExports.Add(export);
        }

        public void AddExport(Export export)
        {
            _exports.Add(export);
        }



        public void AddImport(TypeDescriptor importType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            var importTypeInfo = ImportTypeInfo.Parse(importType);
            var contract = importTypeInfo.ItemType.TypeName;

            var import = new Import(importTypeInfo, setterID, contract);
            AddImport(import);
        }

        public void AddImport(Import import)
        {
            _imports.Add(import);
        }

        public void SetImportingCtor(params TypeDescriptor[] importingParameters)
        {
            var parameters = new ParameterTypeInfo[importingParameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                var importType = importingParameters[i];
                parameters[i] = ParameterTypeInfo.Create("p", importType);

                var importTypeInfo = ImportTypeInfo.Parse(importType);
                var contract = importTypeInfo.ItemType.TypeName;
                var preImport = new Import(importTypeInfo, null, contract);
                _imports.Add(preImport);
            }

            _importingCtor = Naming.Method(ComponentType, Naming.CtorName, false, parameters);
        }

        public void AddExplicitCompositionPoint(MethodID method)
        {
            _explicitCompositionPoints.Add(new CompositionPoint(ComponentType, method, true));
        }

        private MethodID getSetterID(TypeDescriptor importType, string setterName)
        {
            var parameters = new ParameterTypeInfo[] { ParameterTypeInfo.Create("value", importType) };
            var setterID = Naming.Method(ComponentType, Naming.SetterPrefix + setterName, false, parameters);
            return setterID;
        }

        public void AddManyImport(TypeDescriptor importType, TypeDescriptor itemType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            var importTypeInfo = ImportTypeInfo.ParseFromMany(importType, itemType);
            _imports.Add(new Import(importTypeInfo, setterID, importTypeInfo.ItemType.TypeName, true));
        }

        public ComponentInfo BuildInfo()
        {
            if (_importingCtor == null)
            {
                //default importin constructor
                _importingCtor = Naming.Method(ComponentType, Naming.CtorName, false);
            }

            var compositionPoints = new List<CompositionPoint>(_explicitCompositionPoints);

            return new ComponentInfo(ComponentType, _importingCtor, _imports.ToArray(), _exports.ToArray(), _selfExports.ToArray(), compositionPoints.ToArray());
        }

    }
}

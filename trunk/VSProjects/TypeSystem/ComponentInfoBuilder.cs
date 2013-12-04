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

        private readonly InstanceInfo _componentType;

        private MethodID _importingCtor;

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

        public ComponentInfoBuilder(InstanceInfo componentType)
        {
            _componentType = componentType;
        }

        public void AddExport(InstanceInfo exportType, string getterName)
        {
            var getterID = Naming.Method(_componentType, "get_" + getterName, false, new ParameterTypeInfo[0]);
            _exports.Add(new Export(exportType, getterID));
        }

        public void AddImport(InstanceInfo importType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            _imports.Add(new Import(importType, setterID));
        }

        public void SetImportingCtor(params InstanceInfo[] importingParameters)
        {
            var parameters = new ParameterTypeInfo[importingParameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                var importType = importingParameters[i];
                parameters[i] = ParameterTypeInfo.Create("p", importType);
                var preImport = new Import(importType, null);
                _imports.Add(preImport);
            }

            _importingCtor = Naming.Method(_componentType, Naming.CtorName, false, parameters);
        }

        public void AddExplicitCompositionPoint(MethodID method)
        {
            _explicitCompositionPoints.Add(new CompositionPoint(method, true));
        }

        private MethodID getSetterID(InstanceInfo importType, string setterName)
        {
            var parameters = new ParameterTypeInfo[] { ParameterTypeInfo.Create("value", importType) };
            var setterID = Naming.Method(_componentType, "set_" + setterName, false, parameters);
            return setterID;
        }

        public void AddManyImport(InstanceInfo importType, InstanceInfo itemType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            _imports.Add(new Import(importType, itemType, setterID, true));
        }

        public ComponentInfo BuildInfo()
        {
            if (_importingCtor == null)
            {
                //default importin constructor
                _importingCtor = Naming.Method(_componentType, Naming.CtorName, false);
            }


            return new ComponentInfo(_componentType, _importingCtor, _imports.ToArray(), _exports.ToArray(), _selfExports.ToArray());
        }


    }
}

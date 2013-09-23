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

        private readonly InstanceInfo _componentType;

        public ComponentInfoBuilder(InstanceInfo componentType)
        {
            _componentType = componentType;
        }

        public void AddExport(InstanceInfo exportType, string getterName)
        {
            var getterID = Naming.Method(_componentType, "get_"+getterName, new ParameterTypeInfo[0]);
            _exports.Add(new Export(exportType, getterID));
        }

        public void AddImport(InstanceInfo importType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            _imports.Add(new Import(importType, setterID));
        }

        private MethodID getSetterID(InstanceInfo importType, string setterName)
        {
            var parameters = new ParameterTypeInfo[] { ParameterTypeInfo.Create("value", importType) };
            var setterID = Naming.Method(_componentType, "set_" + setterName, parameters);
            return setterID;
        }

        public void AddManyImport(InstanceInfo importType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            _imports.Add(new Import(importType, setterID,true));
        }

        public ComponentInfo BuildInfo()
        {
            return new ComponentInfo(_componentType,_imports.ToArray(), _exports.ToArray(), _selfExports.ToArray());
        }
    }
}

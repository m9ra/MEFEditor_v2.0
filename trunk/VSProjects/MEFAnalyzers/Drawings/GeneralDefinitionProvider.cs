using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
using Analyzing;

using TypeSystem;
using TypeSystem.DrawingServices;

namespace MEFAnalyzers.Drawings
{
    public static class GeneralDefinitionProvider
    {
        public static void Draw(DrawedInstance instance, ComponentInfo info)
        {
            if (info == null)
                return;

            foreach (var export in info.Exports)
            {
                var connector = instance.GetJoinPoint(export);

                setProperty(connector, "Kind", "Export");
                setProperty(connector, "Contract", export.Contract);
                setProperty(connector, "ExportType", export.ExportType);
            }

            foreach (var import in info.Imports)
            {
                var connector = instance.GetJoinPoint(import);

                setProperty(connector, "Kind", "Import");
                setProperty(connector, "Contract", import.Contract);
                setProperty(connector, "ImportType", import.ImportTypeInfo.ImportType);
                setProperty(connector, "AllowMany", import.AllowMany);
                setProperty(connector, "AllowDefault", import.AllowDefault);
                setProperty(connector, "IsPrerequisity", import.IsPrerequisity);
            }
        }

        private static void setProperty(ConnectorDefinition connector, string propertyName, object propertyValue)
        {
            connector.SetProperty(new DrawingProperty(propertyName, propertyValue.ToString()));
        }

        private static void setProperty(ConnectorDefinition connector, string propertyName, InstanceInfo info)
        {
            setProperty(connector, propertyName, info.TypeName);
        }
    }
}

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
            if (instance.WrappedInstance.IsDirty)
            {
                //instance is dirty
                instance.SetProperty("IsDirty", "");
            }

            if (info == null)
                return;

            instance.SetProperty("DefiningAssembly", info.DefiningAssembly.Name);

            if (instance.WrappedInstance.IsEntryInstance)
            {
                handleEntryInstance(instance);
            }

            foreach (var export in info.Exports)
            {
                var connector = instance.GetJoinPoint(export);

                setProperty(connector, "Kind", "Export");
                setProperty(connector, "Contract", export.Contract);
                setProperty(connector, "ContractType", export.ExportType);

                setMetaProperties(connector, "$Meta", export.Meta);
            }

            foreach (var export in info.SelfExports)
            {
                var connector = instance.GetJoinPoint(export);

                setProperty(connector, "Kind", "SelfExport");
                setProperty(connector, "Contract", export.Contract);
                setProperty(connector, "ContractType", export.ExportType);

                setMetaProperties(connector, "$Meta", export.Meta);
            }

            foreach (var import in info.Imports)
            {
                var connector = instance.GetJoinPoint(import);

                setProperty(connector, "Kind", "Import");
                setProperty(connector, "Contract", import.Contract);
                setProperty(connector, "ContractType", import.ImportTypeInfo.ImportType);
                if (import.AllowMany)
                    setProperty(connector, "ContractItemType", import.ImportTypeInfo.ItemType);
                setProperty(connector, "AllowMany", import.AllowMany);
                setProperty(connector, "AllowDefault", import.AllowDefault);
                setProperty(connector, "IsPrerequisity", import.IsPrerequisity);
            }
        }

        private static void handleEntryInstance(DrawedInstance instance)
        {
            //instance that was pasted on analysis start
            instance.SetProperty("EntryInstance", "");
            var wrappedInstance = instance.WrappedInstance;
            var creationBlock = wrappedInstance.CreationBlock;
            if (creationBlock == null)
                return;

            var firstBlockInfo = creationBlock.FirstBlock.Info;
            if (firstBlockInfo != null && firstBlockInfo.BlockTransformProvider != null)
            {
                var navigation = firstBlockInfo.BlockTransformProvider.GetNavigation();
                if (navigation != null)
                    instance.Drawing.AddCommand(new CommandDefinition("Navigate to", () => navigation()));
            }
        }

        private static void setMetaProperties(ConnectorDefinition connector, string propertyPrefix, MetaExport meta)
        {
            foreach (var key in meta.ExportedKeys)
            {
                var item = meta.GetItem(key);
                var index = 1;
                foreach (var value in item.Data)
                {
                    var propertyName = propertyPrefix + index + "-" + item.Key;
                    var valueRepresentation = "'" + value + "'";
                    if (item.IsMultiple)
                        valueRepresentation += " | IsMultiple";

                    setProperty(connector, propertyName, valueRepresentation);

                    ++index;
                }
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

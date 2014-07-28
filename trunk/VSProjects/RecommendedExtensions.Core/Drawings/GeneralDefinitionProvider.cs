using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;

using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.DrawingServices;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Provider of general drawing used for every draw <see cref="DiagramItem" />.
    /// </summary>
    public static class GeneralDefinitionProvider
    {
        /// <summary>
        /// Draws the specified instance.
        /// </summary>
        /// <param name="instance">The draw instance.</param>
        /// <param name="info">The component information available for draw instance.</param>
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

                setInherited(connector, export);
                setProperty(connector, "Kind", "Export");
                setProperty(connector, "Contract", export.Contract);
                setProperty(connector, "ContractType", export.ExportType);

                setMetaProperties(connector, "$Meta", export.Meta);
            }

            foreach (var export in info.SelfExports)
            {
                var connector = instance.GetJoinPoint(export);

                setInherited(connector, export);
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

        /// <summary>
        /// Sets the inherited flag.
        /// </summary>
        /// <param name="connector">The connector where flag will be set.</param>
        /// <param name="export">The export which flag will be set.</param>
        private static void setInherited(ConnectorDefinition connector, Export export)
        {
            if (export.IsInherited)
                setProperty(connector, "IsInherited", "True");
        }

        /// <summary>
        /// Handles the entry instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
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

        /// <summary>
        /// Sets the meta information to properties with given prefix.
        /// </summary>
        /// <param name="connector">The connector where meta properties will be available.</param>
        /// <param name="propertyPrefix">The property prefix.</param>
        /// <param name="meta">The meta information.</param>
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

        /// <summary>
        /// Sets value of the property for given connector.
        /// </summary>
        /// <param name="connector">The connector.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        private static void setProperty(ConnectorDefinition connector, string propertyName, object propertyValue)
        {
            connector.SetProperty(new DrawingProperty(propertyName, propertyValue.ToString()));
        }

        /// <summary>
        /// Sets value of the property for given connector.
        /// </summary>
        /// <param name="connector">The connector to set.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="info">The information.</param>
        private static void setProperty(ConnectorDefinition connector, string propertyName, InstanceInfo info)
        {
            setProperty(connector, propertyName, info.TypeName);
        }
    }
}

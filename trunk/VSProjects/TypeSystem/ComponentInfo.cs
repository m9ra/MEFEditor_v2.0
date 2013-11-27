using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem
{
    /// <summary>
    /// Information about composition point.
    /// </summary>
    public class CompositionPoint
    {
        /// <summary>
        /// Determine that composition point has been created explicitly
        /// via attribute.
        /// </summary>
        public readonly bool IsExplicit;

        /// <summary>
        /// Entry method of composition point.
        /// </summary>
        public readonly MethodID EntryMethod;

        public CompositionPoint(MethodID entryMethod, bool isExplicit)
        {
            IsExplicit = isExplicit;
            EntryMethod = entryMethod;
        }
    }

    /// <summary>
    /// Interface used for information about components.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Type of component
        /// </summary>
        public readonly InstanceInfo ComponentType;
        /// <summary>
        /// Exports defined on whole class.
        /// </summary>
        public readonly Export[] SelfExports;
        /// <summary>
        /// Exports defined on class members.
        /// </summary>
        public readonly Export[] Exports;
        /// <summary>
        /// Imports defined on component.
        /// </summary>
        public readonly Import[] Imports;
        /// <summary>
        /// Composition points in component.
        /// </summary>
        public readonly CompositionPoint[] CompositionPoints;
        /// <summary>
        /// Constructor marked as importing constructor, or paramless constructor.
        /// </summary>
        public readonly MethodID ImportingConstructor;

        public ComponentInfo(InstanceInfo thisType, MethodID importingCtor, Import[] imports, Export[] exports, Export[] selfExports)
        {
            ComponentType = thisType;
            SelfExports = selfExports;
            Exports = exports;
            Imports = imports;
            ImportingConstructor = importingCtor;
        }
    }

    /// <summary>
    /// Exported metadata.
    /// </summary>
    public class MetaExport
    {
        /// <summary>
        /// Determine if key appeared with multiple indicator.
        /// IsMultiple values are stored in lists.
        /// </summary>
        /// <param name="key">Key for metadata entry</param>
        /// <returns>True if specified metadata entry is multiple. Otherwise false.</returns>
        public bool IsMultiple(string key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// All exported metadata
        /// </summary>
        public IDictionary<string, IEnumerable<Instance>> Data { get; private set; }
    }

    /// <summary>
    /// Information stored for export.
    /// </summary>
    public class Export
    {
        /// <summary>
        /// Exported metadata for this export.
        /// </summary>
        public readonly MetaExport Meta;
        /// <summary>
        /// Contract specified in Export attribute, or default contract according to ExportType.
        /// </summary>
        public readonly string Contract;
        /// <summary>
        /// Getter, which retrieves exported instance.
        /// </summary>
        public readonly MethodID Getter;
        /// <summary>
        /// Type of exported value.
        /// </summary>
        public readonly InstanceInfo ExportType;

        public Export(InstanceInfo exportType, MethodID getter)
        {
            ExportType = exportType;
            Contract = exportType.TypeName;
            Getter = getter;
        }
    }

    /// <summary>
    /// Parsed type of import.
    /// </summary>
    public class ImportTypeInfo
    {
        /// <summary>
        /// True, if ItemType is wrapped in lazy object
        /// </summary>
        public readonly bool IsItemLazy;
        /// <summary>
        /// Type of one item, without lazy, collection,... 
        /// Should be used as default Contract.
        /// </summary>
        public readonly InstanceInfo ItemType;
        /// <summary>
        /// Type of meta info, null if not available.
        /// </summary>
        public readonly InstanceInfo MetaDataType;
        /// <summary>
        /// Type for Importing setter/parameter.
        /// </summary>
        public readonly InstanceInfo ImportType;

        public ImportTypeInfo(InstanceInfo importType, InstanceInfo itemType = null)
        {
            if (itemType == null)
                itemType = importType;

            ImportType = importType;
            ItemType = itemType;
        }
    }

    /// <summary>
    /// Information stored for import.
    /// </summary>
    public class Import
    {
        /// <summary>
        /// Contract specified in Import attribute, or default contract according to import type
        /// </summary>
        public readonly string Contract;
        /// <summary>
        /// Info about importing type, IsLazy,ItemType,...
        /// </summary>
        public readonly ImportTypeInfo ImportTypeInfo;
        /// <summary>
        /// Setter, which set instance to requested target        
        /// is null, if import was obtained from importing constructor
        /// </summary>
        public readonly MethodID Setter;

        /// <summary>
        /// Determine if this import has to be satisfied before instance constructing
        /// </summary>
        public bool IsPrerequisity { get { return Setter == null; } }
        /// <summary>
        /// Determine if value can be default (no export needed)
        /// </summary>
        public readonly bool AllowDefault;
        /// <summary>
        /// Determine if import can accept more than one export
        /// </summary>
        public readonly bool AllowMany;

        public Import(InstanceInfo importType, MethodID setter, bool allowMany = false)
        {
            ImportTypeInfo = new ImportTypeInfo(importType);
            Contract = ImportTypeInfo.ItemType.TypeName;
            Setter = setter;
            AllowMany = allowMany;
        }

        public Import(InstanceInfo importType, InstanceInfo itemType, MethodID setter, bool allowMany = false)
        {
            ImportTypeInfo = new ImportTypeInfo(importType, itemType);
            Contract = ImportTypeInfo.ItemType.TypeName;
            Setter = setter;
            AllowMany = allowMany;
        }
    }
}

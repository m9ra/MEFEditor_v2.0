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
        /// Determine if composition point is implicit or explicitly specified via attribute.
        /// </summary>
        public bool IsExplicit { get; private set; }
        /// <summary>
        /// Entry method of composition point.
        /// </summary>
        public MethodID EntryMethod { get; private set; }
        /// <summary>
        /// Arguments which will be pasted into entry method on composition start.
        /// </summary>
        public Instance[] Arguments { get; private set; }
    }

    /// <summary>
    /// Interface used for information about components.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Type of component
        /// </summary>
        public InstanceInfo ComponentType { get; private set; }
        /// <summary>
        /// Exports defined on whole class.
        /// </summary>
        public Export[] SelfExports { get; private set; }
        /// <summary>
        /// Exports defined on class members.
        /// </summary>
        public Export[] Exports { get; private set; }
        /// <summary>
        /// Imports defined on component.
        /// </summary>
        public Import[] Imports { get; private set; }
        /// <summary>
        /// Composition points in component.
        /// </summary>
        public CompositionPoint[] CompositionPoints { get; private set; }
        /// <summary>
        /// Constructor marked as importing constructor, or paramless constructor.
        /// </summary>
        public MethodID ImportingConstructor { get; private set; }

        public ComponentInfo(InstanceInfo thisType,MethodID importingCtor,Import[] imports, Export[] exports, Export[] selfExports)
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
        public MetaExport Meta { get; private set; }
        /// <summary>
        /// Contract specified in Export attribute, or default contract according to ExportType.
        /// </summary>
        public string Contract { get; private set; }
        /// <summary>
        /// Getter, which retrieves exported instance.
        /// </summary>
        public MethodID Getter { get; private set; }
        /// <summary>
        /// Type of exported value.
        /// </summary>
        public InstanceInfo ExportType { get; private set; }

        public Export(InstanceInfo exportType,MethodID getter)
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
        public bool IsItemLazy { get; private set; }
        /// <summary>
        /// Type of one item, without lazy, collection,... 
        /// Should be used as default Contract.
        /// </summary>
        public InstanceInfo ItemType { get; private set; }
        /// <summary>
        /// Type of meta info, null if not available.
        /// </summary>
        public InstanceInfo MetaDataType { get; private set; }
        /// <summary>
        /// Type for Importing setter/parameter.
        /// </summary>
        public InstanceInfo ImportType { get; private set; }

        public ImportTypeInfo(InstanceInfo importType,InstanceInfo itemType=null)
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
        public string Contract { get; private set; }
        /// <summary>
        /// Info about importing type, IsLazy,ItemType,...
        /// </summary>
        public ImportTypeInfo ImportTypeInfo { get; private set; }
        /// <summary>
        /// Setter, which set instance to requested target        
        /// is null, if import was obtained from importing constructor
        /// </summary>
        public MethodID Setter { get; private set; }

        /// <summary>
        /// Determine if this import has to be satisfied before instance constructing
        /// </summary>
        public bool IsPrerequisity { get { return Setter == null; } }
        /// <summary>
        /// Determine if value can be default (no export needed)
        /// </summary>
        public bool AllowDefault { get; private set; }
        /// <summary>
        /// Determine if import can accept more than one export
        /// </summary>
        public bool AllowMany { get; private set; }

        public Import(InstanceInfo importType,MethodID setter,bool allowMany=false)
        {
            ImportTypeInfo = new ImportTypeInfo(importType);
            Contract = ImportTypeInfo.ItemType.TypeName;
            Setter = setter;
            AllowMany = allowMany;
        }

        public Import(InstanceInfo importType, InstanceInfo itemType, MethodID setter, bool allowMany = false)
        {
            ImportTypeInfo = new ImportTypeInfo(importType,itemType);
            Contract =ImportTypeInfo.ItemType.TypeName;
            Setter = setter;
            AllowMany = allowMany;
        }
    }
}

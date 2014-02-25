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

        /// <summary>
        /// Component where composition point is declared
        /// </summary>
        public readonly TypeDescriptor DeclaringComponent;

        public CompositionPoint(TypeDescriptor declaringComponent, MethodID entryMethod, bool isExplicit)
        {
            IsExplicit = isExplicit;
            EntryMethod = entryMethod;
            DeclaringComponent = declaringComponent;
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

        /// <summary>
        /// Assembly where current component was defined
        /// </summary>
        public TypeAssembly DefiningAssembly { get; internal set; }

        public ComponentInfo(InstanceInfo thisType, MethodID importingCtor, Import[] imports, Export[] exports, Export[] selfExports, CompositionPoint[] compositionPoints)
        {
            ComponentType = thisType;
            SelfExports = selfExports;
            Exports = exports;
            Imports = imports;
            ImportingConstructor = importingCtor;
            CompositionPoints = compositionPoints;
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
        public readonly TypeDescriptor ExportType;

        public Export(TypeDescriptor exportType, MethodID getter, string contract)
        {
            ExportType = exportType;
            Contract = contract;
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
        /// True if ImportType is wrapped in lazy object
        /// </summary>
        public readonly bool IsLazy;

        /// <summary>
        /// Type of one item, without lazy, collection,... 
        /// Should be used as default Contract.
        /// </summary>
        public readonly TypeDescriptor ItemType;

        /// <summary>
        /// Type of meta info, null if not available.
        /// </summary>
        public readonly TypeDescriptor MetaDataType;

        /// <summary>
        /// Type for Importing setter/parameter.
        /// </summary>
        public readonly TypeDescriptor ImportType;

        private ImportTypeInfo(TypeDescriptor importType, TypeDescriptor itemType)
        {
            ImportType = importType;
            ItemType = itemType;

            //TODO check if items are lazy
        }

        /// <summary>
        /// In ManyImport there can be Array{}, IEnumerable{} or anything derived from ICollection{}
        /// </summary>
        /// <param name="importManyType"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static ImportTypeInfo ParseFromMany(TypeDescriptor importManyType, bool allowMany, TypeServices services)
        {
            if (allowMany)
            {
                var currentChain = services.GetChain(importManyType);
                var itemType = findManyItemDescriptor(currentChain);
                return new ImportTypeInfo(importManyType, itemType);
            }
            else
            {
                return new ImportTypeInfo(importManyType, importManyType);
            }
        }

        public static ImportTypeInfo ParseFromMany(TypeDescriptor importManyType, TypeDescriptor itemType)
        {
            return new ImportTypeInfo(importManyType, itemType);
        }

        #region ImportMany ItemType parsing

        /// <summary>
        /// Find ItemType from given type representing many imports
        /// </summary>
        /// <param name="manyType">Many imports representation</param>
        /// <returns>ItemType if found according to MEF rules, null otherwise</returns>
        private static TypeDescriptor findManyItemDescriptor(InheritanceChain manyType)
        {
            var signature = manyType.Path.Signature;
            if (signature == TypeDescriptor.IEnumerableSignature)
            {
                //type is IEnumerable
                return manyType.Type.Arguments.First();
            }

            if (signature == TypeDescriptor.ArraySignature)
            {
                //type is Array
                return manyType.Type.Arguments.First();
            }

            return findICollectionItemDescriptor(manyType);
        }

        /// <summary>
        /// Find ItemType from given type representing many imports in ICollection{}
        /// </summary>
        /// <param name="manyType">Many imports representation</param>
        /// <returns>ItemType if found within ICollection according to MEF rules, null otherwise</returns>
        private static TypeDescriptor findICollectionItemDescriptor(InheritanceChain manyType)
        {
            var signature = manyType.Path.Signature;
            if (signature == TypeDescriptor.ICollectionSignature)
            {
                //type is ICollection
                return manyType.Type.Arguments.First();
            }

            foreach (var subchain in manyType.SubChains)
            {
                var descriptor = findICollectionItemDescriptor(subchain);
                if (descriptor != null)
                    return descriptor;
            }

            return null;
        }

        #endregion
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

        public Import(ImportTypeInfo importTypeInfo, MethodID setter, string contract, bool allowMany = false, bool allowDefault = false)
        {
            ImportTypeInfo = importTypeInfo;
            Contract = contract;
            Setter = setter;
            AllowMany = allowMany;
            AllowDefault = allowDefault;
        }
    }
}

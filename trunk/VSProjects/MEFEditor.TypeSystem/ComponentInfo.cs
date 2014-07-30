using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
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
        /// Component where composition point is declared.
        /// </summary>
        public readonly TypeDescriptor DeclaringComponent;

        /// <summary>
        /// Generator providing values for composition point arguments if available.
        /// </summary>
        public readonly GeneratorBase ArgumentProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionPoint"/> class.
        /// </summary>
        /// <param name="declaringComponent">The declaring component.</param>
        /// <param name="entryMethod">The entry method.</param>
        /// <param name="isExplicit">if set to <c>true</c> it is explicit composition point.</param>
        /// <param name="argumentProvider">The argument provider.</param>
        public CompositionPoint(TypeDescriptor declaringComponent, MethodID entryMethod, bool isExplicit, GeneratorBase argumentProvider)
        {
            IsExplicit = isExplicit;
            EntryMethod = entryMethod;
            DeclaringComponent = declaringComponent;
            ArgumentProvider = argumentProvider;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return EntryMethod.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o = obj as CompositionPoint;
            if (o == null)
                return false;

            return EntryMethod.Equals(o.EntryMethod);
        }
    }

    /// <summary>
    /// Interface used for information about components.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Names on that component validity depends.
        /// </summary>
        public readonly string[] DependencyNames;

        /// <summary>
        /// Type of component.
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
        /// Constructor marked as importing constructor, or param less constructor.
        /// </summary>
        public readonly MethodID ImportingConstructor;

        /// <summary>
        /// Assembly where current component was defined.
        /// </summary>
        /// <value>The defining assembly.</value>
        public TypeAssembly DefiningAssembly { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentInfo"/> class.
        /// </summary>
        /// <param name="componentType">Type of the component.</param>
        /// <param name="importingCtor">The importing ctor.</param>
        /// <param name="imports">The imports.</param>
        /// <param name="exports">The exports.</param>
        /// <param name="selfExports">The self exports.</param>
        /// <param name="compositionPoints">The composition points.</param>
        public ComponentInfo(InstanceInfo componentType, MethodID importingCtor, Import[] imports, Export[] exports, Export[] selfExports, CompositionPoint[] compositionPoints)
        {
            ComponentType = componentType;
            SelfExports = selfExports;
            Exports = exports;
            Imports = imports;
            ImportingConstructor = importingCtor;
            CompositionPoints = compositionPoints;

            //find dependency names
            var names = new HashSet<string>();
            names.Add(ComponentType.TypeName);
            foreach (var export in Exports)
            {
                names.Add(export.Contract);
                names.Add(export.ExportType.TypeName);
            }

            foreach (var export in SelfExports)
            {
                names.Add(export.Contract);
                names.Add(export.ExportType.TypeName);
            }

            foreach (var import in Imports)
            {
                names.Add(import.Contract);
                names.Add(import.ImportTypeInfo.ImportType.TypeName);
                names.Add(import.ImportTypeInfo.ItemType.TypeName);
            }

            DependencyNames = names.ToArray();
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return ComponentType.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var o = obj as ComponentInfo;
            if (o == null)
                return false;

            return ComponentType.Equals(o.ComponentType);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        /// <inheritdoc />
        public override string ToString()
        {
            return "[Component]" + ComponentType.TypeName;
        }
    }

    /// <summary>
    /// Item of exported metadata.
    /// </summary>
    public class MetaItem
    {
        /// <summary>
        /// Exporting key of meta item.
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Determine if item appeared with multiple indicator.
        /// IsMultiple values are stored in lists.
        /// </summary>
        public readonly bool IsMultiple;

        /// <summary>
        /// Exported metadata - note that only direct metadata
        /// can be used for composition.
        /// </summary>
        public readonly IEnumerable<object> Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaItem"/> class.
        /// </summary>
        /// <param name="key">The key of meta item.</param>
        /// <param name="isMultiple">if set to <c>true</c> it is multiple item.</param>
        /// <param name="items">The items.</param>
        public MetaItem(string key, bool isMultiple, IEnumerable<object> items)
        {
            Key = key;
            IsMultiple = isMultiple;
            Data = items.ToArray();
        }
    }

    /// <summary>
    /// Exported metadata.
    /// </summary>
    public class MetaExport
    {
        /// <summary>
        /// All exported metadata.
        /// </summary>
        private readonly Dictionary<string, MetaItem> _data;

        /// <summary>
        /// Keys that has been exported.
        /// </summary>
        /// <value>The exported keys.</value>
        public IEnumerable<string> ExportedKeys { get { return _data.Keys; } }

        /// <summary>
        /// Get <see cref="MetaItem" /> according to given key.
        /// </summary>
        /// <param name="key">Key of item.</param>
        /// <returns><see cref="MetaItem" /> if available for given key, <c>null</c> otherwise.</returns>
        public MetaItem GetItem(string key)
        {
            MetaItem result;
            _data.TryGetValue(key, out result);
            return result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaExport"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public MetaExport(IEnumerable<MetaItem> items)
        {
            _data = new Dictionary<string, MetaItem>();

            foreach (var item in items)
            {
                _data.Add(item.Key, item);
            }
        }
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

        /// <summary>
        /// Determine that export is inherited.
        /// </summary>
        public readonly bool IsInherited;

        /// <summary>
        /// Initializes a new instance of the <see cref="Export"/> class.
        /// </summary>
        /// <param name="exportType">Type of the export.</param>
        /// <param name="isInherited">if set to <c>true</c> export is inherited.</param>
        /// <param name="getter">The getter.</param>
        /// <param name="contract">The contract.</param>
        /// <param name="meta">The meta.</param>
        public Export(TypeDescriptor exportType, bool isInherited, MethodID getter, string contract, MetaExport meta)
        {
            ExportType = exportType;
            Contract = contract;
            Getter = getter;
            Meta = meta;
            IsInherited = isInherited;
        }
    }

    /// <summary>
    /// Parsed type of import.
    /// </summary>
    public class ImportTypeInfo
    {
        /// <summary>
        /// True, if ItemType is wrapped in lazy object.
        /// </summary>
        public readonly bool IsItemLazy;

        /// <summary>
        /// True if ImportType is wrapped in lazy object.
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportTypeInfo"/> class.
        /// </summary>
        /// <param name="importType">Type of the import.</param>
        /// <param name="itemType">Type of the imported item.</param>
        /// <exception cref="System.ArgumentNullException">importType</exception>
        private ImportTypeInfo(TypeDescriptor importType, TypeDescriptor itemType)
        {
            if (importType == null)
                throw new ArgumentNullException("importType");

            ImportType = importType;
            ItemType = itemType == null ? importType : itemType;

            IsItemLazy = itemType.TypeName.StartsWith("System.Lazy<");
            if (IsItemLazy)
            {
                var lazyTypeArguments = itemType.Arguments.ToArray();
                //item type wont contain 
                ItemType = lazyTypeArguments[0];

                if (lazyTypeArguments.Length > 1)
                {
                    //there is metadata type for lazy
                    MetaDataType = lazyTypeArguments[1];
                }
            }
        }

        /// <summary>
        /// In ManyImport there can be Array{}, IEnumerable{} or anything derived from ICollection{}.
        /// </summary>
        /// <param name="importManyType">Type of the import many.</param>
        /// <param name="allowMany">if set to <c>true</c> importing many items is allowed.</param>
        /// <param name="services">The services.</param>
        /// <returns>ImportTypeInfo.</returns>
        public static ImportTypeInfo ParseFromMany(TypeDescriptor importManyType, bool allowMany, TypeServices services)
        {
            if (allowMany)
            {
                var currentChain = services.GetChain(importManyType);
                if (currentChain == null)
                {
                    //TODO Log that there is missing chain info and allowMany cannot be parsed
                }
                else
                {
                    var itemType = findManyItemDescriptor(currentChain);
                    return new ImportTypeInfo(importManyType, itemType);
                }
            }
            return new ImportTypeInfo(importManyType, importManyType);
        }

        /// <summary>
        /// In ManyImport there can be Array{}, IEnumerable{} or anything derived from ICollection{}.
        /// </summary>
        /// <param name="importManyType">Type of the import many.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <returns>ImportTypeInfo.</returns>
        public static ImportTypeInfo ParseFromMany(TypeDescriptor importManyType, TypeDescriptor itemType)
        {
            return new ImportTypeInfo(importManyType, itemType);
        }

        #region ImportMany ItemType parsing

        /// <summary>
        /// Find ItemType from given type representing many imports.
        /// </summary>
        /// <param name="manyType">Many imports representation.</param>
        /// <returns>ItemType if found according to MEF rules, null otherwise.</returns>
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
        /// Find ItemType from given type representing many imports in ICollection{}.
        /// </summary>
        /// <param name="manyType">Many imports representation.</param>
        /// <returns>ItemType if found within ICollection according to MEF rules, null otherwise.</returns>
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
        /// Contract specified in Import attribute, or default contract according to import type.
        /// </summary>
        public readonly string Contract;

        /// <summary>
        /// Info about importing type, IsLazy,ItemType,...
        /// </summary>
        public readonly ImportTypeInfo ImportTypeInfo;

        /// <summary>
        /// Setter, which set instance to requested target
        /// is null, if import was obtained from importing constructor.
        /// </summary>
        public readonly MethodID Setter;

        /// <summary>
        /// Determine if this import has to be satisfied before instance constructing.
        /// </summary>
        /// <value><c>true</c> if this instance is prerequisity; otherwise, <c>false</c>.</value>
        public bool IsPrerequisity { get { return Setter == null; } }

        /// <summary>
        /// Determine if value can be default (no export needed).
        /// </summary>
        public readonly bool AllowDefault;

        /// <summary>
        /// Determine if import can accept more than one export.
        /// </summary>
        public readonly bool AllowMany;

        /// <summary>
        /// Initializes a new instance of the <see cref="Import"/> class.
        /// </summary>
        /// <param name="importTypeInfo">The import type information.</param>
        /// <param name="setter">The setter.</param>
        /// <param name="contract">The contract.</param>
        /// <param name="allowMany">if set to <c>true</c> [allow many].</param>
        /// <param name="allowDefault">if set to <c>true</c> [allow default].</param>
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

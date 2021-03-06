﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using MEFEditor.Analyzing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Builder helping with creating component info.
    /// </summary>
    public class ComponentInfoBuilder
    {
        /// <summary>
        /// Defined self exports.
        /// </summary>
        private readonly List<Export> _selfExports = new List<Export>();

        /// <summary>
        /// Defined exports.
        /// </summary>
        private readonly List<Export> _exports = new List<Export>();

        /// <summary>
        /// Defined imports.
        /// </summary>
        private readonly List<Import> _imports = new List<Import>();

        /// <summary>
        /// Defined explicit composition points, which has been marked with composition point attribute.
        /// </summary>
        private readonly List<CompositionPoint> _explicitCompositionPoints = new List<CompositionPoint>();

        /// <summary>
        /// Current meta exports.
        /// </summary>
        private readonly MultiDictionary<string, object> _metaData = new MultiDictionary<string, object>();

        /// <summary>
        /// Current info about meta multiple flag.
        /// </summary>
        private readonly Dictionary<string, bool> _metaMultiplicity = new Dictionary<string, bool>();

        /// <summary>
        /// Defined implicit composition point if available, <c>null</c> otherwise.
        /// </summary>
        private CompositionPoint _implicitCompositionPoint;

        /// <summary>
        /// Importing constructor if available.
        /// </summary>
        private MethodID _importingCtor;

        /// <summary>
        /// Type of component that is builded.
        /// </summary>
        public readonly TypeDescriptor ComponentType;

        /// <summary>
        /// Determine that no component info has been added yet.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Determine that component has any composition point.
        /// </summary>
        /// <value><c>true</c> if this instance has composition point; otherwise, <c>false</c>.</value>
        public bool HasCompositionPoint { get { return _explicitCompositionPoints.Count > 0 || _implicitCompositionPoint != null; } }

        /// <summary>
        /// Determine that component has importing constructor.
        /// </summary>
        /// <value><c>true</c> if this instance has importing ctor; otherwise, <c>false</c>.</value>
        public bool HasImportingCtor { get { return _importingCtor != null; } }

        /// <summary>
        /// Create ComponentInfoBuilder.
        /// </summary>
        /// <param name="componentType">Type of component that is builded.</param>
        public ComponentInfoBuilder(TypeDescriptor componentType)
        {
            ComponentType = componentType;
        }

        #region Exports building

        /// <summary>
        /// Add export of given type.
        /// </summary>
        /// <param name="exportType">Type of exported value.</param>
        /// <param name="propertyName">Name of exporting property.</param>
        /// <param name="isInherited">if set to <c>true</c> export is marked as inherited.</param>
        /// <param name="contract">Contract of export.</param>
        public void AddPropertyExport(TypeDescriptor exportType, string propertyName, bool isInherited = false, string contract = null)
        {
            var getterID = Naming.Method(ComponentType, Naming.GetterPrefix + propertyName, false, new ParameterTypeInfo[0]);
            AddExport(exportType, getterID, isInherited, contract);
        }

        /// <summary>
        /// Adds the meta data for next export.
        /// </summary>
        /// <param name="key">The item key.</param>
        /// <param name="data">The exported metadata.</param>
        /// <param name="isMultiple">if set to <c>true</c> it means that meta export is multiple.</param>
        public void AddMeta(string key, object data, bool isMultiple)
        {
            _metaData.Add(key, data);

            if (_metaMultiplicity.ContainsKey(key) && _metaMultiplicity[key])
                return;

            _metaMultiplicity[key] = isMultiple;
        }

        /// <summary>
        /// Add export of given type.
        /// </summary>
        /// <param name="exportType">Type of exported value.</param>
        /// <param name="getterID">Id of exporting method.</param>
        /// <param name="isInherited">Determine that export is marked as inherited.</param>
        /// <param name="contract">Contract of export.</param>
        public void AddExport(TypeDescriptor exportType, MethodID getterID, bool isInherited, string contract = null)
        {
            if (contract == null)
            {
                contract = exportType.TypeName;
            }

            var meta = buildMeta();
            var export = new Export(exportType, isInherited, getterID, contract, meta);
            AddExport(export);
        }

        /// <summary>
        /// Add given export to component info.
        /// </summary>
        /// <param name="export">Added export.</param>
        public void AddExport(Export export)
        {
            _exports.Add(export);
        }

        /// <summary>
        /// Add self export of component.
        /// </summary>
        /// <param name="isInherited">Determine that self export is marked as inherited.</param>
        /// <param name="contract">Contract of export.</param>
        public void AddSelfExport(bool isInherited, string contract)
        {
            var meta = buildMeta();
            var export = new Export(ComponentType, isInherited, null, contract, meta);

            _selfExports.Add(export);
        }

        #endregion

        #region Imports building

        /// <summary>
        /// Add import of given type.
        /// </summary>
        /// <param name="importType">Imported type.</param>
        /// <param name="setterName">Name of property.</param>
        public void AddPropertyImport(TypeDescriptor importType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            var importTypeInfo = ImportTypeInfo.ParseFromMany(importType, false, null);
            var contract = importTypeInfo.ItemType.TypeName;

            AddImport(importTypeInfo, setterID, contract);
        }

        /// <summary>
        /// Add import of given type.
        /// </summary>
        /// <param name="importTypeInfo">Info about imported type.</param>
        /// <param name="setterID">Importing setter.</param>
        /// <param name="contract">Contract of import.</param>
        /// <param name="allowMany">Determine many values can be imported.</param>
        /// <param name="allowDefault">if set to <c>true</c> [allow default].</param>
        public void AddImport(ImportTypeInfo importTypeInfo, MethodID setterID, string contract = null, bool allowMany = false, bool allowDefault = false)
        {
            if (contract == null)
            {
                contract = importTypeInfo.ItemType.TypeName;
            }

            var import = new Import(importTypeInfo, setterID, contract, allowMany, allowDefault);
            AddImport(import);
        }

        /// <summary>
        /// Add import of component.
        /// </summary>
        /// <param name="import">Added import.</param>
        public void AddImport(Import import)
        {
            _imports.Add(import);
        }

        /// <summary>
        /// Add many import of given type.
        /// </summary>
        /// <param name="importType">Type of the import.</param>
        /// <param name="itemType">Type of the item.</param>
        /// <param name="setterName">Name of the setter.</param>
        public void AddManyImport(TypeDescriptor importType, TypeDescriptor itemType, string setterName)
        {
            var setterID = getSetterID(importType, setterName);
            var importTypeInfo = ImportTypeInfo.ParseFromMany(importType, itemType);
            AddImport(importTypeInfo, setterID, null, true);
        }

        /// <summary>
        /// Set importing constructor of component.
        /// </summary>
        /// <param name="info">The information.</param>
        public void SetImportingCtor(TypeMethodInfo info)
        {
            _importingCtor = info.MethodID;
            foreach (var parameter in info.Parameters)
            {
                var type = parameter.Type;
                addPreImport(type);
            }
        }

        /// <summary>
        /// Set importing constructor of component.
        /// </summary>
        /// <param name="importingParameters">Parameters of import.</param>
        public void SetImportingCtor(params TypeDescriptor[] importingParameters)
        {
            var parameters = new ParameterTypeInfo[importingParameters.Length];

            for (int i = 0; i < parameters.Length; ++i)
            {
                var importType = importingParameters[i];
                parameters[i] = ParameterTypeInfo.Create("p", importType);

                addPreImport(importType);
            }

            _importingCtor = Naming.Method(ComponentType, Naming.CtorName, false, parameters);
        }
        #endregion

        /// <summary>
        /// Add explicit composition point of component.
        /// </summary>
        /// <param name="method">Composition point method.</param>
        /// <param name="initializer">The initializer.</param>
        public void AddExplicitCompositionPoint(MethodID method, GeneratorBase initializer = null)
        {
            _explicitCompositionPoints.Add(new CompositionPoint(ComponentType, method, true, initializer));
        }

        /// <summary>
        /// Add Implicit importing constructor of component.
        /// </summary>
        public void AddImplicitImportingConstructor()
        {
            if (_importingCtor != null)
                return;
            _importingCtor = getComponentParamLessCtorID();
        }

        /// <summary>
        /// Add Implicit composition point of component.
        /// </summary>
        public void AddImplicitCompositionPoint()
        {
            _implicitCompositionPoint = new CompositionPoint(ComponentType, getComponentParamLessCtorID(), false, null);
        }

        /// <summary>
        /// Build collected info into ComponentInfo with implicit ctor if required.
        /// </summary>
        /// <returns>Created ComponentInfo.</returns>
        public ComponentInfo BuildWithImplicitCtor()
        {
            if (_importingCtor == null)
            {
                //default importin constructor
                _importingCtor = getComponentParamLessCtorID();
            }

            return Build();
        }

        /// <summary>
        /// Build collected info into ComponentInfo.
        /// </summary>
        /// <returns>Created ComponentInfo.</returns>
        public ComponentInfo Build()
        {
            var compositionPoints = new List<CompositionPoint>(_explicitCompositionPoints);
            if (_implicitCompositionPoint != null)
            {
                compositionPoints.Add(_implicitCompositionPoint);
            }

            return new ComponentInfo(ComponentType, _importingCtor, _imports.ToArray(), _exports.ToArray(), _selfExports.ToArray(), compositionPoints.ToArray());
        }

        #region Private helpers

        /// <summary>
        /// Add prerequisity import of given type.
        /// </summary>
        /// <param name="type">Type of import.</param>
        private void addPreImport(TypeDescriptor type)
        {
            var importTypeInfo = ImportTypeInfo.ParseFromMany(type, false, null);
            var contract = importTypeInfo.ItemType.TypeName;
            var preImport = new Import(importTypeInfo, null, contract);
            _imports.Add(preImport);
        }

        /// <summary>
        /// Get id for setter of given property.
        /// </summary>
        /// <param name="propertyType">Type of property.</param>
        /// <param name="property">Property which setter is needed.</param>
        /// <returns>MethodID of desired property.</returns>
        private MethodID getSetterID(TypeDescriptor propertyType, string property)
        {
            var parameters = new ParameterTypeInfo[] { ParameterTypeInfo.Create("value", propertyType) };
            var setterID = Naming.Method(ComponentType, Naming.SetterPrefix + property, false, parameters);
            return setterID;
        }

        /// <summary>
        /// Gets the component parameter less ctor identifier.
        /// </summary>
        /// <returns>MethodID.</returns>
        private MethodID getComponentParamLessCtorID()
        {
            return Naming.Method(ComponentType, Naming.CtorName, false);
        }

        /// <summary>
        /// Builds the meta.
        /// </summary>
        /// <returns>MetaExport.</returns>
        private MetaExport buildMeta()
        {
            var items = new List<MetaItem>();
            foreach (var key in _metaData.Keys)
            {
                var values = _metaData.Get(key);
                var isMultiple = _metaMultiplicity[key];
                var item = new MetaItem(key, isMultiple, values);

                items.Add(item);
            }

            _metaData.Clear();
            _metaMultiplicity.Clear();

            var meta = new MetaExport(items);
            return meta;
        }

        #endregion
    }
}

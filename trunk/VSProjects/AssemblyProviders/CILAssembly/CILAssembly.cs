using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition;

using Mono.Cecil;

using Analyzing;
using TypeSystem;

using AssemblyProviders.CIL;
using AssemblyProviders.CIL.Providing;

namespace AssemblyProviders.CILAssembly
{
    /// <summary>
    /// Assembly provider implementation for CIL assemblies loaded from files.
    /// </summary>
    public class CILAssembly : AssemblyProvider
    {
        /// <summary>
        /// Full path of represented assembly
        /// </summary>
        private readonly string _fullPath;

        /// <summary>
        /// Type builder used for translating TypeReferences into TypeDescriptors at assembly scope (no substitutions)
        /// </summary>
        private readonly TypeReferenceHelper _typeBuilder = new TypeReferenceHelper();

        /// <summary>
        /// Represented assembly
        /// </summary>
        private readonly AssemblyDefinition _assembly;

        /// <summary>
        /// Storage where available namespaces are stored
        /// </summary>
        private readonly NamespaceStorage _namespaces = new NamespaceStorage();

        /// <summary>
        /// Create CIL assembly provider from file loaded from given file. If loading fails, appropriate exception is thrown.
        /// </summary>
        /// <param name="assemblyPath"></param>
        public CILAssembly(string assemblyPath)
        {
            _fullPath = Path.GetFullPath(assemblyPath);

            //TODO use correct resolvers
            //probably resolvers are not needed and all desired functionality will be
            //gained via using TypeSystems resolving
            var pars = new ReaderParameters();
            var resolver = new DefaultAssemblyResolver();

            pars.AssemblyResolver = resolver;

            _assembly = AssemblyDefinition.ReadAssembly(_fullPath, pars);

            OnTypeSystemInitialized += initializeAssembly;
        }

        #region Assembly initialization routines

        /// <summary>
        /// Initialize assembly
        /// </summary>
        private void initializeAssembly()
        {
            hookChangesHandler();
            initializeReferences();
            scanNamespaces();
        }

        /// <summary>
        /// Hook handler that will recieve change events in project
        /// </summary>
        private void hookChangesHandler()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Scan namespaces within assembly - because of iterators prunning
        /// </summary>
        private void scanNamespaces()
        {
            foreach (var type in _assembly.MainModule.GetTypes())
            {
                _namespaces.Insert(type.FullName);
            }
        }

        /// <summary>
        /// Set references according to project referencies
        /// </summary>
        private void initializeReferences()
        {
            StartTransaction("Collecting references");

            try
            {
                addReferences();
            }
            finally
            {
                CommitTransaction();
            }
        }

        /// <summary>
        /// Add references to current assembly
        /// </summary>
        private void addReferences()
        {
            foreach (var reference in _assembly.MainModule.AssemblyReferences)
            {
                //TODO find path of assembly

                /*
                var refAssembly = _assembly.MainModule.AssemblyResolver.Resolve(reference);
                var fullPath = refAssembly.MainModule.FullyQualifiedName;

                AddReference(fullPath);*/
            }
        }


        #endregion

        #region Components handling

        /// <summary>
        /// Search components defined in _assembly and report them.
        /// </summary>
        protected override void loadComponents()
        {
            foreach (var type in _assembly.MainModule.GetTypes())
            {
                if (isComponent(type))
                {
                    var info = createComponentInfo(type);
                    ComponentDiscovered(info);
                }
            }
        }


        /// <summary>
        /// Determine that given type defines component
        /// </summary>
        /// <param name="type">Tested type</param>
        /// <returns>True if type defines component, false otherwise</returns>
        private bool isComponent(TypeDefinition type)
        {
            foreach (var attribute in type.CustomAttributes)
            {
                if (isComponentAttribute(attribute))
                {
                    //self export defined
                    return true;
                }
            }

            foreach (var method in type.Methods)
            {
                foreach (var attribute in method.CustomAttributes)
                {
                    //importing constructor
                    if (isComponentAttribute(attribute))
                    {
                        return true;
                    }
                }
            }

            foreach (var property in type.Properties)
            {
                foreach (var attribute in property.CustomAttributes)
                {
                    //importing/exporting property
                    if (isComponentAttribute(attribute))
                    {
                        return true;
                    }
                }
            }

            foreach (var field in type.Fields)
            {
                //importing/exporting field
                foreach (var attribute in field.CustomAttributes)
                {
                    if (isComponentAttribute(attribute))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Create component info for given type
        /// </summary>
        /// <param name="componentType">type that will create component</param>
        /// <returns>Created component info</returns>
        private ComponentInfo createComponentInfo(TypeDefinition componentType)
        {
            var componentDescriptor = getDescriptor(componentType);
            var infoBuilder = new ComponentInfoBuilder(componentDescriptor);

            reportSelfExports(componentType, infoBuilder);
            reportComponentMethods(componentType, infoBuilder);
            reportComponentFields(componentType, infoBuilder);
            reportComponentProperties(componentType, infoBuilder);

            return infoBuilder.BuildInfo();
        }

        /// <summary>
        /// Report self exports defined on componentType
        /// </summary>
        /// <param name="componentType">Type of component which self exports are reported</param>
        /// <param name="infoBuilder">Builder where self exports will be added</param>
        private void reportSelfExports(TypeDefinition componentType, ComponentInfoBuilder infoBuilder)
        {
            foreach (var attribute in componentType.CustomAttributes)
            {
                var fullname = attribute.AttributeType.FullName;

                if (fullname == Naming.ExportAttribute)
                {
                    var contract = getExportContract(attribute, infoBuilder.ComponentType);
                    infoBuilder.AddSelfExport(contract);
                }
            }
        }

        /// <summary>
        /// Report composition related fields defined by component type into infoBuilder
        /// </summary>
        /// <param name="componentType">Type defining component</param>
        /// <param name="infoBuilder">Builder where imports/exports are reported</param>
        private void reportComponentFields(TypeDefinition componentType, ComponentInfoBuilder infoBuilder)
        {
            foreach (var field in componentType.Fields)
            {
                //importing/exporting field
                foreach (var attribute in field.CustomAttributes)
                {
                    var fullname = attribute.AttributeType.FullName;

                    if (fullname == Naming.ExportAttribute)
                    {
                        addExport(infoBuilder, field, attribute);
                    }
                    else if (fullname == Naming.ImportAttribute)
                    {
                        addImport(infoBuilder, field, attribute, false);
                    }
                    else if (fullname == Naming.ImportManyAttribute)
                    {
                        addImport(infoBuilder, field, attribute, true);
                    }
                }
            }
        }

        /// <summary>
        /// Report composition related properties defined by component type into infoBuilder
        /// </summary>
        /// <param name="componentType">Type defining component</param>
        /// <param name="infoBuilder">Builder where imports/exports are reported</param>
        private void reportComponentProperties(TypeDefinition componentType, ComponentInfoBuilder infoBuilder)
        {
            foreach (var property in componentType.Properties)
            {
                //importing/exporting property
                foreach (var attribute in property.CustomAttributes)
                {
                    var fullname = attribute.AttributeType.FullName;
                    var setterId = getMethodId(infoBuilder.ComponentType, property.SetMethod);

                    if (fullname == Naming.ExportAttribute)
                    {
                        var getterId = getMethodId(infoBuilder.ComponentType, property.GetMethod);
                        addExport(infoBuilder, property.GetMethod, getterId, attribute);
                    }
                    else if (fullname == Naming.ImportAttribute)
                    {
                        addImport(infoBuilder, property.SetMethod, setterId, attribute, false);
                    }
                    else if (fullname == Naming.ImportManyAttribute)
                    {
                        addImport(infoBuilder, property.SetMethod, setterId, attribute, true);
                    }
                }
            }
        }

        /// <summary>
        /// Report composition related methods defined by component type into infoBuilder
        /// </summary>
        /// <param name="componentType">Type defining component</param>
        /// <param name="infoBuilder">Builder where methods are reported</param>
        private void reportComponentMethods(TypeDefinition componentType, ComponentInfoBuilder infoBuilder)
        {
            //TODO add implicit importing constructor if needed

            foreach (var method in componentType.Methods)
            {
                foreach (var attribute in method.CustomAttributes)
                {
                    var fullname = attribute.AttributeType.FullName;
                    var methodId = getMethodId(infoBuilder.ComponentType, method);

                    //importing constructor  
                    if (fullname == Naming.CompositionPointAttribute)
                    {
                        //composition point
                        addCompositionPoint(infoBuilder, methodId, attribute);
                    }
                }
            }
        }

        /// <summary>
        /// Add composition point into infoBuilder
        /// </summary>
        /// <param name="infoBuilder">Info builder where export will be added</param>
        /// <param name="methodId">MethodID of composition point</param>
        /// <param name="attribute">Attribute defining composition point</param>
        private void addCompositionPoint(ComponentInfoBuilder infoBuilder, MethodID methodId, CustomAttribute attribute)
        {
            //TODO add composition point arguments
            infoBuilder.AddExplicitCompositionPoint(methodId, null);
        }

        /// <summary>
        /// Add export method into infoBuilder
        /// </summary>
        /// <param name="infoBuilder">Info builder where export will be added</param>
        /// <param name="field">Exported field</param>
        /// <param name="attribute">Attribute defining export</param>
        private void addExport(ComponentInfoBuilder infoBuilder, FieldDefinition field, CustomAttribute attribute)
        {
            var getter = buildAutoGetter(infoBuilder.ComponentType, field);

            var exportType = getter.Info.Parameters[0].Type;
            var contract = getExportContract(attribute, exportType);

            infoBuilder.AddExport(exportType, getter.Info.MethodID, contract);
        }

        /// <summary>
        /// Add export method into infoBuilder
        /// </summary>
        /// <param name="infoBuilder">Info builder where export will be added</param>
        /// <param name="method">Export method</param>
        /// <param name="methodId">Id of export method</param>
        /// <param name="attribute">Attribute defining export</param>
        private void addExport(ComponentInfoBuilder infoBuilder, MethodDefinition method, MethodID methodId, CustomAttribute attribute)
        {
            var exportType = getDescriptor(method.ReturnType);
            var contract = getExportContract(attribute, exportType);

            infoBuilder.AddExport(exportType, methodId, contract);
        }

        /// <summary>
        /// Add import method into infoBuilder
        /// </summary>
        /// <param name="infoBuilder">Info builder where import will be added</param>
        /// <param name="field">Imported field</param>
        /// <param name="attribute">Attribute defining import</param>
        private void addImport(ComponentInfoBuilder infoBuilder, FieldDefinition field, CustomAttribute attribute, bool isManyImport)
        {
            var setter = buildAutoSetter(infoBuilder.ComponentType, field);

            var importType = setter.Info.Parameters[0].Type;
            var importTypeInfo = getImportTypeInfo(importType, isManyImport);
            var contract = getImportContract(attribute, importType);
            var allowDefault = getAllowDefault(attribute);

            infoBuilder.AddImport(importTypeInfo, setter.Info.MethodID, contract, isManyImport, allowDefault);
        }

        /// <summary>
        /// Add import method into infoBuilder
        /// </summary>
        /// <param name="infoBuilder">Info builder where import will be added</param>
        /// <param name="method">Import method</param>
        /// <param name="methodId">Id of import method</param>
        /// <param name="attribute">Attribute defining import</param>
        private void addImport(ComponentInfoBuilder infoBuilder, MethodDefinition method, MethodID methodId, CustomAttribute attribute, bool isManyImport)
        {
            var importType = getDescriptor(method.Parameters[0].ParameterType);
            var importTypeInfo = getImportTypeInfo(importType, isManyImport);
            var contract = getImportContract(attribute, importType);
            var allowDefault = getAllowDefault(attribute);

            infoBuilder.AddImport(importTypeInfo, methodId, contract, isManyImport, allowDefault);
        }

        /// <summary>
        /// Determine that given attribute defines component
        /// </summary>
        /// <param name="attribute">Tested attribute</param>
        /// <returns>True if attribute define component, false otherwise</returns>
        private bool isComponentAttribute(CustomAttribute attribute)
        {
            var fullname = attribute.AttributeType.FullName;

            return
                fullname == Naming.ExportAttribute ||
                fullname == Naming.ImportAttribute ||
                fullname == Naming.ImportManyAttribute ||
                fullname == Naming.CompositionPointAttribute;
        }

        /// <summary>
        /// Creates type info for given import type
        /// </summary>
        /// <param name="importType">Type of import</param>
        /// <param name="isManyImport">Determine that import allows importing multiple items</param>
        /// <returns>Created ImportTypeInfo</returns>
        private ImportTypeInfo getImportTypeInfo(TypeDescriptor importType, bool isManyImport)
        {
            return ImportTypeInfo.ParseFromMany(importType, isManyImport, TypeServices);
        }

        /// <summary>
        /// Get contract defined by given exportAttribute
        /// </summary>
        /// <param name="exportAttribute">Attribute where export contract is defined</param>
        /// <param name="defaultContract">Describe type which is used as default contract</param>
        /// <returns>Contract for given export attribute</returns>
        private string getExportContract(CustomAttribute exportAttribute, TypeDescriptor defaultContract)
        {
            //syntactically same as getImportContract, but semantically different
            switch (exportAttribute.ConstructorArguments.Count)
            {
                case 0:
                    return defaultContract.TypeName;
                case 2:
                //TODO what is ContractType good for ?
                case 1:
                    return resolveContract(exportAttribute.ConstructorArguments[0]);

                default:
                    throw new NotSupportedException("Unknown export attribute constructor with argument count: " + exportAttribute.ConstructorArguments.Count);
            }
        }

        /// <summary>
        /// Get contract defined by given importAttribute
        /// </summary>
        /// <param name="importAttribute">Attribute where import contract is defined</param>
        /// <param name="defaultContract">Describe type which is used as default contract</param>
        /// <returns>Contract for given import attribute</returns>
        private string getImportContract(CustomAttribute importAttribute, TypeDescriptor defaultContract)
        {
            //syntactically same as getExportContract, but semantically different
            switch (importAttribute.ConstructorArguments.Count)
            {
                case 0:
                    return defaultContract.TypeName;
                case 2:
                //TODO what is ContractType good for ?
                case 1:
                    return resolveContract(importAttribute.ConstructorArguments[0]);

                default:
                    throw new NotSupportedException("Unknown import attribute constructor with argument count: " + importAttribute.ConstructorArguments.Count);
            }
        }

        /// <summary>
        /// Get indicator that import allow default value
        /// </summary>
        /// <param name="importAttribute">Attribute where allow default is defined</param>
        /// <returns>AllowDefault indicator for given import attribute</returns>
        private bool getAllowDefault(CustomAttribute importAttribute)
        {
            foreach (var property in importAttribute.Properties)
            {
                if (property.Name == "AllowDefault")
                {
                    return (bool)property.Argument.Value;
                }
            }

            return false;
        }


        /// <summary>
        /// Resolve contract from given attribute argument
        /// </summary>
        /// <param name="contractArgument">Attribute defining contract</param>
        /// <returns>Resolved contract</returns>
        private string resolveContract(CustomAttributeArgument contractArgument)
        {
            var argumentType = contractArgument.Type.FullName;
            switch (argumentType)
            {
                case "System.String":
                    return contractArgument.Value as string;
                default:
                    //TODO add type argument support
                    throw new NotImplementedException("Missing support for contract argument of type: " + argumentType);
            }
        }

        #endregion

        #region Method building

        /// <summary>
        /// Create complete method info for given method definition
        /// </summary>
        /// <param name="declaringType">Type where method is declared</param>
        /// <param name="method">Method which info is retrieved</param>
        /// <returns>Created method info</returns>
        internal TypeMethodInfo CreateMethodInfo(TypeDescriptor declaringType, MethodDefinition method)
        {
            return CILInstruction.CreateMethodInfo(method, method.IsAbstract);
        }

        /// <summary>
        /// Creates method item from given method definition.
        /// Static constructors checking is proceeded
        /// </summary>
        /// <param name="method">Method definition</param>
        /// <returns>Created method item</returns>
        private MethodItem createItem(TypeDescriptor declaringType, MethodDefinition method)
        {
            //TODO cache results

            var methodInfo = CreateMethodInfo(declaringType, method);

            //Generics is resolved via correct naming conventions and universal CILGenerator
            var item = new MethodItem(new CILGenerator(method, methodInfo, TypeServices), methodInfo);

            return item;
        }

        /// <summary>
        /// Build getter method providing field value
        /// </summary>
        /// <param name="declaringType">Type where getter is defined</param>
        /// <param name="field">Field which value is provided</param>
        /// <returns>Builded method</returns>
        private MethodItem buildAutoGetter(TypeDescriptor declaringType, FieldDefinition field)
        {
            //TODO caching is needed because of components

            var fieldName = field.Name;
            var isStatic = field.IsStatic;
            var fieldType = getDescriptor(field.FieldType);

            var getter = new TypeMethodInfo(declaringType,
                "get_" + fieldName, fieldType,
                new ParameterTypeInfo[0], isStatic, TypeDescriptor.NoDescriptors
                );

            var getItem = new MethodItem(new GetterGenerator(fieldName), getter);

            return getItem;
        }

        /// <summary>
        /// Build setter method setting field value
        /// </summary>
        /// <param name="declaringType">Type where setter is defined</param>
        /// <param name="field">Field which value is set</param>
        /// <returns>Builded method</returns>
        private MethodItem buildAutoSetter(TypeDescriptor declaringType, FieldDefinition field)
        {
            //TODO caching is needed because of components

            var fieldName = field.Name;
            var isStatic = field.IsStatic;
            var fieldType = getDescriptor(field.FieldType);

            var setter = new TypeMethodInfo(declaringType,
                "set_" + fieldName, TypeDescriptor.Void,
                new ParameterTypeInfo[]{
                    ParameterTypeInfo.Create("value",fieldType)
                    }, isStatic, TypeDescriptor.NoDescriptors
                );

            var setItem = new MethodItem(new SetterGenerator(fieldName), setter);

            return setItem;
        }

        /// <summary>
        /// Build constructor methods for given type
        /// </summary>
        /// <param name="type">Type which constructors are builded</param>
        /// <returns>Builded methods</returns>
        private IEnumerable<MethodItem> buildConstructors(TypeDefinition type)
        {
            var info = getDescriptor(type);

            foreach (var method in type.Methods)
            {
                if (method.Name == ".ctor")
                {
                    yield return createItem(info, method);
                }
            }
        }

        /// <summary>
        /// Build method for static initializer for given type
        /// </summary>
        /// <param name="type">Type which initializer is builded</param>
        /// <returns>Builded method</returns>
        private MethodItem buildStaticInitilizer(TypeDefinition type)
        {
            var info = getDescriptor(type);

            foreach (var method in type.Methods)
            {
                if (method.Name == ".cctor")
                    return createItem(info, method);
            }

            //if no explicit cctor is found default one is created
            var initializerId = TypeServices.Settings.GetSharedInitializer(info);

            //add default implementation
            var methodInfo = new TypeMethodInfo(
                info, Naming.GetMethodName(initializerId), TypeDescriptor.Void,
                new ParameterTypeInfo[0], false, TypeDescriptor.NoDescriptors, false
                );

            var item = new MethodItem(new CILGenerator(null, methodInfo, TypeServices), methodInfo);

            return item;
        }

        /// <summary>
        /// Get method ID for given method definition
        /// </summary>
        /// <param name="method">Method which id is retrieved</param>
        /// <param name="declaringType">Type where method is declared</param>
        /// <returns>Method ID of given method definition</returns>
        private MethodID getMethodId(TypeDescriptor declaringType, MethodDefinition method)
        {
            var methodItem = createItem(declaringType, method);

            return methodItem.Info.MethodID;
        }

        #endregion

        #region Method searching

        /// <summary>
        /// Get methods defined on type with given typeFullName
        /// </summary>
        /// <param name="typeFullName">FullName of type where method is searched (In Mono.Cecil notation)</param>
        /// <param name="searchedMethodName">Name of method that is searched</param>
        /// <returns>All methods defined on given type with corresponding name</returns>
        internal IEnumerable<MethodItem> GetMethods(string typeFullName, string searchedMethodName)
        {
            var foundType = getType(typeFullName);

            return GetMethods(foundType, searchedMethodName);
        }

        /// <summary>
        /// Get methods defined on type with given typeFullName
        /// </summary>
        /// <param name="typeFullName">FullName of type where method is searched (In Mono.Cecil notation)</param>
        /// <param name="searchedMethodName">Name of method that is searched</param>
        /// <returns>All methods defined on given type with corresponding name</returns>
        internal IEnumerable<MethodItem> GetMethods(TypeDefinition type, string searchedMethodName)
        {
            if (type != null)
            {
                switch (searchedMethodName)
                {
                    case Naming.CtorName:
                        //usual type constructors
                        foreach (var ctor in buildConstructors(type))
                        {
                            yield return ctor;
                        }
                        break;

                    case Naming.ClassCtorName:
                        //static type constructors
                        yield return buildStaticInitilizer(type);
                        break;

                    default:
                        //usual method defined on type
                        var typeDescriptor = getDescriptor(type);

                        foreach (var method in type.Methods)
                        {
                            if (method.Name == searchedMethodName)
                                yield return createItem(typeDescriptor, method);
                        }

                        //wrapping fields into properties
                        foreach (var field in type.Fields)
                        {
                            if ("get_" + field.Name == searchedMethodName)
                                yield return buildAutoGetter(typeDescriptor, field);

                            if ("set_" + field.Name == searchedMethodName)
                                yield return buildAutoSetter(typeDescriptor, field);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Get method according to given ID. Works only for non-generic methods.
        /// </summary>
        /// <param name="methodID">ID of method that is searched</param>
        /// <returns>Found method, or null if desired method isn't found</returns>
        internal MethodItem GetMethod(MethodID methodID)
        {
            //TODO caching here will have great performance benefit 

            var typeFullname = Naming.GetDeclaringType(methodID);
            var methodName = Naming.GetMethodName(methodID);

            //typeFullname doesn't contain generic's - it doesn't
            //have to be translated
            var methods = GetMethods(typeFullname, methodName);
            foreach (var method in methods)
            {
                if (method.Info.MethodID.Equals(methodID))
                    return method;
            }

            return null;
        }

        /// <summary>
        /// Get substituted method according to given ID. Works only for generic methods.
        /// </summary>
        /// <param name="methodID">ID of method that is searched</param>
        /// <param name="substitutionInfo">Path info that is used for subtitution of generic arguments.</param>
        /// <returns>Found method, or null if desired method isn't found</returns>
        internal MethodItem GetGenericMethod(MethodID methodID, PathInfo substitutionInfo)
        {
            //find declaring type and method name
            var typeName = Naming.GetDeclaringType(methodID);
            var typePath = new PathInfo(typeName);

            var methodName = Naming.GetMethodName(methodID);
            var namePath = new PathInfo(methodName);

            if (namePath.HasGenericArguments)
                //TODO proper generic translation
                methodName = namePath.ShortSignature;

            //name of type is translated before search
            var type = getType(typePath);
            var methods = GetMethods(type, methodName);
            foreach (var method in methods)
            {
                //make generic method according to substitution info
                var generic = method.Make(substitutionInfo);

                if (generic.Info.MethodID.Equals(methodID))
                    return generic;
            }

            return null;
        }

        /// <summary>
        /// Determine that types prefixed with given path may be available within assembly
        /// <remarks>It is used for prunning hash iterators</remarks>
        /// </summary>
        /// <param name="path">Path which is tested</param>
        /// <returns><c>true</c> if it may be included, <c>false</c> otherwise</returns>
        internal bool MayInclude(string path)
        {
            return _namespaces.CanContains(path);
        }

        #endregion

        #region Type translation methods

        /// <summary>
        /// Build type descriptor from given type reference. No substitutions are resolved.
        /// </summary>
        /// <param name="type">Type reference which descriptor is builded</param>
        /// <returns>Builded type descriptor</returns>
        private TypeDescriptor getDescriptor(TypeReference type)
        {
            return _typeBuilder.BuildDescriptor(type);
        }

        /// <summary>
        /// Find type definition in represented assembly according to typePath.
        /// Translation into mono cecil format is provided
        /// </summary>
        /// <param name="typePath">Path where to search for definition</param>
        /// <returns>Found type definition or null if type doesn't exists</returns>
        private TypeDefinition getType(PathInfo typePath)
        {
            var typeFullname = typePath.Name;

            if (typePath.HasGenericArguments)
                //translate type into mono cecil format
                typeFullname = string.Format("{0}`{1}", typePath.ShortSignature, typePath.GenericArgs.Count);

            return getType(typeFullname);
        }

        /// <summary>
        /// Find type according to type fullname.
        /// </summary>
        /// <param name="fullname">Fullname in Mono.Cecil notation</param>
        /// <returns>Found type or null if type doesn't exists</returns>
        private TypeDefinition getType(string fullname)
        {
            return _assembly.MainModule.GetType(fullname);
        }

        #endregion

        #region Type operations

        /// <summary>
        /// Creates inheritance chain for given type.
        /// </summary>
        /// <param name="type">Type of desired inheritnace chain</param>
        /// <returns>Created inheritnace chain</returns>
        private InheritanceChain createChain(TypeDefinition type)
        {
            //caching is provided outside assembly

            //firstly we will collect all sub chains from interfaces
            var subChains = new List<InheritanceChain>();
            foreach (var iface in type.Interfaces)
            {
                var descriptor = getDescriptor(iface);
                var subChain = TypeServices.GetChain(descriptor);

                subChains.Add(subChain);
            }

            //subchain from base class
            var subTypeDescriptor = getDescriptor(type.BaseType);

            if (subTypeDescriptor != null)
            {
                var subTypeChain = TypeServices.GetChain(subTypeDescriptor);
                subChains.Add(subTypeChain);
            }

            var typeDescriptor = getDescriptor(type);
            return TypeServices.CreateChain(typeDescriptor, subChains);
        }

        #endregion

        #region Assembly provider implementation

        ///<inheritdoc />
        protected override string getAssemblyFullPath()
        {
            return _fullPath;
        }

        ///<inheritdoc />
        protected override string getAssemblyName()
        {
            //assembly name is determined by name found in compiled assembly
            return _assembly.Name.Name;
        }

        ///<inheritdoc />
        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            //Try to find given method
            var methodItem = GetMethod(method);

            if (methodItem == null)
                //method hasn't been found
                return null;

            return methodItem.Generator;
        }

        ///<inheritdoc />
        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            //Try to find given generic method
            var methodItem = GetGenericMethod(method, searchPath);

            if (methodItem == null)
                //method hasn't been found
                return null;

            return methodItem.Generator;
        }

        ///<inheritdoc />
        public override SearchIterator CreateRootIterator()
        {
            return new TypeModuleIterator(this);
        }

        ///<inheritdoc />
        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo, out TypeDescriptor alternativeImplementer)
        {
            alternativeImplementer = null;

            //we have info about type, where desired method is implemented
            var implementedMethod = Naming.ChangeDeclaringType(dynamicInfo.TypeName, method, false);

            var result = GetMethod(implementedMethod);
            if (result == null)
                //implementation hasn't been found
                return null;

            return result.Info.MethodID;
        }

        ///<inheritdoc />
        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath, out PathInfo alternativeImplementer)
        {
            alternativeImplementer = null;

            //we have info about type, where desired method is implemented
            var implementedMethod = Naming.ChangeDeclaringType(implementingTypePath.Name, methodID, false);
            var path = Naming.GetMethodPath(implementedMethod);

            var result = GetGenericMethod(implementedMethod, path);
            if (result == null)
                //implementation hasn't been found
                return null;

            return result.Info.MethodID;
        }

        ///<inheritdoc />
        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            var type = getType(typePath);

            if (type == null)
                //searched wasn't found in assembly
                return null;

            //caching is provided outside of assembly
            var chain = createChain(type);
            if (chain.Type.HasParameters)
                chain = chain.MakeGeneric(typePath.GenericArgs);

            return chain;
        }
        #endregion
    }
}
